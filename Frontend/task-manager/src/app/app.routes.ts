import { Routes } from '@angular/router';
import { authGuard } from './auth.guard';
import { Landing } from './pages/landing/landing';
import { Tasks } from './pages/tasks/tasks';

export const routes: Routes = [
  { path: '', component: Landing },
  { path: 'tasks', component: Tasks, canActivate: [authGuard] },
  { path: '**', redirectTo: '' },
];
