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

export interface AuthResponse {
  userId: string
  email: string
  firstName: string
  lastName: string
  token: string
}
