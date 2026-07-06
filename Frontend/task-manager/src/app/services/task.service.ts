import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { API_BASE_URL } from '../api';
import { CreateTaskRequest, TaskDto } from '../models';

/**
 * Sends commands to the API. Reads come through the SignalR
 * subscription in RealtimeService, not through HTTP.
 */
@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/api/tasks`;

  createTask(request: CreateTaskRequest): Promise<TaskDto> {
    return firstValueFrom(this.http.post<TaskDto>(this.baseUrl, request));
  }

  completeTask(id: string, completedDescription: string): Promise<TaskDto> {
    return firstValueFrom(
      this.http.post<TaskDto>(`${this.baseUrl}/${id}/complete`, { completedDescription })
    );
  }

  reorderTask(id: string, newOrder: number): Promise<TaskDto[]> {
    return firstValueFrom(
      this.http.post<TaskDto[]>(`${this.baseUrl}/${id}/reorder`, { newOrder })
    );
  }

  deleteTask(id: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${this.baseUrl}/${id}`));
  }
}
