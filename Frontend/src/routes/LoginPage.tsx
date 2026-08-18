import { useState, type FormEvent } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { motion, AnimatePresence } from 'framer-motion'
import { useAuth } from '../context/AuthContext'
import { RoleId } from '../lib/roles'

export function LoginPage() {
  const { session, loading, roleId, otpVerified, signIn } = useAuth()
  const location = useLocation()

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)
  const [shakeKey, setShakeKey] = useState(0)

  if (!loading && session) {
    const from = (location.state as { from?: string } | null)?.from

    if (roleId === RoleId.Customer && !otpVerified) {
      return <Navigate to="/login/otp" state={{ from }} replace />
    }

    return <Navigate to={from ?? '/'} replace />
  }

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    setSubmitting(true)
    setError(null)

    const { error: signInError } = await signIn(email, password)

    setSubmitting(false)

    if (signInError) {
      setError(signInError)
      setShakeKey((k) => k + 1)
    }
  }

  return (
    <div className="login-page">
      <div className="login-page-pattern" aria-hidden="true" />
      <motion.div
        className="login-blob login-blob-1"
        aria-hidden="true"
        animate={{ x: [0, 36, -18, 0], y: [0, -26, 18, 0] }}
        transition={{ duration: 22, repeat: Infinity, ease: 'easeInOut' }}
      />
      <motion.div
        className="login-blob login-blob-2"
        aria-hidden="true"
        animate={{ x: [0, -44, 26, 0], y: [0, 34, -18, 0] }}
        transition={{ duration: 27, repeat: Infinity, ease: 'easeInOut' }}
      />
      <motion.div
        className="login-blob login-blob-3"
        aria-hidden="true"
        animate={{ x: [0, 26, -34, 0], y: [0, -18, 26, 0] }}
        transition={{ duration: 24, repeat: Infinity, ease: 'easeInOut' }}
      />

      <motion.form
        className="login-card"
        onSubmit={handleSubmit}
        initial={{ opacity: 0, y: 24 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5, ease: [0.16, 1, 0.3, 1] }}
      >
        <motion.div
          className="login-mark"
          initial={{ scale: 0.6, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          transition={{ delay: 0.1, duration: 0.4, ease: 'backOut' }}
        >
          <svg viewBox="0 0 48 48" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path
              d="M24 4 8 10v12c0 11 7 17.6 16 22 9-4.4 16-11 16-22V10L24 4Z"
              fill="var(--color-primary)"
              opacity="0.12"
            />
            <path
              d="M24 4 8 10v12c0 11 7 17.6 16 22 9-4.4 16-11 16-22V10L24 4Z"
              stroke="var(--color-primary)"
              strokeWidth="1.6"
              strokeLinejoin="round"
            />
            <path
              d="M17 24.5 21.7 29 31.5 18.5"
              stroke="var(--color-primary)"
              strokeWidth="2.2"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          </svg>
        </motion.div>

        <h1>ClaimShield</h1>
        <p className="subtitle">Surveyor / Approver / Repairer portal</p>

        <label htmlFor="email">Email</label>
        <input
          id="email"
          type="email"
          autoComplete="username"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          required
        />

        <label htmlFor="password">Password</label>
        <input
          id="password"
          type="password"
          autoComplete="current-password"
          value={password}
          onChange={(event) => setPassword(event.target.value)}
          required
        />

        <AnimatePresence mode="wait">
          {error && (
            <motion.p
              key={shakeKey}
              className="error-text"
              initial={{ x: 0 }}
              animate={{ x: [0, -8, 8, -6, 6, -3, 3, 0] }}
              transition={{ duration: 0.4 }}
            >
              {error}
            </motion.p>
          )}
        </AnimatePresence>

        <motion.button
          type="submit"
          disabled={submitting}
          whileHover={{ scale: submitting ? 1 : 1.02 }}
          whileTap={{ scale: submitting ? 1 : 0.98 }}
        >
          {submitting ? 'Signing in…' : 'Sign in'}
        </motion.button>
      </motion.form>
    </div>
  )
}