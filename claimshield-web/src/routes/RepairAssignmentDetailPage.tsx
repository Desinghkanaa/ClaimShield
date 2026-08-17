import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { Link, useParams } from 'react-router-dom'
import {
  ApiError,
  getClaim,
  getRepairAssignment,
  getRepairEstimatesByAssignment,
  submitRepairEstimate,
  updateRepairAssignmentStatus,
} from '../lib/api'
import type {
  ClaimResponseDto,
  RepairAssignmentResponseDto,
  RepairEstimateResponseDto,
} from '../lib/types'
import { AssignmentStatus, AssignmentStatusName, ClaimStatusName } from '../lib/statuses'

function formatDate(value: string | null) {
  return value ? new Date(value).toLocaleString() : '—'
}

function formatCurrency(amount: number | null) {
  return amount != null ? `₹ ${amount.toLocaleString('en-IN')}` : '—'
}

export function RepairAssignmentDetailPage() {
  const { repairAssignmentId } = useParams<{ repairAssignmentId: string }>()

  const [assignment, setAssignment] = useState<RepairAssignmentResponseDto | null>(null)
  const [claim, setClaim] = useState<ClaimResponseDto | null>(null)
  const [estimates, setEstimates] = useState<RepairEstimateResponseDto[]>([])
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [actionMessage, setActionMessage] = useState<string | null>(null)

  const loadAll = useCallback(async () => {
    if (!repairAssignmentId) return

    setLoading(true)
    setLoadError(null)

    try {
      const assignmentData = await getRepairAssignment(repairAssignmentId)
      const [claimData, estimateData] = await Promise.all([
        getClaim(assignmentData.claimId),
        getRepairEstimatesByAssignment(repairAssignmentId),
      ])

      setAssignment(assignmentData)
      setClaim(claimData)
      setEstimates(estimateData)
    } catch (err) {
      setLoadError(
        err instanceof ApiError ? err.message : 'Failed to load this repair assignment.',
      )
    } finally {
      setLoading(false)
    }
  }, [repairAssignmentId])

  useEffect(() => {
    void loadAll()
  }, [loadAll])

  if (loading) {
    return <p>Loading…</p>
  }

  if (loadError || !assignment || !claim) {
    return <p className="error-text">{loadError ?? 'Repair assignment not found.'}</p>
  }

  return (
    <div>
      <p>
        <Link to="/repairs">← Back to my repairs</Link>
      </p>

      <h1>{claim.claimNumber}</h1>

      <section className="card">
        <h2>Claim summary</h2>
        <dl className="fact-grid">
          <dt>Claim status</dt>
          <dd>{ClaimStatusName[claim.statusId ?? 0] ?? 'Unknown'}</dd>

          <dt>Estimated loss</dt>
          <dd>{formatCurrency(claim.estimatedLossAmount)}</dd>

          <dt>Approved amount</dt>
          <dd>{formatCurrency(claim.approvedAmount)}</dd>
        </dl>
        {claim.incidentDescription && (
          <>
            <h3>Description</h3>
            <p>{claim.incidentDescription}</p>
          </>
        )}
      </section>

      <section className="card">
        <h2>Assignment details</h2>
        <dl className="fact-grid">
          <dt>Status</dt>
          <dd>{AssignmentStatusName[assignment.assignmentStatusId] ?? 'Unknown'}</dd>

          <dt>Assigned date</dt>
          <dd>{formatDate(assignment.assignedDate)}</dd>

          <dt>Expected completion</dt>
          <dd>{formatDate(assignment.expectedCompletionDate)}</dd>
        </dl>
        {assignment.remarks && (
          <>
            <h3>Remarks</h3>
            <p>{assignment.remarks}</p>
          </>
        )}
        <AssignmentStatusControl
          assignment={assignment}
          onDone={(message) => {
            setActionMessage(message)
            void loadAll()
          }}
        />
      </section>

      {actionMessage && <p className="success-text banner">{actionMessage}</p>}

      <section className="card">
        <h2>Estimates</h2>
        {estimates.length === 0 && <p>No estimates submitted yet.</p>}
        {estimates.length > 0 && (
          <ul className="timeline">
            {estimates.map((estimate) => (
              <li key={estimate.repairEstimateId}>
                <strong>{formatCurrency(estimate.estimatedAmount)}</strong>{' '}
                submitted {formatDate(estimate.submittedDate)}
                {estimate.estimatedCompletionDays != null && (
                  <> · {estimate.estimatedCompletionDays} day(s) estimated</>
                )}{' '}
                <span className="badge">{estimate.approvalStatus ?? 'Pending'}</span>
                {estimate.approvedAmount != null && (
                  <p>Approved: {formatCurrency(estimate.approvedAmount)}</p>
                )}
                {estimate.approvalRemarks && (
                  <p>
                    <em>Reviewer note: {estimate.approvalRemarks}</em>
                  </p>
                )}
                {estimate.estimateRemarks && <p>{estimate.estimateRemarks}</p>}
              </li>
            ))}
          </ul>
        )}
      </section>

      <EstimateForm
        repairAssignmentId={assignment.repairAssignmentId}
        claimId={assignment.claimId}
        onDone={(message) => {
          setActionMessage(message)
          void loadAll()
        }}
      />
    </div>
  )
}

function AssignmentStatusControl({
  assignment,
  onDone,
}: {
  assignment: RepairAssignmentResponseDto
  onDone: (message: string) => void
}) {
  const [busy, setBusy] = useState(false)

  const setStatus = async (assignmentStatusId: number, label: string) => {
    setBusy(true)
    try {
      await updateRepairAssignmentStatus(assignment, assignmentStatusId)
      onDone(`Assignment marked as ${label}.`)
    } catch (err) {
      alert(err instanceof ApiError ? err.message : 'Failed to update status.')
    } finally {
      setBusy(false)
    }
  }

  if (assignment.assignmentStatusId === AssignmentStatus.Assigned) {
    return (
      <button
        type="button"
        disabled={busy}
        onClick={() => void setStatus(AssignmentStatus.InProgress, 'In Progress')}
      >
        Mark as In Progress
      </button>
    )
  }

  if (assignment.assignmentStatusId === AssignmentStatus.InProgress) {
    return (
      <button
        type="button"
        disabled={busy}
        onClick={() => void setStatus(AssignmentStatus.Completed, 'Completed')}
      >
        Mark as Completed
      </button>
    )
  }

  return null
}

function EstimateForm({
  repairAssignmentId,
  claimId,
  onDone,
}: {
  repairAssignmentId: string
  claimId: string
  onDone: (message: string) => void
}) {
  const [amount, setAmount] = useState('')
  const [days, setDays] = useState('')
  const [remarks, setRemarks] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    setSubmitting(true)
    setError(null)

    try {
      await submitRepairEstimate(
        repairAssignmentId,
        claimId,
        Number(amount),
        days ? Number(days) : null,
        remarks,
      )
      setAmount('')
      setDays('')
      setRemarks('')
      onDone('Estimate submitted.')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Failed to submit estimate.')
      setSubmitting(false)
    }
  }

  return (
    <section className="card">
      <h2>Submit an estimate</h2>
      <form onSubmit={handleSubmit}>
        <label htmlFor="amount">Estimated amount (₹)</label>
        <input
          id="amount"
          type="number"
          min="0"
          step="0.01"
          value={amount}
          onChange={(event) => setAmount(event.target.value)}
          required
        />

        <label htmlFor="days">Estimated completion (days)</label>
        <input
          id="days"
          type="number"
          min="0"
          value={days}
          onChange={(event) => setDays(event.target.value)}
        />

        <label htmlFor="remarks">Remarks</label>
        <textarea
          id="remarks"
          value={remarks}
          onChange={(event) => setRemarks(event.target.value)}
          rows={3}
        />

        {error && <p className="error-text">{error}</p>}

        <button type="submit" disabled={submitting}>
          {submitting ? 'Submitting…' : 'Submit estimate'}
        </button>
      </form>
    </section>
  )
}
