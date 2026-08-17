import { useEffect, useState, type FormEvent } from 'react'
import { ApiError, getAuthorityLimit, upsertAuthorityLimit } from '../../lib/api'
import { RoleId, RoleName, type RoleIdValue } from '../../lib/roles'

const CONFIGURABLE_ROLE_IDS: RoleIdValue[] = [RoleId.Surveyor, RoleId.Approver]

export function AuthorityLimitsPage() {
  return (
    <div>
      <h1>Authority Limits</h1>
      <p>
        Controls the Surveyor maker-checker auto-finalize path: a Surveyor's
        Approve decision only finalizes immediately if the claim amount and
        risk score are both within their limit here. No row for a role means
        zero authority - everything escalates to an Approver.
      </p>

      {CONFIGURABLE_ROLE_IDS.map((roleId) => (
        <RoleLimitCard key={roleId} roleId={roleId} />
      ))}
    </div>
  )
}

function RoleLimitCard({ roleId }: { roleId: RoleIdValue }) {
  const [maxApprovalAmount, setMaxApprovalAmount] = useState('')
  const [maxRiskScore, setMaxRiskScore] = useState('')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [message, setMessage] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false

    getAuthorityLimit(roleId)
      .then((limit) => {
        if (cancelled) return
        setMaxApprovalAmount(limit?.maxApprovalAmount?.toString() ?? '')
        setMaxRiskScore(limit?.maxRiskScore?.toString() ?? '')
      })
      .catch((err: unknown) => {
        if (!cancelled) {
          setError(err instanceof ApiError ? err.message : 'Failed to load limit.')
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [roleId])

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    setSaving(true)
    setError(null)
    setMessage(null)

    try {
      await upsertAuthorityLimit(
        roleId,
        maxApprovalAmount ? Number(maxApprovalAmount) : null,
        maxRiskScore ? Number(maxRiskScore) : null,
      )
      setMessage('Saved.')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Failed to save limit.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <section className="card">
      <h2>{RoleName[roleId]}</h2>

      {loading ? (
        <p>Loading…</p>
      ) : (
        <form onSubmit={handleSubmit}>
          <label htmlFor={`amount-${roleId}`}>Max approval amount (₹, blank = no limit)</label>
          <input
            id={`amount-${roleId}`}
            type="number"
            min="0"
            step="0.01"
            value={maxApprovalAmount}
            onChange={(e) => setMaxApprovalAmount(e.target.value)}
          />

          <label htmlFor={`risk-${roleId}`}>Max risk score (0-100, blank = no limit)</label>
          <input
            id={`risk-${roleId}`}
            type="number"
            min="0"
            max="100"
            step="0.01"
            value={maxRiskScore}
            onChange={(e) => setMaxRiskScore(e.target.value)}
          />

          {error && <p className="error-text">{error}</p>}
          {message && <p className="success-text">{message}</p>}

          <button type="submit" disabled={saving}>
            {saving ? 'Saving…' : 'Save'}
          </button>
        </form>
      )}
    </section>
  )
}
