import { motion } from 'framer-motion'
import step1 from '../assets/illustrations/step1-report-claim.svg'
import step2 from '../assets/illustrations/step2-verify-instantly.svg'
import step3 from '../assets/illustrations/step3-get-paid.svg'

const STEPS = [
  {
    image: step1,
    title: 'Report your claim',
    description: 'Add the incident details and a few photos - it takes a few minutes.',
  },
  {
    image: step2,
    title: 'We verify instantly',
    description:
      'For minor accidents, we check your documents and vehicle details automatically, right away.',
  },
  {
    image: step3,
    title: 'Get paid',
    description:
      'Claims that qualify move straight to payment. Everything else goes to a surveyor for a full assessment.',
  },
]

export function HowItWorks() {
  return (
    <section className="card how-it-works">
      <h2>How ClaimShield Works</h2>
      <div className="how-it-works-grid">
        {STEPS.map((step, index) => (
          <motion.div
            key={step.title}
            className="how-it-works-step"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.45, delay: index * 0.12, ease: 'easeOut' }}
          >
            <img src={step.image} alt="" className="how-it-works-illustration" />
            <h3>{step.title}</h3>
            <p>{step.description}</p>
          </motion.div>
        ))}
      </div>
    </section>
  )
}
