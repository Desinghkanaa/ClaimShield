import { useEffect, useState } from 'react'
import { motion } from 'framer-motion'
import {
  ClipboardList,
  MapPin,
  Gauge,
  AlertTriangle,
  Wrench,
  Calculator,
  CheckCircle2,
  Camera,
  Activity,
  Plus,
  Trash2,
  Save,
  Eye,
  Send,
} from 'lucide-react'
import {
  ApiError,
  getSurveyAssessment,
  getSurveyAssignmentsByClaim,
  getClaimDocuments,
  getAuditLogsForClaim,
  saveSurveyAssessmentDraft,
  completeSurveyAssessment,
} from '../lib/api'
import type {
  ClaimResponseDto,
  SurveyAssessmentResponseDto,
  SurveyAssignmentResponseDto,
  ClaimDocumentResponseDto,
  AuditLogResponseDto,
  DamageAssessmentItemRequest,
  SaveSurveyAssessmentRequest,
} from '../lib/types'
import { RoleId, type RoleIdValue } from '../lib/roles'
import {
  AssessmentStatus,
  ASSESSMENT_STEP_LABELS,
  VehicleConditionName,
  RepairabilityStatusName,
  SurveyorRecommendationName,
  DamageCategoryName,
  DamageSeverityName,
  LossTypeName,
  InspectionModeName,
  ClaimStatusName,
  DocumentType,
} from '../lib/statuses'
import { WizardProgress } from './WizardShell'
import { Modal } from './Modal'
import { SkeletonBlock } from './Skeleton'
import { UploadCard } from './UploadCard'
import { useToast } from '../context/ToastContext'

function formatDate(value: string | null | undefined) {
  return value ? new Date(value).toLocaleDateString('en-IN') : '—'
}

function formatCurrency(amount: number | null | undefined) {
  return amount != null ? `₹ ${amount.toLocaleString('en-IN')}` : '—'
}

function toDateInputValue(iso: string | null | undefined): string {
  if (!iso) return new Date().toISOString().slice(0, 10)
  return iso.slice(0, 10)
}

function numToStr(n: number | null | undefined): string {
  return n == null ? '' : String(n)
}

function num(s: string): number {
  const trimmed = s.trim()
  return trimmed === '' ? 0 : Number(trimmed)
}

interface FormState {
  inspectionDate: string
  surveyLocation: string
  surveyRemarks: string
  assessmentStatusId: number

  vehicleConditionId: string
  odometerReading: string
  preExistingDamageNotes: string
  damageTypeId: string
  damageDescription: string
  repairabilityStatusId: string
  totalLoss: boolean

  estimatedRepairerName: string
  labourCost: string
  partsCost: string
  towingCharges: string
  paintCost: string
  estimatedDurationDays: string

  taxAmount: string
  depreciationAmount: string
  compulsoryExcess: string
  salvageAmount: string

  repairRecommended: boolean
  replaceRecommended: boolean
  cashSettlementRecommended: boolean
  totalLossRecommended: boolean
  overallRecommendationId: string
  assessmentRemarks: string
}

function buildInitialForm(
  assessment: SurveyAssessmentResponseDto | null,
  claim: ClaimResponseDto,
): FormState {
  return {
    inspectionDate: toDateInputValue(assessment?.inspectionDate),
    surveyLocation: assessment?.surveyLocation ?? '',
    surveyRemarks: assessment?.surveyRemarks ?? '',
    assessmentStatusId: assessment?.assessmentStatusId ?? AssessmentStatus.Assigned,

    vehicleConditionId: numToStr(assessment?.vehicleConditionId),
    odometerReading: numToStr(assessment?.odometerReading),
    preExistingDamageNotes: assessment?.preExistingDamageNotes ?? '',
    damageTypeId: assessment ? String(assessment.damageTypeId) : numToStr(claim.lossTypeId),
    damageDescription: assessment?.damageDescription ?? '',
    repairabilityStatusId: numToStr(assessment?.repairabilityStatusId),
    totalLoss: assessment?.totalLoss ?? false,

    estimatedRepairerName: assessment?.estimatedRepairerName ?? '',
    labourCost: numToStr(assessment?.labourCost),
    partsCost: numToStr(assessment?.partsCost),
    towingCharges: numToStr(assessment?.towingCharges),
    paintCost: numToStr(assessment?.paintCost),
    estimatedDurationDays: numToStr(assessment?.estimatedDurationDays),

    taxAmount: numToStr(assessment?.taxAmount),
    depreciationAmount: numToStr(assessment?.depreciationAmount),
    compulsoryExcess: numToStr(assessment?.compulsoryExcess),
    salvageAmount: numToStr(assessment?.salvageAmount),

    repairRecommended: assessment?.repairRecommended ?? false,
    replaceRecommended: assessment?.replaceRecommended ?? false,
    cashSettlementRecommended: assessment?.cashSettlementRecommended ?? false,
    totalLossRecommended: assessment?.totalLossRecommended ?? false,
    overallRecommendationId: numToStr(assessment?.overallRecommendationId),
    assessmentRemarks: assessment?.assessmentRemarks ?? '',
  }
}

export function SurveyAssessment({
  claim,
  roleId,
  currentUserId,
}: {
  claim: ClaimResponseDto
  roleId: RoleIdValue | null
  currentUserId: string | null
}) {
  const { showToast } = useToast()

  const [loaded, setLoaded] = useState(false)
  const [assessment, setAssessment] = useState<SurveyAssessmentResponseDto | null>(null)
  const [assignments, setAssignments] = useState<SurveyAssignmentResponseDto[]>([])
  const [documents, setDocuments] = useState<ClaimDocumentResponseDto[]>([])
  const [auditLogs, setAuditLogs] = useState<AuditLogResponseDto[]>([])

  const [form, setForm] = useState<FormState>(() => buildInitialForm(null, claim))
  const [damageItems, setDamageItems] = useState<DamageAssessmentItemRequest[]>([])
  const [damagePhotoSlots, setDamagePhotoSlots] = useState(1)
  const [supportingDocSlots, setSupportingDocSlots] = useState(1)

  const [saving, setSaving] = useState(false)
  const [completing, setCompleting] = useState(false)
  const [showPreview, setShowPreview] = useState(false)
  const [showCompleteConfirm, setShowCompleteConfirm] = useState(false)

  useEffect(() => {
    let cancelled = false

    async function load() {
      try {
        const [assessmentData, assignmentData, docData, logData] = await Promise.all([
          getSurveyAssessment(claim.claimId).catch(() => null),
          getSurveyAssignmentsByClaim(claim.claimId).catch(() => []),
          getClaimDocuments(claim.claimId).catch(() => []),
          getAuditLogsForClaim(claim.claimId).catch(() => []),
        ])

        if (cancelled) return

        setAssessment(assessmentData)
        setAssignments(assignmentData)
        setDocuments(docData)
        setAuditLogs(logData)
        setForm(buildInitialForm(assessmentData, claim))
        setDamageItems(
          assessmentData?.damageAssessmentItems.map((item) => ({
            componentName: item.componentName,
            damageCategoryId: item.damageCategoryId,
            severityId: item.severityId,
            repairRequired: item.repairRequired,
            replacementRequired: item.replacementRequired,
            remarks: item.remarks,
          })) ?? [],
        )
      } finally {
        if (!cancelled) setLoaded(true)
      }
    }

    void load()

    return () => {
      cancelled = true
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [claim.claimId])

  if (roleId !== RoleId.Surveyor && roleId !== RoleId.Approver && roleId !== RoleId.Admin) {
    return null
  }

  const myAssignment = assignments.find((a) => a.surveyorId === currentUserId) ?? null

  const editable =
    roleId === RoleId.Surveyor &&
    !!myAssignment &&
    (assessment == null || assessment.surveyorId === currentUserId) &&
    (assessment == null || assessment.assessmentStatusId < AssessmentStatus.SubmittedForReview)

  const surveyTypeId = assessment?.surveyTypeId ?? myAssignment?.inspectionMode ?? null

  const previewGross = num(form.labourCost) + num(form.paintCost) + num(form.partsCost) + num(form.taxAmount)
  const previewNet = Math.max(
    0,
    previewGross -
      num(form.depreciationAmount) -
      num(form.compulsoryExcess) -
      num(form.salvageAmount) +
      num(form.towingCharges),
  )

  if (!loaded) {
    return (
      <section className="card survey-loading-card">
        <SkeletonBlock lines={5} />
      </section>
    )
  }

  if (!myAssignment && roleId === RoleId.Surveyor) {
    return null
  }

  const buildRequest = (): SaveSurveyAssessmentRequest | null => {
    if (!myAssignment || !currentUserId) return null

    if (!form.damageTypeId) {
      showToast('Select a damage type before saving.', 'error')
      return null
    }

    return {
      surveyAssignmentId: assessment?.surveyAssignmentId ?? myAssignment.surveyAssignmentId,
      claimId: claim.claimId,
      surveyorId: currentUserId,

      inspectionDate: new Date(form.inspectionDate).toISOString(),
      surveyLocation: form.surveyLocation || null,
      surveyRemarks: form.surveyRemarks || null,
      assessmentStatusId: form.assessmentStatusId,

      vehicleConditionId: form.vehicleConditionId ? Number(form.vehicleConditionId) : null,
      odometerReading: form.odometerReading ? Number(form.odometerReading) : null,
      preExistingDamageNotes: form.preExistingDamageNotes || null,
      damageTypeId: Number(form.damageTypeId),
      damageDescription: form.damageDescription || null,
      repairabilityStatusId: form.repairabilityStatusId ? Number(form.repairabilityStatusId) : null,
      totalLoss: form.totalLoss,

      damageAssessmentItems: damageItems.filter((item) => item.componentName.trim() !== ''),

      estimatedRepairerName: form.estimatedRepairerName || null,
      labourCost: form.labourCost ? Number(form.labourCost) : null,
      partsCost: form.partsCost ? Number(form.partsCost) : null,
      towingCharges: form.towingCharges ? Number(form.towingCharges) : null,
      paintCost: form.paintCost ? Number(form.paintCost) : null,
      estimatedDurationDays: form.estimatedDurationDays ? Number(form.estimatedDurationDays) : null,

      taxAmount: form.taxAmount ? Number(form.taxAmount) : null,
      depreciationAmount: form.depreciationAmount ? Number(form.depreciationAmount) : null,
      compulsoryExcess: form.compulsoryExcess ? Number(form.compulsoryExcess) : null,
      salvageAmount: form.salvageAmount ? Number(form.salvageAmount) : null,

      repairRecommended: form.repairRecommended,
      replaceRecommended: form.replaceRecommended,
      cashSettlementRecommended: form.cashSettlementRecommended,
      totalLossRecommended: form.totalLossRecommended,
      overallRecommendationId: form.overallRecommendationId ? Number(form.overallRecommendationId) : null,
      assessmentRemarks: form.assessmentRemarks || null,
    }
  }

  const handleSaveDraft = async () => {
    const req = buildRequest()
    if (!req) return

    setSaving(true)
    try {
      const result = await saveSurveyAssessmentDraft(req)
      setAssessment(result)
      showToast('Assessment saved as draft.', 'success')
    } catch (err) {
      showToast(err instanceof ApiError ? err.message : 'Failed to save draft.', 'error')
    } finally {
      setSaving(false)
    }
  }

  const handleComplete = async () => {
    setCompleting(true)
    try {
      let current = assessment
      const req = buildRequest()

      if (req) {
        current = await saveSurveyAssessmentDraft(req)
        setAssessment(current)
      }

      if (!current) {
        throw new ApiError(400, 'Save the assessment before completing it.')
      }

      const result = await completeSurveyAssessment(current.surveyReportId)
      setAssessment(result)
      setShowCompleteConfirm(false)
      showToast('Assessment completed and submitted for review.', 'success')

      const refreshedLogs = await getAuditLogsForClaim(claim.claimId).catch(() => [])
      setAuditLogs(refreshedLogs)
    } catch (err) {
      showToast(err instanceof ApiError ? err.message : 'Failed to complete assessment.', 'error')
    } finally {
      setCompleting(false)
    }
  }

  const addDamageItem = () =>
    setDamageItems((items) => [
      ...items,
      {
        componentName: '',
        damageCategoryId: null,
        severityId: null,
        repairRequired: false,
        replacementRequired: false,
        remarks: null,
      },
    ])

  const removeDamageItem = (index: number) =>
    setDamageItems((items) => items.filter((_, i) => i !== index))

  const updateDamageItem = (index: number, patch: Partial<DamageAssessmentItemRequest>) =>
    setDamageItems((items) => items.map((item, i) => (i === index ? { ...item, ...patch } : item)))

  const docsFor = (documentTypeId: number) =>
    documents.filter((d) => d.documentTypeId === documentTypeId)

  return (
    <div className="survey-assessment">
      <section className="card survey-stepper-card">
        <WizardProgress
          currentStep={form.assessmentStatusId}
          labels={ASSESSMENT_STEP_LABELS}
        />
      </section>

      <section className="card survey-section survey-claim-header">
        <div className="survey-section-title">
          <span className="survey-section-icon"><ClipboardList size={17} /></span>
          <h2>Claim Information</h2>
        </div>
        <dl className="survey-fact-grid">
          <dt>Claim Number</dt>
          <dd>{claim.claimNumber}</dd>
          <dt>Customer</dt>
          <dd>{claim.customerName ?? '—'}</dd>
          <dt>Policy Number</dt>
          <dd>{claim.policyNumber ?? '—'}</dd>
          <dt>Vehicle Number</dt>
          <dd>{claim.vehicleRegistrationNumber ?? '—'}</dd>
          <dt>Claim Type</dt>
          <dd>{claim.lossTypeId ? LossTypeName[claim.lossTypeId] : '—'}</dd>
          <dt>Date of Loss</dt>
          <dd>{formatDate(claim.incidentDate)}</dd>
          <dt>Date of Intimation</dt>
          <dd>{formatDate(claim.reportedDate)}</dd>
          <dt>Current Status</dt>
          <dd>{claim.statusId ? ClaimStatusName[claim.statusId] : '—'}</dd>
        </dl>
      </section>

      <section className="card survey-section">
        <div className="survey-section-title">
          <span className="survey-section-icon"><MapPin size={17} /></span>
          <h2>Survey Information</h2>
        </div>
        <div className="survey-grid">
          <div className="survey-field">
            <label>Surveyor</label>
            <input value={assessment?.surveyorName ?? '—'} disabled />
          </div>
          <div className="survey-field">
            <label htmlFor="survey-date">Survey Date</label>
            <input
              id="survey-date"
              type="date"
              value={form.inspectionDate}
              disabled={!editable}
              onChange={(e) => setForm((f) => ({ ...f, inspectionDate: e.target.value }))}
            />
          </div>
          <div className="survey-field">
            <label htmlFor="survey-location">Location</label>
            <input
              id="survey-location"
              value={form.surveyLocation}
              disabled={!editable}
              placeholder="Survey site / workshop address"
              onChange={(e) => setForm((f) => ({ ...f, surveyLocation: e.target.value }))}
            />
          </div>
          <div className="survey-field">
            <label>Survey Type</label>
            <input value={surveyTypeId ? InspectionModeName[surveyTypeId] : '—'} disabled />
          </div>
          <div className="survey-field">
            <label htmlFor="survey-status">Status</label>
            <select
              id="survey-status"
              value={form.assessmentStatusId}
              disabled={!editable}
              onChange={(e) =>
                setForm((f) => ({ ...f, assessmentStatusId: Number(e.target.value) }))
              }
            >
              {ASSESSMENT_STEP_LABELS.slice(0, 6).map((label, i) => (
                <option key={label} value={i + 1}>
                  {label}
                </option>
              ))}
            </select>
          </div>
          <div className="survey-field survey-field-wide">
            <label htmlFor="survey-remarks">Remarks</label>
            <textarea
              id="survey-remarks"
              rows={2}
              value={form.surveyRemarks}
              disabled={!editable}
              onChange={(e) => setForm((f) => ({ ...f, surveyRemarks: e.target.value }))}
            />
          </div>
        </div>
      </section>

      <section className="card survey-section">
        <div className="survey-section-title">
          <span className="survey-section-icon"><Gauge size={17} /></span>
          <h2>Vehicle Inspection Details</h2>
        </div>
        <div className="survey-grid">
          <div className="survey-field">
            <label htmlFor="vehicle-condition">Vehicle Condition</label>
            <select
              id="vehicle-condition"
              value={form.vehicleConditionId}
              disabled={!editable}
              onChange={(e) => setForm((f) => ({ ...f, vehicleConditionId: e.target.value }))}
            >
              <option value="">Select…</option>
              {Object.entries(VehicleConditionName).map(([id, label]) => (
                <option key={id} value={id}>
                  {label}
                </option>
              ))}
            </select>
          </div>
          <div className="survey-field">
            <label htmlFor="odometer">Odometer Reading (km)</label>
            <input
              id="odometer"
              type="number"
              min="0"
              value={form.odometerReading}
              disabled={!editable}
              onChange={(e) => setForm((f) => ({ ...f, odometerReading: e.target.value }))}
            />
          </div>
          <div className="survey-field">
            <label htmlFor="damage-type">Damage Type</label>
            <select
              id="damage-type"
              value={form.damageTypeId}
              disabled={!editable}
              onChange={(e) => setForm((f) => ({ ...f, damageTypeId: e.target.value }))}
            >
              <option value="">Select…</option>
              {Object.entries(LossTypeName).map(([id, label]) => (
                <option key={id} value={id}>
                  {label}
                </option>
              ))}
            </select>
          </div>
          <div className="survey-field">
            <label htmlFor="repairability">Repairability Status</label>
            <select
              id="repairability"
              value={form.repairabilityStatusId}
              disabled={!editable}
              onChange={(e) => setForm((f) => ({ ...f, repairabilityStatusId: e.target.value }))}
            >
              <option value="">Select…</option>
              {Object.entries(RepairabilityStatusName).map(([id, label]) => (
                <option key={id} value={id}>
                  {label}
                </option>
              ))}
            </select>
          </div>
          <div className="survey-field survey-field-checkbox">
            <label>
              <input
                type="checkbox"
                checked={form.totalLoss}
                disabled={!editable}
                onChange={(e) => setForm((f) => ({ ...f, totalLoss: e.target.checked }))}
              />
              Total Loss Indicator
            </label>
          </div>
          <div className="survey-field survey-field-wide">
            <label htmlFor="pre-existing-damage">Pre-existing Damage Notes</label>
            <textarea
              id="pre-existing-damage"
              rows={2}
              value={form.preExistingDamageNotes}
              disabled={!editable}
              onChange={(e) => setForm((f) => ({ ...f, preExistingDamageNotes: e.target.value }))}
            />
          </div>
          <div className="survey-field survey-field-wide">
            <label htmlFor="accident-damage">Accident-related Damage</label>
            <textarea
              id="accident-damage"
              rows={2}
              value={form.damageDescription}
              disabled={!editable}
              onChange={(e) => setForm((f) => ({ ...f, damageDescription: e.target.value }))}
            />
          </div>
        </div>
      </section>

      <section className="card survey-section">
        <div className="survey-section-title">
          <span className="survey-section-icon"><AlertTriangle size={17} /></span>
          <h2>Damage Assessment</h2>
        </div>
        <div className="survey-damage-table-wrap">
          <table className="survey-damage-table">
            <thead>
              <tr>
                <th>Component</th>
                <th>Category</th>
                <th>Severity</th>
                <th>Repair</th>
                <th>Replace</th>
                <th>Remarks</th>
                {editable && <th></th>}
              </tr>
            </thead>
            <tbody>
              {damageItems.length === 0 && (
                <tr>
                  <td colSpan={7} className="survey-damage-empty">
                    No damaged components added yet.
                  </td>
                </tr>
              )}
              {damageItems.map((item, index) => (
                <tr key={index}>
                  <td>
                    <input
                      value={item.componentName}
                      disabled={!editable}
                      placeholder="e.g. Front Bumper"
                      onChange={(e) => updateDamageItem(index, { componentName: e.target.value })}
                    />
                  </td>
                  <td>
                    <select
                      value={item.damageCategoryId ?? ''}
                      disabled={!editable}
                      onChange={(e) =>
                        updateDamageItem(index, {
                          damageCategoryId: e.target.value ? Number(e.target.value) : null,
                        })
                      }
                    >
                      <option value="">—</option>
                      {Object.entries(DamageCategoryName).map(([id, label]) => (
                        <option key={id} value={id}>
                          {label}
                        </option>
                      ))}
                    </select>
                  </td>
                  <td>
                    <select
                      value={item.severityId ?? ''}
                      disabled={!editable}
                      onChange={(e) =>
                        updateDamageItem(index, {
                          severityId: e.target.value ? Number(e.target.value) : null,
                        })
                      }
                    >
                      <option value="">—</option>
                      {Object.entries(DamageSeverityName).map(([id, label]) => (
                        <option key={id} value={id}>
                          {label}
                        </option>
                      ))}
                    </select>
                  </td>
                  <td className="survey-damage-checkbox-cell">
                    <input
                      type="checkbox"
                      checked={item.repairRequired}
                      disabled={!editable}
                      onChange={(e) => updateDamageItem(index, { repairRequired: e.target.checked })}
                    />
                  </td>
                  <td className="survey-damage-checkbox-cell">
                    <input
                      type="checkbox"
                      checked={item.replacementRequired}
                      disabled={!editable}
                      onChange={(e) =>
                        updateDamageItem(index, { replacementRequired: e.target.checked })
                      }
                    />
                  </td>
                  <td>
                    <input
                      value={item.remarks ?? ''}
                      disabled={!editable}
                      onChange={(e) => updateDamageItem(index, { remarks: e.target.value })}
                    />
                  </td>
                  {editable && (
                    <td>
                      <button
                        type="button"
                        className="survey-icon-button survey-icon-button-danger"
                        onClick={() => removeDamageItem(index)}
                        title="Remove row"
                      >
                        <Trash2 size={14} />
                      </button>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {editable && (
          <button type="button" className="survey-add-row-button" onClick={addDamageItem}>
            <Plus size={14} /> Add component
          </button>
        )}
      </section>

      <section className="card survey-section">
        <div className="survey-section-title">
          <span className="survey-section-icon"><Wrench size={17} /></span>
          <h2>Repair Estimate Details</h2>
        </div>
        <div className="survey-grid">
          <div className="survey-field survey-field-wide">
            <label htmlFor="repairer-name">Repairer / Workshop</label>
            <input
              id="repairer-name"
              value={form.estimatedRepairerName}
              disabled={!editable}
              onChange={(e) => setForm((f) => ({ ...f, estimatedRepairerName: e.target.value }))}
            />
          </div>
          <div className="survey-field">
            <label htmlFor="labour-cost">Labour Cost (₹)</label>
            <input
              id="labour-cost"
              type="number"
              min="0"
              value={form.labourCost}
              disabled={!editable}
              onChange={(e) => setForm((f) => ({ ...f, labourCost: e.target.value }))}
            />
          </div>
          <div className="survey-field">
            <label htmlFor="parts-cost">Parts Cost (₹)</label>
            <input
              id="parts-cost"
              type="number"
              min="0"
              value={form.partsCost}
              disabled={!editable}
              onChange={(e) => setForm((f) => ({ ...f, partsCost: e.target.value }))}
            />
          </div>
          <div className="survey-field">
            <label htmlFor="towing-charges">Towing Charges (₹)</label>
            <input
              id="towing-charges"
              type="number"
              min="0"
              value={form.towingCharges}
              disabled={!editable}
              onChange={(e) => setForm((f) => ({ ...f, towingCharges: e.target.value }))}
            />
          </div>
          <div className="survey-field">
            <label htmlFor="paint-cost">Paint Cost (₹)</label>
            <input
              id="paint-cost"
              type="number"
              min="0"
              value={form.paintCost}
              disabled={!editable}
              onChange={(e) => setForm((f) => ({ ...f, paintCost: e.target.value }))}
            />
          </div>
          <div className="survey-field">
            <label htmlFor="est-duration">Estimated Duration (days)</label>
            <input
              id="est-duration"
              type="number"
              min="0"
              value={form.estimatedDurationDays}
              disabled={!editable}
              onChange={(e) => setForm((f) => ({ ...f, estimatedDurationDays: e.target.value }))}
            />
          </div>
          <div className="survey-field">
            <label>Estimated Repair Amount</label>
            <input value={formatCurrency(assessment?.estimatedRepairCost)} disabled />
          </div>
        </div>
      </section>

      <section className="card survey-section survey-section-computation">
        <div className="survey-section-title">
          <span className="survey-section-icon"><Calculator size={17} /></span>
          <h2>Assessment Computation</h2>
        </div>
        <div className="survey-grid">
          <div className="survey-field">
            <label>Labour Amount (₹)</label>
            <input value={formatCurrency(num(form.labourCost))} disabled />
          </div>
          <div className="survey-field">
            <label>Parts Amount (₹)</label>
            <input value={formatCurrency(num(form.partsCost))} disabled />
          </div>
          <div className="survey-field">
            <label htmlFor="tax-amount">Tax (₹)</label>
            <input
              id="tax-amount"
              type="number"
              min="0"
              value={form.taxAmount}
              disabled={!editable}
              onChange={(e) => setForm((f) => ({ ...f, taxAmount: e.target.value }))}
            />
          </div>
          <div className="survey-field">
            <label htmlFor="depreciation">Depreciation (₹)</label>
            <input
              id="depreciation"
              type="number"
              min="0"
              value={form.depreciationAmount}
              disabled={!editable}
              onChange={(e) => setForm((f) => ({ ...f, depreciationAmount: e.target.value }))}
            />
          </div>
          <div className="survey-field">
            <label htmlFor="excess">Compulsory Excess (₹)</label>
            <input
              id="excess"
              type="number"
              min="0"
              value={form.compulsoryExcess}
              disabled={!editable}
              onChange={(e) => setForm((f) => ({ ...f, compulsoryExcess: e.target.value }))}
            />
          </div>
          <div className="survey-field">
            <label htmlFor="salvage">Salvage Amount (₹)</label>
            <input
              id="salvage"
              type="number"
              min="0"
              value={form.salvageAmount}
              disabled={!editable}
              onChange={(e) => setForm((f) => ({ ...f, salvageAmount: e.target.value }))}
            />
          </div>
          <div className="survey-field">
            <label>Towing Amount (₹)</label>
            <input value={formatCurrency(num(form.towingCharges))} disabled />
          </div>
        </div>
        <div className="survey-computation-summary">
          <div className="survey-computation-line">
            <span>Gross Assessment Amount</span>
            <strong>{formatCurrency(assessment?.grossAssessmentAmount ?? previewGross)}</strong>
          </div>
          <div className="survey-computation-line survey-computation-net">
            <span>Net Assessment Amount</span>
            <strong>{formatCurrency(assessment?.netAssessmentAmount ?? previewNet)}</strong>
          </div>
        </div>
      </section>

      <section className="card survey-section">
        <div className="survey-section-title">
          <span className="survey-section-icon"><CheckCircle2 size={17} /></span>
          <h2>Assessment Recommendation</h2>
        </div>
        <div className="survey-recommendation-checks">
          <label>
            <input
              type="checkbox"
              checked={form.repairRecommended}
              disabled={!editable}
              onChange={(e) => setForm((f) => ({ ...f, repairRecommended: e.target.checked }))}
            />
            Repair Recommended
          </label>
          <label>
            <input
              type="checkbox"
              checked={form.replaceRecommended}
              disabled={!editable}
              onChange={(e) => setForm((f) => ({ ...f, replaceRecommended: e.target.checked }))}
            />
            Replace Recommended
          </label>
          <label>
            <input
              type="checkbox"
              checked={form.cashSettlementRecommended}
              disabled={!editable}
              onChange={(e) =>
                setForm((f) => ({ ...f, cashSettlementRecommended: e.target.checked }))
              }
            />
            Cash Settlement Recommended
          </label>
          <label>
            <input
              type="checkbox"
              checked={form.totalLossRecommended}
              disabled={!editable}
              onChange={(e) => setForm((f) => ({ ...f, totalLossRecommended: e.target.checked }))}
            />
            Total Loss Recommended
          </label>
        </div>
        <div className="survey-grid">
          <div className="survey-field">
            <label htmlFor="overall-recommendation">Overall Recommendation</label>
            <select
              id="overall-recommendation"
              value={form.overallRecommendationId}
              disabled={!editable}
              onChange={(e) => setForm((f) => ({ ...f, overallRecommendationId: e.target.value }))}
            >
              <option value="">Select…</option>
              {Object.entries(SurveyorRecommendationName).map(([id, label]) => (
                <option key={id} value={id}>
                  {label}
                </option>
              ))}
            </select>
          </div>
          <div className="survey-field survey-field-wide">
            <label htmlFor="assessment-remarks">Assessment Remarks</label>
            <textarea
              id="assessment-remarks"
              rows={2}
              value={form.assessmentRemarks}
              disabled={!editable}
              onChange={(e) => setForm((f) => ({ ...f, assessmentRemarks: e.target.value }))}
            />
          </div>
        </div>
      </section>

      <section className="card survey-section">
        <div className="survey-section-title">
          <span className="survey-section-icon"><Camera size={17} /></span>
          <h2>Photos &amp; Documents</h2>
        </div>

        <h3 className="survey-doc-group-title">Vehicle Photos</h3>
        <div className="upload-grid">
          <UploadCard
            label="Front"
            claimId={claim.claimId}
            documentTypeId={DocumentType.VehicleFront}
            onUploaded={(doc) => setDocuments((d) => [...d, doc])}
          />
          <UploadCard
            label="Left"
            claimId={claim.claimId}
            documentTypeId={DocumentType.VehicleLeft}
            onUploaded={(doc) => setDocuments((d) => [...d, doc])}
          />
          <UploadCard
            label="Back"
            claimId={claim.claimId}
            documentTypeId={DocumentType.VehicleBack}
            onUploaded={(doc) => setDocuments((d) => [...d, doc])}
          />
          <UploadCard
            label="Right"
            claimId={claim.claimId}
            documentTypeId={DocumentType.VehicleRight}
            onUploaded={(doc) => setDocuments((d) => [...d, doc])}
          />
        </div>

        <h3 className="survey-doc-group-title">Damage Photos</h3>
        {docsFor(DocumentType.DamagePhoto).length > 0 && (
          <ul className="survey-doc-list">
            {docsFor(DocumentType.DamagePhoto).map((doc) => (
              <li key={doc.claimDocumentId}>{doc.originalFileName}</li>
            ))}
          </ul>
        )}
        {editable && (
          <>
            <div className="upload-grid">
              {Array.from({ length: damagePhotoSlots }, (_, i) => (
                <UploadCard
                  key={`damage-${i}`}
                  label={`Damage Photo ${i + 1}`}
                  claimId={claim.claimId}
                  documentTypeId={DocumentType.DamagePhoto}
                  onUploaded={(doc) => setDocuments((d) => [...d, doc])}
                />
              ))}
            </div>
            <button
              type="button"
              className="survey-add-row-button"
              onClick={() => setDamagePhotoSlots((n) => n + 1)}
            >
              <Plus size={14} /> Add another photo
            </button>
          </>
        )}

        <h3 className="survey-doc-group-title">Repair Estimate / Workshop Quotation / Survey Report</h3>
        <div className="upload-grid">
          <UploadCard
            label="Repair Estimate"
            claimId={claim.claimId}
            documentTypeId={DocumentType.RepairEstimateDocument}
            onUploaded={(doc) => setDocuments((d) => [...d, doc])}
          />
          <UploadCard
            label="Workshop Quotation"
            claimId={claim.claimId}
            documentTypeId={DocumentType.WorkshopQuotation}
            onUploaded={(doc) => setDocuments((d) => [...d, doc])}
          />
          <UploadCard
            label="Survey Report"
            claimId={claim.claimId}
            documentTypeId={DocumentType.SurveyReportDocument}
            onUploaded={(doc) => setDocuments((d) => [...d, doc])}
          />
        </div>

        <h3 className="survey-doc-group-title">Supporting Documents</h3>
        {docsFor(DocumentType.Other).length > 0 && (
          <ul className="survey-doc-list">
            {docsFor(DocumentType.Other).map((doc) => (
              <li key={doc.claimDocumentId}>{doc.originalFileName}</li>
            ))}
          </ul>
        )}
        {editable && (
          <>
            <div className="upload-grid">
              {Array.from({ length: supportingDocSlots }, (_, i) => (
                <UploadCard
                  key={`support-${i}`}
                  label={`Supporting Doc ${i + 1}`}
                  claimId={claim.claimId}
                  documentTypeId={DocumentType.Other}
                  onUploaded={(doc) => setDocuments((d) => [...d, doc])}
                />
              ))}
            </div>
            <button
              type="button"
              className="survey-add-row-button"
              onClick={() => setSupportingDocSlots((n) => n + 1)}
            >
              <Plus size={14} /> Add another document
            </button>
          </>
        )}
      </section>

      {auditLogs.length > 0 && (
        <section className="card survey-section">
          <div className="survey-section-title">
            <span className="survey-section-icon"><Activity size={17} /></span>
            <h2>Recent Activity</h2>
          </div>
          <ul className="survey-activity-list">
            {auditLogs.map((log) => (
              <li key={log.auditLogId}>
                <strong>{log.userName}</strong> — {log.action.replace(/\./g, ' › ')}
                <span className="survey-activity-time">
                  {new Date(log.timestamp).toLocaleString('en-IN')}
                </span>
              </li>
            ))}
          </ul>
        </section>
      )}

      {editable && (
        <div className="survey-actions">
          <motion.button
            type="button"
            className="survey-action-secondary"
            onClick={() => void handleSaveDraft()}
            disabled={saving || completing}
            whileTap={{ scale: 0.97 }}
          >
            <Save size={15} /> {saving ? 'Saving…' : 'Save as Draft'}
          </motion.button>
          <motion.button
            type="button"
            className="survey-action-secondary"
            onClick={() => setShowPreview(true)}
            disabled={saving || completing}
            whileTap={{ scale: 0.97 }}
          >
            <Eye size={15} /> Preview Assessment
          </motion.button>
          <motion.button
            type="button"
            className="survey-action-primary"
            onClick={() => setShowCompleteConfirm(true)}
            disabled={saving || completing}
            whileTap={{ scale: 0.97 }}
          >
            <Send size={15} /> Complete Assessment
          </motion.button>
        </div>
      )}

      <Modal open={showPreview} onClose={() => setShowPreview(false)} title="Assessment Preview">
        <dl className="survey-fact-grid">
          <dt>Survey Date</dt>
          <dd>{formatDate(form.inspectionDate)}</dd>
          <dt>Location</dt>
          <dd>{form.surveyLocation || '—'}</dd>
          <dt>Vehicle Condition</dt>
          <dd>
            {form.vehicleConditionId ? VehicleConditionName[Number(form.vehicleConditionId)] : '—'}
          </dd>
          <dt>Total Loss</dt>
          <dd>{form.totalLoss ? 'Yes' : 'No'}</dd>
          <dt>Damaged Components</dt>
          <dd>{damageItems.filter((i) => i.componentName.trim() !== '').length}</dd>
          <dt>Gross Assessment Amount</dt>
          <dd>{formatCurrency(previewGross)}</dd>
          <dt>Net Assessment Amount</dt>
          <dd>{formatCurrency(previewNet)}</dd>
          <dt>Overall Recommendation</dt>
          <dd>
            {form.overallRecommendationId
              ? SurveyorRecommendationName[Number(form.overallRecommendationId)]
              : '—'}
          </dd>
        </dl>
        <button type="button" onClick={() => setShowPreview(false)}>
          Close
        </button>
      </Modal>

      <Modal
        open={showCompleteConfirm}
        onClose={() => setShowCompleteConfirm(false)}
        title="Complete this assessment?"
      >
        <p>
          This will save your latest entries, submit the assessment for review, and mark this
          claim's survey as completed. This can't be undone.
        </p>
        <button type="button" onClick={() => void handleComplete()} disabled={completing}>
          {completing ? 'Submitting…' : 'Yes, complete assessment'}
        </button>{' '}
        <button type="button" onClick={() => setShowCompleteConfirm(false)} disabled={completing}>
          Cancel
        </button>
      </Modal>
    </div>
  )
}
