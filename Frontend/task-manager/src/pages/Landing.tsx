import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../AuthContext'
import { LIMITS, isValidEmail } from '../models'

type Mode = 'signin' | 'register'

interface FieldErrors {
  email?: string
  password?: string
  firstName?: string
  lastName?: string
  form?: string
}

export function Landing() {
  const { login, register } = useAuth()
  const navigate = useNavigate()

  const [mode, setMode] = useState<Mode>('signin')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [errors, setErrors] = useState<FieldErrors>({})
  const [busy, setBusy] = useState(false)

  const switchMode = (next: Mode) => {
    setMode(next)
    setErrors({})
  }

  const clearError = (field: keyof FieldErrors) =>
    setErrors((prev) => (prev[field] ? { ...prev, [field]: undefined } : prev))

  const submit = async (event: FormEvent) => {
    event.preventDefault()

    const next: FieldErrors = {}
    if (!email.trim()) {
      next.email = 'Email is required.'
    } else if (!isValidEmail(email)) {
      next.email = 'Enter a valid email address.'
    }
    if (!password) {
      next.password = 'Password is required.'
    }
    if (mode === 'register') {
      const first = firstName.trim()
      const last = lastName.trim()
      if (!first) next.firstName = 'First name is required.'
      else if (first.length < LIMITS.nameMin) next.firstName = `First name must be at least ${LIMITS.nameMin} characters.`
      if (!last) next.lastName = 'Last name is required.'
      else if (last.length < LIMITS.nameMin) next.lastName = `Last name must be at least ${LIMITS.nameMin} characters.`
      if (password && password.length < LIMITS.passwordMin) {
        next.password = `Password must be at least ${LIMITS.passwordMin} characters.`
      }
    }
    if (next.email || next.password || next.firstName || next.lastName) {
      setErrors(next)
      return
    }

    setBusy(true)
    setErrors({})
    try {
      if (mode === 'signin') {
        await login(email.trim(), password)
      } else {
        await register(email.trim(), password, firstName.trim(), lastName.trim())
      }
      navigate('/tasks')
    } catch (err) {
      setErrors({ form: err instanceof Error ? err.message : 'Something went wrong. Please try again.' })
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

        <form onSubmit={submit} noValidate>
          {mode === 'register' && (
            <div className="row">
              <label className="grow">
                First name
                <input
                  name="firstName"
                  value={firstName}
                  onChange={(e) => {
                    setFirstName(e.target.value)
                    clearError('firstName')
                  }}
                  placeholder="Ada"
                  autoComplete="given-name"
                  maxLength={LIMITS.nameMax}
                  aria-invalid={!!errors.firstName}
                />
                {errors.firstName && <span className="field-error">{errors.firstName}</span>}
              </label>
              <label className="grow">
                Last name
                <input
                  name="lastName"
                  value={lastName}
                  onChange={(e) => {
                    setLastName(e.target.value)
                    clearError('lastName')
                  }}
                  placeholder="Lovelace"
                  autoComplete="family-name"
                  maxLength={LIMITS.nameMax}
                  aria-invalid={!!errors.lastName}
                />
                {errors.lastName && <span className="field-error">{errors.lastName}</span>}
              </label>
            </div>
          )}

          <label>
            Email
            <input
              type="email"
              name="email"
              value={email}
              onChange={(e) => {
                setEmail(e.target.value)
                clearError('email')
              }}
              placeholder="you@example.com"
              autoComplete="email"
              maxLength={LIMITS.emailMax}
              aria-invalid={!!errors.email}
            />
            {errors.email && <span className="field-error">{errors.email}</span>}
          </label>

          <label>
            Password
            <input
              type="password"
              name="password"
              value={password}
              onChange={(e) => {
                setPassword(e.target.value)
                clearError('password')
              }}
              placeholder={mode === 'register' ? `At least ${LIMITS.passwordMin} characters` : 'Your password'}
              autoComplete={mode === 'register' ? 'new-password' : 'current-password'}
              aria-invalid={!!errors.password}
            />
            {errors.password && <span className="field-error">{errors.password}</span>}
          </label>

          {errors.form && <p className="error">{errors.form}</p>}

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
