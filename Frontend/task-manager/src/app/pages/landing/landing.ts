import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-landing',
  imports: [FormsModule],
  templateUrl: './landing.html',
  styleUrl: './landing.css',
})
export class Landing {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly mode = signal<'signin' | 'register'>('signin');
  readonly error = signal<string | null>(null);
  readonly busy = signal(false);

  email = '';
  password = '';

  constructor() {
    if (this.auth.isAuthenticated()) {
      this.router.navigate(['/tasks']);
    }
  }

  setMode(mode: 'signin' | 'register'): void {
    this.mode.set(mode);
    this.error.set(null);
  }

  async submit(): Promise<void> {
    if (!this.email.trim() || !this.password) {
      this.error.set('Email and password are required.');
      return;
    }

    this.busy.set(true);
    this.error.set(null);
    try {
      if (this.mode() === 'signin') {
        await this.auth.login(this.email, this.password);
      } else {
        await this.auth.register(this.email, this.password);
      }
      await this.router.navigate(['/tasks']);
    } catch (err: unknown) {
      this.error.set(this.messageFrom(err));
    } finally {
      this.busy.set(false);
    }
  }

  private messageFrom(err: unknown): string {
    const httpError = err as { error?: { message?: string }; status?: number };
    if (httpError?.error?.message) {
      return httpError.error.message;
    }
    if (httpError?.status === 0) {
      return 'Cannot reach the server. Is the backend running?';
    }
    return 'Something went wrong. Please try again.';
  }
}
