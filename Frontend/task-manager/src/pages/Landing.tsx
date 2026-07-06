import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../AuthContext'

type Mode = 'signin' | 'register'

export function Landing() {
  const { login, register } = useAuth()
  const navigate = useNavigate()

  const [mode, setMode] = useState<Mode>('signin')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  const switchMode = (next: Mode) => {
    setMode(next)
    setError(null)
  }

  const submit = async (event: FormEvent) => {
    event.preventDefault()

    if (!email.trim() || !password) {
      setError('Email and password are required.')
      return
    }
    if (mode === 'register' && (!firstName.trim() || !lastName.trim())) {
      setError('First and last name are required.')
      return
    }

    setBusy(true)
    setError(null)
    try {
      if (mode === 'signin') {
        await login(email, password)
      } else {
        await register(email, password, firstName, lastName)
      }
      navigate('/tasks')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Something went wrong. Please try again.')
    } finally {
      setBusy(false)
    }
  }

  return (
    <main className="landing">
      <section className="card">
        <h1>Task Management</h1>
        <p className="subtitle">Stay on top of your work, in real time.</p>

        <div className="tabs">
          <button type="button" className={mode === 'signin' ? 'active' : ''} onClick={() => switchMode('signin')}>
            Sign in
          </button>
          <button type="button" className={mode === 'register' ? 'active' : ''} onClick={() => switchMode('register')}>
            Create account
          </button>
        </div>

        <form onSubmit={submit}>
          {mode === 'register' && (
            <div className="row">
              <label className="grow">
                First name
                <input
                  name="firstName"
                  value={firstName}
                  onChange={(e) => setFirstName(e.target.value)}
                  placeholder="Ada"
                  autoComplete="given-name"
                  required
                />
              </label>
              <label className="grow">
                Last name
                <input
                  name="lastName"
                  value={lastName}
                  onChange={(e) => setLastName(e.target.value)}
                  placeholder="Lovelace"
                  autoComplete="family-name"
                  required
                />
              </label>
            </div>
          )}

          <label>
            Email
            <input
              type="email"
              name="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="you@example.com"
              autoComplete="email"
              required
            />
          </label>

          <label>
            Password
            <input
              type="password"
              name="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder={mode === 'register' ? 'At least 6 characters' : 'Your password'}
              autoComplete={mode === 'register' ? 'new-password' : 'current-password'}
              required
            />
          </label>

          {error && <p className="error">{error}</p>}

          <button className="primary" type="submit" disabled={busy}>
            {busy ? 'Please wait…' : mode === 'signin' ? 'Sign in' : 'Create account'}
          </button>
        </form>

        <p className="hint">
          Demo account: <code>demo@taskmanagement.local</code> / <code>Demo123!</code>
        </p>
      </section>
    </main>
  )
}
