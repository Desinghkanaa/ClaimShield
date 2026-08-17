import { useCallback, useEffect, useRef, useState, type FormEvent } from 'react'
import { Link, useParams } from 'react-router-dom'
import {
  ApiError,
  closeClaim,
  getClaim,
  getClaimDocuments,
  getDocumentDownloadUrl,
  getMyClaimScore,
  getPaymentsByClaim,
  uploadClaimDocument,
} from '../lib/api'
import type {
  ClaimDocumentResponseDto,
  ClaimResponseDto,
  CustomerClaimScoreDto,
  PaymentResponseDto,
} from '../lib/types'
import { ClaimStatus, ClaimStatusName } from '../lib/statuses'

function formatCurrency(amount: number | null) {
  return amount != null ? `₹ ${amount.toLocaleString('en-IN')}` : '—'
}

function formatDate(value: string | null) {
  return value ? new Date(value).toLocaleString() : '—'
}

function BandBadge({ bandName }: { bandName: string }) {
  return (
    <span className={`band-badge band-${bandName.toLowerCase()}`}>
      {bandName}
    </span>
  )
}

export function MyClaimDetailPage() {
  const { claimId } = useParams<{ claimId: string }>()

  const [claim, setClaim] = useState<ClaimResponseDto | null>(null)
  const [score, setScore] = useState<CustomerClaimScoreDto | null>(null)
  const [documents, setDocuments] = useState<ClaimDocumentResponseDto[]>([])
  const [payments, setPayments] = useState<PaymentResponseDto[]>([])
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [actionMessage, setActionMessage] = useState<string | null>(null)

  const loadAll = useCallback(async () => {
    if (!claimId) return

    setLoading(true)
    setLoadError(null)

    try {
      const [claimData, scoreData, documentData, paymentData] = await Promise.all([
        getClaim(claimId),
        getMyClaimScore(claimId).catch(() => null),
        getClaimDocuments(claimId).catch(() => []),
        getPaymentsByClaim(claimId).catch(() => []),
      ])

      setClaim(claimData)
      setScore(scoreData)
      setDocuments(documentData)
      setPayments(paymentData)
    } catch (err) {
      setLoadError(err instanceof ApiError ? err.message : 'Failed to load this claim.')
    } finally {
      setLoading(false)
    }
  }, [claimId])

  useEffect(() => {
    void loadAll()
  }, [loadAll])

  const handleClose = async () => {
    if (!claimId) return

    try {
      const result = await closeClaim(claimId, 'Closed by customer.')
      setActionMessage(result.message)
      void loadAll()
    } catch (err) {
      setActionMessage(
        err instanceof ApiError ? err.message : 'Failed to close claim.',
      )
    }
  }

  if (loading) {
    return <p>Loading…</p>
  }

  if (loadError || !claim) {
    return <p className="error-text">{loadError ?? 'Claim not found.'}</p>
  }

  return (
    <div>
      <p>
        <Link to="/my-claims">← Back to my claims</Link>
      </p>

      <h1>{claim.claimNumber}</h1>

      <section className="card">
        <h2>Claim summary</h2>
        <dl className="fact-grid">
          <dt>Status</dt>
          <dd>{ClaimStatusName[claim.statusId ?? 0] ?? 'Unknown'}</dd>

          <dt>Incident date</dt>
          <dd>{formatDate(claim.incidentDate)}</dd>

          <dt>Approved amount</dt>
          <dd>{formatCurrency(claim.approvedAmount)}</dd>

          <dt>Location</dt>
          <dd>{claim.incidentLocation ?? '—'}</dd>
        </dl>
        {claim.incidentDescription && (
          <>
            <h3>Description</h3>
            <p>{claim.incidentDescription}</p>
          </>
        )}
      </section>

      {score && (
        <section className="card">
          <h2>
            Claim risk assessment <BandBadge bandName={score.compositeBandName} />
          </h2>
          <dl className="fact-grid">
            <dt>Score</dt>
            <dd>{score.compositeScore}</dd>

            <dt>Last assessed</dt>
            <dd>{formatDate(score.lastScoredAt)}</dd>
          </dl>
        </section>
      )}

      {payments.length > 0 && (
        <section className="card">
          <h2>Payment status</h2>
          {(() => {
            const latest = payments[0]
            return (
              <dl className="fact-grid">
                <dt>Status</dt>
                <dd>{latest.paymentStatus}</dd>

                <dt>Amount</dt>
                <dd>{formatCurrency(latest.amount)}</dd>

                <dt>Date</dt>
                <dd>{formatDate(latest.paymentDate)}</dd>
              </dl>
            )
          })()}
        </section>
      )}

      {actionMessage && <p className="success-text banner">{actionMessage}</p>}

      {claim.statusId === ClaimStatus.Settled && (
        <section className="card">
          <p>
            Your claim is Settled. You can close it once you're satisfied
            everything is resolved.
          </p>
          <button type="button" onClick={() => void handleClose()}>
            Close claim
          </button>
        </section>
      )}

      <DocumentsSection
        claimId={claim.claimId}
        documents={documents}
        onUploaded={loadAll}
      />
    </div>
  )
}

function DocumentsSection({
  claimId,
  documents,
  onUploaded,
}: {
  claimId: string
  documents: ClaimDocumentResponseDto[]
  onUploaded: () => void
}) {
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [uploading, setUploading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleUpload = async (event: FormEvent) => {
    event.preventDefault()

    const file = fileInputRef.current?.files?.[0]

    if (!file) {
      setError('Choose a file first.')
      return
    }

    setUploading(true)
    setError(null)

    try {
      await uploadClaimDocument(claimId, 1, file)
      if (fileInputRef.current) fileInputRef.current.value = ''
      onUploaded()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Failed to upload document.')
    } finally {
      setUploading(false)
    }
  }

  const handleDownload = async (claimDocumentId: string) => {
    try {
      const { url } = await getDocumentDownloadUrl(claimDocumentId)
      window.open(url, '_blank', 'noopener,noreferrer')
    } catch (err) {
      alert(err instanceof ApiError ? err.message : 'Failed to get download link.')
    }
  }

  return (
    <section className="card">
      <h2>Documents</h2>

      {documents.length === 0 && <p>No documents uploaded yet.</p>}

      {documents.length > 0 && (
        <ul className="document-list">
          {documents.map((doc) => (
            <li key={doc.claimDocumentId}>
              {doc.originalFileName}{' '}
              <button type="button" onClick={() => void handleDownload(doc.claimDocumentId)}>
                Download
              </button>
            </li>
          ))}
        </ul>
      )}

      <form onSubmit={handleUpload}>
        <label htmlFor="file">Upload a supporting document</label>
        <input id="file" type="file" ref={fileInputRef} />

        {error && <p className="error-text">{error}</p>}

        <button type="submit" disabled={uploading}>
          {uploading ? 'Uploading…' : 'Upload'}
        </button>
      </form>
    </section>
  )
}
