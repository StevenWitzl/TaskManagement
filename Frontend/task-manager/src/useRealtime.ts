import { useEffect, useState } from 'react'
import {
  HttpTransportType,
  HubConnectionBuilder,
  LogLevel,
} from '@microsoft/signalr'
import { API_BASE_URL } from './api'
import type { TaskDto } from './models'

/**
 * Real-time view of the user's tasks. The server pushes the full task list
 * over the WebSocket on connect and after every change; components just
 * render what this hook returns.
 */
export function useRealtime(token: string | null) {
  const [tasks, setTasks] = useState<TaskDto[]>([])
  const [connected, setConnected] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!token) return

    const connection = new HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/tasks`, {
        accessTokenFactory: () => token,
        transport: HttpTransportType.WebSockets,
        skipNegotiation: true,
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build()

    connection.on('TasksUpdated', (next: TaskDto[]) => setTasks(next))
    connection.onreconnecting(() => setConnected(false))
    connection.onreconnected(() => setConnected(true))
    connection.onclose(() => setConnected(false))

    let cancelled = false
    const startPromise = connection
      .start()
      .then(() => {
        if (!cancelled) setConnected(true)
      })
      .catch(() => {
        if (!cancelled) setError('Could not connect to the live task feed. Is the backend running?')
      })

    return () => {
      cancelled = true
      // Wait for start() to settle before stopping; stopping mid-start makes
      // SignalR log an error (visible under React StrictMode's double mount).
      void startPromise.then(() => connection.stop())
      setConnected(false)
      setTasks([])
    }
  }, [token])

  return { tasks, connected, error }
}
