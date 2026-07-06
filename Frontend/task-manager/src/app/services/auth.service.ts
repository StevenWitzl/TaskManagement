import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { API_BASE_URL } from '../api';
import { AuthResponse } from '../models';

const STORAGE_KEY = 'taskmanagement.auth';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly session = signal<AuthResponse | null>(this.restoreSession());

  readonly isAuthenticated = computed(() => this.session() !== null);
  readonly email = computed(() => this.session()?.email ?? null);

  get token(): string | null {
    return this.session()?.token ?? null;
  }

  async login(email: string, password: string): Promise<void> {
    const response = await firstValueFrom(
      this.http.post<AuthResponse>(`${API_BASE_URL}/api/auth/login`, { email, password })
    );
    this.storeSession(response);
  }

  async register(email: string, password: string): Promise<void> {
    const response = await firstValueFrom(
      this.http.post<AuthResponse>(`${API_BASE_URL}/api/auth/register`, { email, password })
    );
    this.storeSession(response);
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEY);
    this.session.set(null);
  }

  private storeSession(response: AuthResponse): void {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(response));
    this.session.set(response);
  }

  private restoreSession(): AuthResponse | null {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) {
      return null;
    }
    try {
      return JSON.parse(raw) as AuthResponse;
    } catch {
      return null;
    }
  }
}
