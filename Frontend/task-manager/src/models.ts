export const Priority = {
  Low: 0,
  Medium: 1,
  High: 2,
} as const

export type Priority = (typeof Priority)[keyof typeof Priority]

export const PRIORITY_LABELS: Record<Priority, string> = {
  [Priority.Low]: 'Low',
  [Priority.Medium]: 'Medium',
  [Priority.High]: 'High',
}

export interface TaskDto {
  id: string
  order: number
  priority: Priority
  title: string
  description: string
  createdDate: string
  completedDate: string | null
  completedDescription: string | null
}

// Mirrors the backend FluentValidation rules (AuthValidators / TaskValidators).
export const LIMITS = {
  titleMin: 5,
  titleMax: 200,
  descriptionMax: 2000,
  nameMax: 100,
  emailMax: 256,
  passwordMin: 6,
} as const

export interface AuthResponse {
  userId: string
  email: string
  firstName: string
  lastName: string
  token: string
}
