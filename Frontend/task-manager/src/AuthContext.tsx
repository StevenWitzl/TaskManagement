import { createContext, useCallback, useContext, useState, type ReactNode } from 'react'
import { apiFetch } from './api'
import type { AuthResponse } from './models'

const STORAGE_KEY = 'taskmanagement.auth'

interface AuthContextValue {
  user: AuthResponse | null
  login: (email: string, password: string) => Promise<void>
  register: (email: string, password: string, firstName: string, lastName: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

function restoreSession(): AuthResponse | null {
  const raw = localStorage.getItem(STORAGE_KEY)
  if (!raw) return null
  try {
    return JSON.parse(raw) as AuthResponse
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthResponse | null>(restoreSession)

  const storeSession = (session: AuthResponse) => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(session))
    setUser(session)
  }

  const login = useCallback(async (email: string, password: string) => {
    storeSession(await apiFetch<AuthResponse>('/api/auth/login', { method: 'POST', body: { email, password } }))
  }, [])

  const register = useCallback(
    async (email: string, password: string, firstName: string, lastName: string) => {
      storeSession(
        await apiFetch<AuthResponse>('/api/auth/register', {
          method: 'POST',
          body: { email, password, firstName, lastName },
        }),
      )
    },
    [],
  )

  const logout = useCallback(() => {
    localStorage.removeItem(STORAGE_KEY)
    setUser(null)
  }, [])

  return <AuthContext.Provider value={{ user, login, register, logout }}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider')
  return ctx
}
