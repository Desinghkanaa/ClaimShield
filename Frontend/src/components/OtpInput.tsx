import { useEffect, useRef, useState } from 'react'
import { motion } from 'framer-motion'

export type OtpInputStatus = 'idle' | 'error' | 'success'

export function OtpInput({
  length = 6,
  value,
  onChange,
  onComplete,
  status,
  disabled,
}: {
  length?: number
  value: string
  onChange: (value: string) => void
  onComplete?: (value: string) => void
  status: OtpInputStatus
  disabled?: boolean
}) {
  const inputRefs = useRef<(HTMLInputElement | null)[]>([])
  const [shakeKey, setShakeKey] = useState(0)

  useEffect(() => {
    if (status === 'error') {
      setShakeKey((k) => k + 1)
    }
  }, [status])

  const digits = Array.from({ length }, (_, i) => value[i] ?? '')

  const setDigit = (index: number, digit: string) => {
    const chars = value.split('')
    chars[index] = digit
    const next = chars.join('').slice(0, length)
    onChange(next)

    if (digit && index < length - 1) {
      inputRefs.current[index + 1]?.focus()
    }

    if (next.length === length) {
      onComplete?.(next)
    }
  }

  const handleChange = (index: number, raw: string) => {
    const digit = raw.replace(/\D/g, '').slice(-1)
    setDigit(index, digit)
  }

  const handleKeyDown = (index: number, e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Backspace' && !digits[index] && index > 0) {
      inputRefs.current[index - 1]?.focus()
    }
  }

  const handlePaste = (e: React.ClipboardEvent<HTMLInputElement>) => {
    const pasted = e.clipboardData.getData('text').replace(/\D/g, '').slice(0, length)
    if (!pasted) return
    e.preventDefault()
    onChange(pasted.padEnd(length, '').slice(0, length).replace(/ /g, ''))
    const focusIndex = Math.min(pasted.length, length - 1)
    inputRefs.current[focusIndex]?.focus()
    if (pasted.length === length) {
      onComplete?.(pasted)
    }
  }

  return (
    <div className="otp-input-row">
      <motion.div
        key={shakeKey}
        className="otp-input-boxes"
        animate={
          status === 'error'
            ? { x: [0, -10, 10, -8, 8, -4, 4, 0] }
            : { x: 0 }
        }
        transition={{ duration: 0.45 }}
      >
        {digits.map((digit, index) => (
          <input
            key={index}
            ref={(el) => {
              inputRefs.current[index] = el
            }}
            type="text"
            inputMode="numeric"
            maxLength={1}
            className={`otp-box otp-box-${status}`}
            value={digit}
            disabled={disabled}
            onChange={(e) => handleChange(index, e.target.value)}
            onKeyDown={(e) => handleKeyDown(index, e)}
            onPaste={handlePaste}
            autoFocus={index === 0}
          />
        ))}
      </motion.div>
      {status === 'success' && (
        <motion.div
          className="otp-success-check"
          initial={{ scale: 0, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          transition={{ type: 'spring', stiffness: 500, damping: 20 }}
        >
          ✓
        </motion.div>
      )}
    </div>
  )
}
