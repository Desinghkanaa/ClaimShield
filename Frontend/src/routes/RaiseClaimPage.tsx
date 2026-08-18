import { useEffect, useRef, useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { motion } from 'framer-motion'
import {
  CarFront,
  PackageX,
  CloudLightning,
  AlertOctagon,
  Siren,
  Flame,
  Mic,
  CheckCircle2,
  FileText,
  HelpCircle,
  ClipboardCheck,
  Wallet,
  Car,
  type LucideIcon,
} from 'lucide-react'
import {
  acceptInstantClaim,
  ApiError,
  declineInstantClaim,
  generateEstimate,
  getMyClaimScore,
  getMyCustomerProfile,
  getMyPolicies,
  getMyVehicles,
  raiseClaimStep1,
  raiseClaimStep2,
  sendOtp,
  verifyOtp,
} from '../lib/api'
import type {
  ClaimEstimateResultDto,
  CustomerClaimScoreDto,
  InstantClaimPartsSelection,
  PolicyResponseDto,
  VehicleResponseDto,
} from '../lib/types'
import {
  LossType,
  LossTypeName,
  OtpPurpose,
  VehicleLocationName,
} from '../lib/statuses'
import { WizardShell } from '../components/WizardShell'
import { Modal } from '../components/Modal'
import { OtpInput, type OtpInputStatus } from '../components/OtpInput'
import { UploadCard } from '../components/UploadCard'
import { Skeleton, SkeletonBlock } from '../components/Skeleton'
import { useToast } from '../context/ToastContext'

const STEP_LABELS = ['Basic Information', 'Documents & Checks', 'Review & Estimate']

const LOSS_TYPE_ICONS: Record<number, { Icon: LucideIcon; tone: string }> = {
  [LossType.MinorAccident]: { Icon: CarFront, tone: 'blue' },
  [LossType.PartsTheft]: { Icon: PackageX, tone: 'amber' },
  [LossType.NaturalCalamities]: { Icon: CloudLightning, tone: 'teal' },
  [LossType.FullLossTheft]: { Icon: AlertOctagon, tone: 'red' },
  [LossType.MajorAccident]: { Icon: Siren, tone: 'red' },
  [LossType.Fire]: { Icon: Flame, tone: 'amber' },
}

function formatCurrency(amount: number) {
  return `₹ ${amount.toLocaleString('en-IN')}`
}

export function RaiseClaimPage() {
  const navigate = useNavigate()
  const { showToast } = useToast()

  const [step, setStep] = useState(1)
  const [claimId, setClaimId] = useState<string | null>(null)
  const [claimNumber, setClaimNumber] = useState<string | null>(null)

  const [customerLoading, setCustomerLoading] = useState(true)
  const [policies, setPolicies] = useState<PolicyResponseDto[]>([])
  const [vehicles, setVehicles] = useState<VehicleResponseDto[]>([])
  const [loadError, setLoadError] = useState<string | null>(null)

  const [showConfirmModal, setShowConfirmModal] = useState(false)
  const [confirmMessage, setConfirmMessage] = useState('')
  const [showRoutedModal, setShowRoutedModal] = useState(false)
  const [routedMessage, setRoutedMessage] = useState('')

  useEffect(() => {
    getMyCustomerProfile()
      .then((customer) =>
        Promise.all([getMyPolicies(customer.customerId), getMyVehicles(customer.customerId)]),
      )
      .then(([policyData, vehicleData]) => {
        setPolicies(policyData)
        setVehicles(vehicleData)
      })
      .catch((err: unknown) => {
        setLoadError(err instanceof ApiError ? err.message : 'Failed to load your details.')
      })
      .finally(() => setCustomerLoading(false))
  }, [])

  const handleStep1Done = (id: string, number: string, message: string) => {
    setClaimId(id)
    setClaimNumber(number)
    setConfirmMessage(message)
    setShowConfirmModal(true)
  }

  const handleRouted = (message: string) => {
    setRoutedMessage(message)
    setShowRoutedModal(true)
  }

  if (loadError) {
    return <p className="error-text">{loadError}</p>
  }

  return (
    <div>
      <h1>Raise a Claim</h1>

      <WizardShell currentStep={step} labels={STEP_LABELS}>
        {step === 1 && (
          <Step1
            loading={customerLoading}
            policies={policies}
            vehicles={vehicles}
            onDone={handleStep1Done}
          />
        )}
        {step === 2 && claimId && (
          <Step2
            claimId={claimId}
            claimNumber={claimNumber!}
            onVerified={() => setStep(3)}
            onRouted={handleRouted}
          />
        )}
        {step === 3 && claimId && (
          <Step3
            claimId={claimId}
            claimNumber={claimNumber!}
            onDone={(message) => {
              showToast(message, 'success')
              navigate(`/my-claims/${claimId}`)
            }}
            onRouted={handleRouted}
          />
        )}
      </WizardShell>

      <Modal open={showConfirmModal} title="Claim registered!">
        <p>{confirmMessage}</p>
        <button
          type="button"
          onClick={() => {
            setShowConfirmModal(false)
            setStep(2)
          }}
        >
          Continue
        </button>
      </Modal>

      <Modal open={showRoutedModal} title="Routed to a Surveyor">
        <p>{routedMessage}</p>
        <button
          type="button"
          onClick={() => {
            setShowRoutedModal(false)
            navigate(`/my-claims/${claimId}`)
          }}
        >
          View my claim
        </button>
      </Modal>
    </div>
  )
}

// =====================================================================
// STEP 1 - Basic Information
// =====================================================================

function Step1({
  loading,
  policies,
  vehicles,
  onDone,
}: {
  loading: boolean
  policies: PolicyResponseDto[]
  vehicles: VehicleResponseDto[]
  onDone: (claimId: string, claimNumber: string, message: string) => void
}) {
  const { showToast } = useToast()
  const [policyId, setPolicyId] = useState('')
  const [vehicleId, setVehicleId] = useState('')
  const [vehicleLocationAtLoss, setVehicleLocationAtLoss] = useState(0)
  const [lossType, setLossType] = useState(0)
  const [dateOfLoss, setDateOfLoss] = useState('')
  const [locationOfLoss, setLocationOfLoss] = useState('')
  const [description, setDescription] = useState('')
  const [instantToggle, setInstantToggle] = useState(false)
  const [parts, setParts] = useState<InstantClaimPartsSelection>({
    windshieldFront: false,
    windshieldRear: false,
    glass: false,
    tyre: false,
  })

  const [listening, setListening] = useState(false)
  const [voiceUnsupported, setVoiceUnsupported] = useState(false)

  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!loading && policies.length > 0 && !policyId) {
      setPolicyId(policies[0].policyId)
      setVehicleId(policies[0].vehicleId)
    }
  }, [loading, policies, policyId])

  const selectedPolicy = policies.find((p) => p.policyId === policyId)
  const selectedVehicle = vehicles.find((v) => v.vehicleId === vehicleId)

  const today = new Date().toISOString().slice(0, 10)
  const minDate = selectedPolicy?.startDate.slice(0, 10)

  const handleMic = () => {
    const SpeechRecognitionCtor =
      (window as unknown as { SpeechRecognition?: new () => SpeechRecognition })
        .SpeechRecognition ??
      (window as unknown as { webkitSpeechRecognition?: new () => SpeechRecognition })
        .webkitSpeechRecognition

    if (!SpeechRecognitionCtor) {
      setVoiceUnsupported(true)
      return
    }

    const recognition = new SpeechRecognitionCtor()
    recognition.lang = 'en-IN'
    recognition.interimResults = false

    recognition.onresult = (event: SpeechRecognitionEvent) => {
      const transcript = event.results[0]?.[0]?.transcript ?? ''
      setDescription((current) => (current ? `${current} ${transcript}` : transcript))
    }

    recognition.onerror = (event: SpeechRecognitionErrorEvent) => {
      setListening(false)

      const message =
        event.error === 'not-allowed' || event.error === 'service-not-allowed'
          ? 'Microphone access was denied. Allow microphone access in your browser and try again.'
          : event.error === 'no-speech'
            ? "Didn't catch that - no speech was detected. Please try again."
            : event.error === 'audio-capture'
              ? 'No microphone was found on this device.'
              : 'Voice input failed. Please try again or type your description.'

      showToast(message, 'error')
    }
    recognition.onend = () => setListening(false)

    setListening(true)
    recognition.start()
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError(null)

    if (!policyId || !vehicleId) {
      setError('Select a policy and vehicle.')
      return
    }

    if (!vehicleLocationAtLoss || !lossType || !dateOfLoss || !locationOfLoss || description.length < 10) {
      setError('Please complete all required fields.')
      return
    }

    const effectiveToggle = lossType === LossType.MinorAccident && instantToggle

    if (effectiveToggle && !Object.values(parts).some(Boolean)) {
      setError('Select at least one part for the Instant Claim option.')
      return
    }

    setSubmitting(true)

    try {
      const result = await raiseClaimStep1({
        policyId,
        vehicleId,
        vehicleLocationAtLoss,
        lossType,
        dateOfLoss: new Date(dateOfLoss).toISOString(),
        locationOfLoss,
        description,
        instantClaimToggle: effectiveToggle,
        instantClaimParts: effectiveToggle ? parts : null,
        customerEstimatedAmount: null,
      })

      onDone(result.claimId, result.claimNumber, result.message)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Failed to submit claim.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <section className="card card-tint-blue">
        <h2>Your details</h2>
        {loading ? (
          <SkeletonBlock lines={3} />
        ) : policies.length === 0 ? (
          <p className="error-text">
            No policy is on file for your account. Please contact support.
          </p>
        ) : (
          <dl className="fact-grid fact-grid-form">
            <dt>
              <FileText size={14} />
              Policy
            </dt>
            <dd>
              <label htmlFor="policy" className="visually-hidden">
                Policy
              </label>
              <select
                id="policy"
                className="fact-grid-select"
                value={policyId}
                onChange={(e) => {
                  setPolicyId(e.target.value)
                  const p = policies.find((x) => x.policyId === e.target.value)
                  if (p) setVehicleId(p.vehicleId)
                }}
              >
                {policies.map((p) => (
                  <option key={p.policyId} value={p.policyId}>
                    {p.policyNumber}
                  </option>
                ))}
              </select>
            </dd>

            <dt>
              <Car size={14} />
              Vehicle
            </dt>
            <dd>{selectedVehicle?.registrationNumber ?? '—'}</dd>

            <dt>
              <Wallet size={14} />
              Coverage
            </dt>
            <dd>{selectedPolicy ? formatCurrency(selectedPolicy.coverageAmount) : '—'}</dd>
          </dl>
        )}
      </section>

      <section className="card card-tint-blue">
        <h2>Incident details</h2>

        <div className="form-field">
          <label htmlFor="vehicleLocation">Vehicle location right now</label>
          <select
            id="vehicleLocation"
            value={vehicleLocationAtLoss}
            onChange={(e) => setVehicleLocationAtLoss(Number(e.target.value))}
            required
          >
            <option value={0}>Select…</option>
            {Object.entries(VehicleLocationName).map(([value, name]) => (
              <option key={value} value={value}>
                {name}
              </option>
            ))}
          </select>
        </div>

        <div className="form-field">
          <label>Type of Loss</label>
          <div className="loss-type-cards">
            {Object.entries(LossTypeName).map(([value, name]) => {
              const { Icon, tone } = LOSS_TYPE_ICONS[Number(value)]
              const isSelected = lossType === Number(value)

              return (
                <div
                  key={value}
                  className={`loss-type-card ${isSelected ? 'selected' : ''}`}
                  onClick={() => {
                    setLossType(Number(value))
                    if (Number(value) !== LossType.MinorAccident) {
                      setInstantToggle(false)
                    }
                  }}
                >
                  {isSelected && <CheckCircle2 size={16} className="loss-type-card-check" />}
                  <span className={`loss-type-card-icon loss-type-card-icon-${tone}`}>
                    <Icon size={20} />
                  </span>
                  {name}
                </div>
              )
            })}
          </div>
        </div>

        <div className="form-row">
          <div className="form-field">
            <label htmlFor="dateOfLoss">Date of Loss</label>
            <input
              id="dateOfLoss"
              type="date"
              value={dateOfLoss}
              max={today}
              min={minDate}
              onChange={(e) => setDateOfLoss(e.target.value)}
              required
            />
          </div>

          <div className="form-field">
            <label htmlFor="locationOfLoss">Location of Loss</label>
            <input
              id="locationOfLoss"
              value={locationOfLoss}
              onChange={(e) => setLocationOfLoss(e.target.value)}
              required
            />
          </div>
        </div>

        <div className="form-field">
          <label htmlFor="description">Description</label>
          <div style={{ display: 'flex', gap: '0.6rem', alignItems: 'flex-start' }}>
            <textarea
              id="description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              rows={4}
              required
              minLength={10}
              style={{ flex: 1 }}
            />
            <button
              type="button"
              className={`mic-button ${listening ? 'listening' : ''}`}
              onClick={handleMic}
              title="Speak your description"
            >
              <Mic size={19} />
            </button>
          </div>
          {voiceUnsupported && (
            <p className="error-text">
              Voice input isn't supported in this browser. Please type your description.
            </p>
          )}
          {description.length > 0 && description.length < 10 && (
            <p className="error-text">
              Please provide a bit more detail (at least 10 characters).
            </p>
          )}
        </div>

        {lossType === LossType.MinorAccident && (
          <motion.div
            className="instant-claim-toggle-row instant-claim-toggle-highlight"
            initial={{ opacity: 0, scale: 0.96 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ duration: 0.35, ease: 'easeOut' }}
          >
            <button
              type="button"
              className={`toggle-switch ${instantToggle ? 'on' : ''}`}
              onClick={() => setInstantToggle((v) => !v)}
              aria-pressed={instantToggle}
            >
              <span
                className="toggle-switch-knob"
                style={{ transform: instantToggle ? 'translateX(1.4rem)' : 'translateX(0)' }}
              />
            </button>
            <span>Try for an Instant Claim</span>
          </motion.div>
        )}

        {instantToggle && lossType === LossType.MinorAccident && (
          <div className="instant-parts-checkboxes">
            {(
              [
                ['windshieldFront', 'Windshield — Front'],
                ['windshieldRear', 'Windshield — Rear'],
                ['glass', 'Glass (other than windshield)'],
                ['tyre', 'Tyre'],
              ] as const
            ).map(([key, label]) => (
              <label
                key={key}
                className={`instant-part-checkbox ${parts[key] ? 'checked' : ''}`}
              >
                <input
                  type="checkbox"
                  checked={parts[key]}
                  onChange={(e) => setParts((p) => ({ ...p, [key]: e.target.checked }))}
                />
                {label}
              </label>
            ))}
          </div>
        )}

        {error && <p className="error-text">{error}</p>}

        <button type="submit" disabled={submitting || loading}>
          {submitting ? 'Submitting…' : 'Next'}
        </button>
      </section>
    </form>
  )
}

// =====================================================================
// STEP 2 - Documents & Confirmatory Checks
// =====================================================================

function Step2({
  claimId,
  claimNumber,
  onVerified,
  onRouted,
}: {
  claimId: string
  claimNumber: string
  onVerified: () => void
  onRouted: (message: string) => void
}) {
  const [uploaded, setUploaded] = useState<Record<number, boolean>>({})
  const [vehicleParkedSafely, setVehicleParkedSafely] = useState<boolean | null>(null)
  const [deathOccurred, setDeathOccurred] = useState<boolean | null>(null)
  const [reviewing, setReviewing] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const allUploaded =
    uploaded[1] && uploaded[2] && uploaded[3] && uploaded[4] && uploaded[5] && uploaded[6]

  const handleVerify = async () => {
    if (vehicleParkedSafely == null || deathOccurred == null) {
      setError('Please answer both questions.')
      return
    }

    if (!allUploaded) {
      setError('Please upload all 6 documents before verifying.')
      return
    }

    setError(null)
    setReviewing(true)

    try {
      const result = await raiseClaimStep2(claimId, {
        vehicleParkedSafely,
        deathOccurred,
      })

      if (result.routedToSurveyor) {
        onRouted(result.message)
      } else {
        onVerified()
      }
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Verification failed.')
    } finally {
      setReviewing(false)
    }
  }

  const slots: [number, string][] = [
    [1, 'Vehicle photo — Front'],
    [2, 'Vehicle photo — Left'],
    [3, 'Vehicle photo — Back'],
    [4, 'Vehicle photo — Right'],
    [5, 'Number plate photo'],
    [6, 'RC document'],
  ]

  return (
    <div>
      <section className="card card-tint-blue">
        <h2>
          <FileText size={17} style={{ verticalAlign: '-3px', marginRight: '0.4rem' }} />
          {claimNumber} — Documents
        </h2>
        <div className="upload-grid">
          {slots.map(([typeId, label]) => (
            <UploadCard
              key={typeId}
              label={label}
              claimId={claimId}
              documentTypeId={typeId}
              onUploaded={() => setUploaded((u) => ({ ...u, [typeId]: true }))}
            />
          ))}
        </div>
      </section>

      <section className="card card-tint-blue">
        <h2>
          <HelpCircle size={17} style={{ verticalAlign: '-3px', marginRight: '0.4rem' }} />
          A couple of quick questions
        </h2>

        <label>Is the vehicle currently parked in a safe location?</label>
        <div className="radio-pill-group">
          <label className="radio-pill">
            <input
              type="radio"
              name="parked"
              checked={vehicleParkedSafely === true}
              onChange={() => setVehicleParkedSafely(true)}
            />
            Yes
          </label>
          <label className="radio-pill">
            <input
              type="radio"
              name="parked"
              checked={vehicleParkedSafely === false}
              onChange={() => setVehicleParkedSafely(false)}
            />
            No
          </label>
        </div>

        <label>Did any death occur in this incident?</label>
        <div className="radio-pill-group">
          <label className="radio-pill">
            <input
              type="radio"
              name="death"
              checked={deathOccurred === true}
              onChange={() => setDeathOccurred(true)}
            />
            Yes
          </label>
          <label className="radio-pill">
            <input
              type="radio"
              name="death"
              checked={deathOccurred === false}
              onChange={() => setDeathOccurred(false)}
            />
            No
          </label>
        </div>

        {error && <p className="error-text">{error}</p>}

        {reviewing ? (
          <div className="reviewing-banner">
            <span className="spinner" />
            Reviewing…
          </div>
        ) : (
          <button type="button" onClick={() => void handleVerify()}>
            Verify
          </button>
        )}
      </section>
    </div>
  )
}

// =====================================================================
// STEP 3 - Review, Estimate & Decision
// =====================================================================

function Step3({
  claimId,
  claimNumber,
  onDone,
  onRouted,
}: {
  claimId: string
  claimNumber: string
  onDone: (message: string) => void
  onRouted: (message: string) => void
}) {
  const [score, setScore] = useState<CustomerClaimScoreDto | null>(null)
  const [loadingEstimate, setLoadingEstimate] = useState(true)
  const [estimate, setEstimate] = useState<ClaimEstimateResultDto | null>(null)
  const [notEligibleReason, setNotEligibleReason] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [showOtp, setShowOtp] = useState(false)

  useEffect(() => {
    let cancelled = false

    getMyClaimScore(claimId)
      .then((s) => {
        if (!cancelled) setScore(s)
      })
      .catch(() => {
        /* Non-critical - risk band may not be ready yet. */
      })

    generateEstimate(claimId)
      .then((result) => {
        if (cancelled) return
        if (result.eligible) {
          setEstimate({ ...result, claimId })
        } else {
          setNotEligibleReason(result.reason)
        }
      })
      .catch((err: unknown) => {
        if (!cancelled) {
          setError(err instanceof ApiError ? err.message : 'Failed to generate estimate.')
        }
      })
      .finally(() => {
        if (!cancelled) setLoadingEstimate(false)
      })

    return () => {
      cancelled = true
    }
  }, [claimId])

  const handleDecline = async () => {
    try {
      const result = await declineInstantClaim(claimId)
      onRouted(result.message)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Failed to decline.')
    }
  }

  return (
    <div>
      <section className="card card-tint-blue">
        <h2>
          <ClipboardCheck size={17} style={{ verticalAlign: '-3px', marginRight: '0.4rem' }} />
          {claimNumber} — Review
        </h2>
        {score ? (
          <dl className="fact-grid">
            <dt>Readiness Score</dt>
            <dd>
              <span className={`band-badge band-${score.compositeBandName.toLowerCase()}`}>
                {score.compositeBandName}
              </span>{' '}
              ({score.compositeScore})
            </dd>
          </dl>
        ) : (
          <Skeleton width="8rem" height="1.5rem" />
        )}
      </section>

      {loadingEstimate && (
        <section className="card">
          <div className="reviewing-banner">
            <span className="spinner" />
            Generating your estimate…
          </div>
        </section>
      )}

      {!loadingEstimate && notEligibleReason && (
        <section className="card">
          <h2>This claim will go to a Surveyor</h2>
          <p>
            This claim doesn't qualify for Instant Claim right now ({notEligibleReason}). It
            has been submitted for standard assessment and you'll be notified once a Surveyor
            has been assigned.
          </p>
        </section>
      )}

      {!loadingEstimate && !estimate && !notEligibleReason && error && (
        <section className="card">
          <h2>Couldn't generate your estimate</h2>
          <p className="error-text">{error}</p>
        </section>
      )}

      {!loadingEstimate && estimate && !showOtp && (
        <section className="card card-tint-blue">
          <h2>
            <Wallet size={17} style={{ verticalAlign: '-3px', marginRight: '0.4rem' }} />
            Assessment cost breakdown
          </h2>
          <table className="estimate-breakdown">
            <tbody>
              <tr>
                <td>Remove &amp; Refit Charges</td>
                <td>{formatCurrency(estimate.lineItems.removeRefitCharge)}</td>
              </tr>
              <tr>
                <td>Denting Charges</td>
                <td>{formatCurrency(estimate.lineItems.dentingCharge)}</td>
              </tr>
              <tr>
                <td>Painting Charges</td>
                <td>{formatCurrency(estimate.lineItems.paintingCharge)}</td>
              </tr>
              <tr>
                <td>Total Labour Charges</td>
                <td>{formatCurrency(estimate.lineItems.totalLabourCharges)}</td>
              </tr>
              <tr>
                <td>Total Parts Amount</td>
                <td>{formatCurrency(estimate.lineItems.totalPartsAmount)}</td>
              </tr>
              <tr>
                <td>Policy Excess</td>
                <td>− {formatCurrency(estimate.lineItems.policyExcess)}</td>
              </tr>
              <tr>
                <td>Salvage Amount</td>
                <td>− {formatCurrency(estimate.lineItems.salvageAmount)}</td>
              </tr>
              <tr>
                <td>Other Deductions</td>
                <td>− {formatCurrency(estimate.lineItems.otherDeductions)}</td>
              </tr>
              <tr className="net-total">
                <td>Net Assessment Amount</td>
                <td>{formatCurrency(estimate.netAssessmentAmount)}</td>
              </tr>
            </tbody>
          </table>

          {error && <p className="error-text">{error}</p>}

          <button type="button" onClick={() => setShowOtp(true)}>
            Accept
          </button>{' '}
          <button type="button" onClick={() => void handleDecline()}>
            Decline
          </button>
        </section>
      )}

      {showOtp && estimate && (
        <AcceptOtpPanel
          claimId={claimId}
          netAmount={estimate.netAssessmentAmount}
          onAccepted={onDone}
          onCancel={() => setShowOtp(false)}
        />
      )}
    </div>
  )
}

function AcceptOtpPanel({
  claimId,
  netAmount,
  onAccepted,
  onCancel,
}: {
  claimId: string
  netAmount: number
  onAccepted: (message: string) => void
  onCancel: () => void
}) {
  const { showToast } = useToast()
  const [code, setCode] = useState('')
  const [status, setStatus] = useState<OtpInputStatus>('idle')
  const [sending, setSending] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const sentRef = useRef(false)

  useEffect(() => {
    if (sentRef.current) return
    sentRef.current = true

    sendOtp(OtpPurpose.InstantClaimAccept, claimId)
      .then((result) => {
        showToast(result.message, 'info')
        if (result.devModeCode) {
          showToast(`Dev OTP: ${result.devModeCode}`, 'info')
        }
      })
      .catch((err: unknown) => {
        setError(err instanceof ApiError ? err.message : 'Failed to send OTP.')
      })
      .finally(() => setSending(false))
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [claimId])

  const handleComplete = async (value: string) => {
    setError(null)

    try {
      const verifyResult = await verifyOtp(OtpPurpose.InstantClaimAccept, value, claimId)

      if (!verifyResult.success) {
        setStatus('error')
        setError(verifyResult.message)
        setCode('')
        return
      }

      setStatus('success')

      const acceptResult = await acceptInstantClaim(claimId)
      onAccepted(acceptResult.message)
    } catch (err) {
      setStatus('error')
      setError(err instanceof ApiError ? err.message : 'Failed to verify OTP.')
      setCode('')
    }
  }

  return (
    <section className="card">
      <h2>Verify to accept ₹{netAmount.toLocaleString('en-IN')}</h2>
      {sending ? (
        <p>Sending OTP…</p>
      ) : (
        <p>Enter the 6-digit code sent for this claim.</p>
      )}
      <OtpInput value={code} onChange={setCode} onComplete={handleComplete} status={status} />
      {error && <p className="error-text">{error}</p>}
      <button type="button" onClick={onCancel}>
        Cancel
      </button>
    </section>
  )
}
