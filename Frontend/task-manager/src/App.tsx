import { Navigate, Route, Routes } from 'react-router-dom'
import { useAuth } from './AuthContext'
import { Landing } from './pages/Landing'
import { Tasks } from './pages/Tasks'
import type { ReactNode } from 'react'

function RequireAuth({ children }: { children: ReactNode }) {
  const { user } = useAuth()
  return user ? children : <Navigate to="/" replace />
}

function RedirectIfAuthed({ children }: { children: ReactNode }) {
  const { user } = useAuth()
  return user ? <Navigate to="/tasks" replace /> : children
}

export default function App() {
  return (
    <Routes>
      <Route
        path="/"
        element={
          <RedirectIfAuthed>
            <Landing />
          </RedirectIfAuthed>
        }
      />
      <Route
        path="/tasks"
        element={
          <RequireAuth>
            <Tasks />
          </RequireAuth>
        }
      />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
