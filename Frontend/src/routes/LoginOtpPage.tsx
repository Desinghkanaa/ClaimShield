import { useEffect, useRef, useState } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { motion } from 'framer-motion'
import { useAuth } from '../context/AuthContext'
import { useToast } from '../context/ToastContext'
import { sendOtp } from '../lib/api'
import { OtpPurpose } from '../lib/statuses'
import { OtpInput, type OtpInputStatus } from '../components/OtpInput'
import { RoleId } from '../lib/roles'

export function LoginOtpPage() {
  const { session, loading, roleId, otpVerified, markOtpVerified } = useAuth()
  const { showToast } = useToast()
  const location = useLocation()

  const [code, setCode] = useState('')
  const [status, setStatus] = useState<OtpInputStatus>('idle')
  const [sending, setSending] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const sentRef = useRef(false)

  useEffect(() => {
    if (sentRef.current || !session) return

    sentRef.current = true

    sendOtp(OtpPurpose.Login)
      .then((result) => {
        showToast(result.message, 'info')

        if (result.devModeCode) {
          showToast(`Dev OTP: ${result.devModeCode}`, 'info')
        }
      })
      .catch(() => {
        // For demo/testing, OTP sending failure should not block
        // the user from continuing.
        showToast('Demo OTP mode enabled.', 'info')
      })
      .finally(() => {
        setSending(false)
      })
  }, [session, showToast])

  if (loading) {
    return <div className="centered-page">Loading…</div>
  }

  if (!session) {
    return <Navigate to="/login" replace />
  }

  if (roleId !== RoleId.Customer || otpVerified) {
    const from = (location.state as { from?: string } | null)?.from

    return <Navigate to={from ?? '/'} replace />
  }

  const handleComplete = async (value: string) => {
    setError(null)

    // ---------------------------------------------------------
    // DEMO / TEST MODE
    // ---------------------------------------------------------
    // Accept ANY 6-digit OTP.
    // No backend OTP verification is performed here.
    // ---------------------------------------------------------

    if (value.length !== 6) {
      setStatus('error')
      setError('Please enter a 6-digit OTP.')
      return
    }

    setStatus('success')

    showToast('OTP accepted successfully.', 'success')

    // Give the success animation time to appear.
    await new Promise((resolve) => setTimeout(resolve, 600))

    // Mark the current Supabase user as OTP verified.
    markOtpVerified()
  }

  return (
    <div className="login-page">
      <div className="login-page-pattern" aria-hidden="true" />

      <motion.div
        className="login-blob login-blob-1"
        aria-hidden="true"
        animate={{
          x: [0, 36, -18, 0],
          y: [0, -26, 18, 0],
        }}
        transition={{
          duration: 22,
          repeat: Infinity,
          ease: 'easeInOut',
        }}
      />

      <motion.div
        className="login-blob login-blob-2"
        aria-hidden="true"
        animate={{
          x: [0, -44, 26, 0],
          y: [0, 34, -18, 0],
        }}
        transition={{
          duration: 27,
          repeat: Infinity,
          ease: 'easeInOut',
        }}
      />

      <motion.div
        className="login-card"
        initial={{ opacity: 0, y: 24 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{
          duration: 0.5,
          ease: [0.16, 1, 0.3, 1],
        }}
      >
        <h1>Verify it's you</h1>

        <p className="subtitle">
          {sending
            ? 'Sending a one-time code…'
            : 'Enter any 6-digit code to continue.'}
        </p>

        <OtpInput
          value={code}
          onChange={setCode}
          onComplete={handleComplete}
          status={status}
        />

        {error && <p className="error-text">{error}</p>}
      </motion.div>
    </div>
  )
}