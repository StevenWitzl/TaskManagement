import { DatePipe } from '@angular/common';
import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PRIORITY_LABELS, Priority, TaskDto } from '../../models';
import { AuthService } from '../../services/auth.service';
import { RealtimeService } from '../../services/realtime.service';
import { TaskService } from '../../services/task.service';

@Component({
  selector: 'app-tasks',
  imports: [FormsModule, DatePipe],
  templateUrl: './tasks.html',
  styleUrl: './tasks.css',
})
export class Tasks implements OnInit, OnDestroy {
  private readonly auth = inject(AuthService);
  private readonly realtime = inject(RealtimeService);
  private readonly taskService = inject(TaskService);
  private readonly router = inject(Router);

  readonly Priority = Priority;
  readonly priorityLabels = PRIORITY_LABELS;

  // Fed exclusively by the SignalR subscription
  readonly tasks = this.realtime.tasks;
  readonly connected = this.realtime.connected;
  readonly email = this.auth.email;

  readonly openTasks = computed(() => this.tasks().filter((t) => !t.completedDate));
  readonly completedTasks = computed(() => this.tasks().filter((t) => t.completedDate));

  readonly error = signal<string | null>(null);
  readonly completingTaskId = signal<string | null>(null);

  // Add-task form
  title = '';
  description = '';
  priority = Priority.Medium;

  completionDescription = '';

  async ngOnInit(): Promise<void> {
    try {
      await this.realtime.connect();
    } catch {
      this.error.set('Could not connect to the live task feed. Is the backend running?');
    }
  }

  async ngOnDestroy(): Promise<void> {
    await this.realtime.disconnect();
  }

  async addTask(): Promise<void> {
    if (!this.title.trim() || !this.description.trim()) {
      this.error.set('Title and description are required.');
      return;
    }

    await this.run(async () => {
      await this.taskService.createTask({
        title: this.title,
        description: this.description,
        priority: Number(this.priority),
      });
      this.title = '';
      this.description = '';
      this.priority = Priority.Medium;
    });
  }

  startCompleting(task: TaskDto): void {
    this.completingTaskId.set(task.id);
    this.completionDescription = '';
  }

  cancelCompleting(): void {
    this.completingTaskId.set(null);
  }

  async confirmComplete(task: TaskDto): Promise<void> {
    if (!this.completionDescription.trim()) {
      this.error.set('Please describe how the task was completed.');
      return;
    }

    await this.run(async () => {
      await this.taskService.completeTask(task.id, this.completionDescription);
      this.completingTaskId.set(null);
    });
  }

  async move(task: TaskDto, delta: number): Promise<void> {
    await this.run(() => this.taskService.reorderTask(task.id, task.order + delta));
  }

  async remove(task: TaskDto): Promise<void> {
    await this.run(() => this.taskService.deleteTask(task.id));
  }

  logout(): void {
    this.realtime.disconnect();
    this.auth.logout();
    this.router.navigate(['/']);
  }

  private async run(action: () => Promise<unknown>): Promise<void> {
    this.error.set(null);
    try {
      await action();
    } catch (err: unknown) {
      const httpError = err as { error?: { message?: string } };
      this.error.set(httpError?.error?.message ?? 'Something went wrong. Please try again.');
    }
  }
}
