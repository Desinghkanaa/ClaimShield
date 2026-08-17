import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import {
  ApiError,
  getMyClaims,
  getMyCustomerProfile,
  getMyPolicies,
  getMyVehicles,
} from '../lib/api'
import type { ClaimResponseDto, PolicyResponseDto, VehicleResponseDto } from '../lib/types'
import { ClaimStatus, PolicyTypeName } from '../lib/statuses'
import { SkeletonBlock } from '../components/Skeleton'
import { ClaimStatusBadge, getClaimStatusTone } from '../components/StatusBadge'
import { HowItWorks } from '../components/HowItWorks'
import { InstantClaimBanner } from '../components/InstantClaimBanner'
import { FileText, Car, ClipboardList, Hash, Wallet, CalendarClock, FilePlus2 } from 'lucide-react'

function formatCurrency(amount: number | null) {
  return amount != null ? `₹ ${amount.toLocaleString('en-IN')}` : '—'
}

function formatDate(value: string) {
  return new Date(value).toLocaleDateString('en-IN')
}

export function CustomerDashboardPage() {
  const [claims, setClaims] = useState<ClaimResponseDto[] | null>(null)
  const [policies, setPolicies] = useState<PolicyResponseDto[] | null>(null)
  const [vehicles, setVehicles] = useState<VehicleResponseDto[] | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false

    getMyCustomerProfile()
      .then(async (customer) => {
        const [claimData, policyData, vehicleData] = await Promise.all([
          getMyClaims(customer.customerId),
          getMyPolicies(customer.customerId),
          getMyVehicles(customer.customerId),
        ])
        if (!cancelled) {
          setClaims(claimData)
          setPolicies(policyData)
          setVehicles(vehicleData)
        }
      })
      .catch((err: unknown) => {
        if (!cancelled) {
          setError(err instanceof ApiError ? err.message : 'Failed to load your dashboard.')
        }
      })

    return () => {
      cancelled = true
    }
  }, [])

  const loading = !claims || !policies || !vehicles

  const openClaims = claims?.filter((c) => c.statusId !== ClaimStatus.Closed) ?? []
  const activePolicy = policies?.find((p) => new Date(p.endDate) >= new Date())

  return (
    <div>
      <h1>Dashboard</h1>

      {error && <p className="error-text">{error}</p>}

      {!error && loading && (
        <section className="card">
          <SkeletonBlock lines={4} />
        </section>
      )}

      {!loading && (
        <>
          <div className="stat-cards">
            <div className="stat-card stat-card-blue">
              <span className="stat-card-icon stat-card-icon-blue">
                <FileText size={18} />
              </span>
              <p className="stat-card-label">Active policies</p>
              <p className="stat-card-value">
                {policies!.filter((p) => new Date(p.endDate) >= new Date()).length}
              </p>
            </div>
            <div className="stat-card stat-card-teal">
              <span className="stat-card-icon stat-card-icon-teal">
                <Car size={18} />
              </span>
              <p className="stat-card-label">Vehicles</p>
              <p className="stat-card-value">{vehicles!.length}</p>
            </div>
            <div className="stat-card stat-card-amber">
              <span className="stat-card-icon stat-card-icon-amber">
                <ClipboardList size={18} />
              </span>
              <p className="stat-card-label">Open claims</p>
              <p className="stat-card-value">{openClaims.length}</p>
            </div>
          </div>

          <InstantClaimBanner />

          <HowItWorks />

          {activePolicy &&
            (() => {
              const startMs = new Date(activePolicy.startDate).getTime()
              const endMs = new Date(activePolicy.endDate).getTime()
              const nowMs = Date.now()
              const totalDuration = endMs - startMs
              const remainingMs = Math.max(0, endMs - nowMs)
              const remainingPercent =
                totalDuration > 0 ? Math.min(100, (remainingMs / totalDuration) * 100) : 0
              const daysRemaining = Math.max(0, Math.ceil(remainingMs / (1000 * 60 * 60 * 24)))

              return (
                <section className="card card-tint-blue policy-highlight-card">
                  <h2>Your active policy</h2>
                  <span className="badge badge-icon badge-blue policy-type-chip">
                    <FileText size={13} />
                    {activePolicy.policyTypeId
                      ? (PolicyTypeName[activePolicy.policyTypeId] ?? 'Policy')
                      : 'Policy'}
                  </span>

                  <dl className="fact-grid fact-grid-rich">
                    <dt>
                      <span className="fact-icon fact-icon-blue">
                        <Hash size={14} />
                      </span>
                      Policy number
                    </dt>
                    <dd>{activePolicy.policyNumber}</dd>

                    <dt>
                      <span className="fact-icon fact-icon-teal">
                        <Wallet size={14} />
                      </span>
                      Coverage amount
                    </dt>
                    <dd>{formatCurrency(activePolicy.coverageAmount)}</dd>

                    <dt>
                      <span className="fact-icon fact-icon-amber">
                        <CalendarClock size={14} />
                      </span>
                      Valid until
                    </dt>
                    <dd>
                      {formatDate(activePolicy.endDate)}
                      <div className="policy-validity-bar">
                        <div
                          className="policy-validity-bar-fill"
                          style={{ width: `${remainingPercent}%` }}
                        />
                      </div>
                      <span className="policy-validity-days">
                        {daysRemaining} day{daysRemaining === 1 ? '' : 's'} remaining
                      </span>
                    </dd>
                  </dl>
                  <Link to="/my-policy">View full policy details →</Link>
                </section>
              )
            })()}

          <section className="card card-tint-blue">
            <h2>Recent claims</h2>
            {claims!.length === 0 && <p>You haven't raised any claims yet.</p>}
            {claims!.length > 0 && (
              <ul className="claim-row-list">
                {claims!.slice(0, 5).map((claim) => {
                  const tone = getClaimStatusTone(claim.statusId)
                  const claimDate = claim.reportedDate ?? claim.createdDate ?? claim.incidentDate

                  return (
                    <li key={claim.claimId} className={`claim-row claim-row-${tone}`}>
                      <FileText size={16} className="claim-row-icon" />
                      <span className="claim-row-body">
                        <Link to={`/my-claims/${claim.claimId}`}>{claim.claimNumber}</Link>
                        {claimDate && (
                          <span className="claim-row-date">Reported {formatDate(claimDate)}</span>
                        )}
                      </span>
                      <ClaimStatusBadge statusId={claim.statusId} />
                    </li>
                  )
                })}
              </ul>
            )}
            <p>
              <Link to="/my-claims/new" className="button-link">
                <FilePlus2 size={16} />
                Raise a new claim
              </Link>
            </p>
          </section>
        </>
      )}
    </div>
  )
}
