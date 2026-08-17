import { AnimatePresence, motion } from 'framer-motion'
import type { ReactNode } from 'react'

export function WizardProgress({
  currentStep,
  labels,
}: {
  currentStep: number
  labels: string[]
}) {
  const fillPercent =
    labels.length > 1 ? ((currentStep - 1) / (labels.length - 1)) * 100 : 0

  return (
    <div className="wizard-progress">
      <div className="wizard-progress-track">
        <motion.div
          className="wizard-progress-fill"
          initial={false}
          animate={{ width: `${fillPercent}%` }}
          transition={{ type: 'spring', stiffness: 200, damping: 30 }}
        />
      </div>
      <div className="wizard-progress-steps">
        {labels.map((label, index) => {
          const stepNumber = index + 1
          const isDone = stepNumber < currentStep
          const isActive = stepNumber === currentStep

          return (
            <div
              key={label}
              className={`wizard-progress-step ${isActive ? 'active' : ''} ${isDone ? 'done' : ''}`}
            >
              <motion.div
                className="wizard-progress-dot"
                animate={{
                  scale: isActive ? 1.15 : 1,
                }}
                transition={{ type: 'spring', stiffness: 300, damping: 20 }}
              >
                {isDone ? '✓' : stepNumber}
              </motion.div>
              <span>{label}</span>
            </div>
          )
        })}
      </div>
    </div>
  )
}

export function WizardShell({
  currentStep,
  labels,
  children,
}: {
  currentStep: number
  labels: string[]
  children: ReactNode
}) {
  return (
    <div className="wizard-shell">
      <WizardProgress currentStep={currentStep} labels={labels} />
      <AnimatePresence mode="wait">
        <motion.div
          key={currentStep}
          initial={{ opacity: 0, x: 32 }}
          animate={{ opacity: 1, x: 0 }}
          exit={{ opacity: 0, x: -32 }}
          transition={{ duration: 0.28, ease: 'easeInOut' }}
        >
          {children}
        </motion.div>
      </AnimatePresence>
    </div>
  )
}
