import { motion } from 'framer-motion'
import { Link } from 'react-router-dom'
import { Zap, FileText, ShieldCheck, Wallet } from 'lucide-react'

const MINI_STEPS = [
  { Icon: FileText, label: 'Report' },
  { Icon: ShieldCheck, label: 'Verify' },
  { Icon: Wallet, label: 'Instant payout' },
]

export function InstantClaimBanner() {
  return (
    <motion.section
      className="instant-claim-banner"
      initial={{ opacity: 0, y: 16 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.5, ease: 'easeOut' }}
    >
      <div className="instant-claim-banner-icon-wrap">
        <motion.div
          className="instant-claim-banner-icon"
          animate={{ scale: [1, 1.12, 1], opacity: [0.9, 1, 0.9] }}
          transition={{ duration: 2.2, repeat: Infinity, ease: 'easeInOut' }}
        >
          <Zap size={26} fill="currentColor" />
        </motion.div>
      </div>

      <div className="instant-claim-banner-body">
        <h2>Got a minor accident? You could get paid instantly.</h2>
        <p>
          Minor Accident claims with verified vehicle documents and no reported injuries may
          qualify for Instant Claim — approved and paid in minutes, no surveyor visit needed.
        </p>

        <div className="instant-claim-banner-steps">
          {MINI_STEPS.map(({ Icon, label }, index) => (
            <span key={label} className="instant-claim-banner-step">
              <Icon size={15} />
              {label}
              {index < MINI_STEPS.length - 1 && <span aria-hidden="true">→</span>}
            </span>
          ))}
        </div>
      </div>

      <Link to="/my-claims/new" className="instant-claim-banner-cta">
        Check if you qualify
      </Link>
    </motion.section>
  )
}
