import { Injectable, inject, signal } from '@angular/core';
import {
  HttpTransportType,
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from '@microsoft/signalr';
import { API_BASE_URL } from '../api';
import { TaskDto } from '../models';
import { AuthService } from './auth.service';

/**
 * Real-time view of the user's tasks. The server pushes the full task list
 * over SignalR on connect and after every change; components just read the signal.
 */
@Injectable({ providedIn: 'root' })
export class RealtimeService {
  private readonly auth = inject(AuthService);
  private connection: HubConnection | null = null;

  readonly tasks = signal<TaskDto[]>([]);
  readonly connected = signal(false);

  async connect(): Promise<void> {
    if (this.connection) {
      return;
    }

    const connection = new HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/tasks`, {
        accessTokenFactory: () => this.auth.token ?? '',
        transport: HttpTransportType.WebSockets,
        skipNegotiation: true,
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on('TasksUpdated', (tasks: TaskDto[]) => this.tasks.set(tasks));
    connection.onreconnected(() => this.connected.set(true));
    connection.onreconnecting(() => this.connected.set(false));
    connection.onclose(() => this.connected.set(false));

    this.connection = connection;
    await connection.start();
    this.connected.set(true);
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
    this.connected.set(false);
    this.tasks.set([]);
  }
}
