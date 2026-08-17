using ClaimShield.Api.Constants;
using ClaimShield.Api.Interfaces.Repositories;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.SurveyAssignments;
using ClaimShield.Api.Models.Entities;

namespace ClaimShield.Api.Services
{
    public class SurveyAssignmentService : ISurveyAssignmentService
    {
        private readonly ISurveyAssignmentRepository _surveyAssignmentRepository;
        private readonly IClaimRepository _claimRepository;

        public SurveyAssignmentService(
            ISurveyAssignmentRepository surveyAssignmentRepository,
            IClaimRepository claimRepository)
        {
            _surveyAssignmentRepository = surveyAssignmentRepository;
            _claimRepository = claimRepository;
        }

        public async Task<IEnumerable<SurveyAssignmentResponseDto>> GetAllAsync()
        {
            var assignments =
                await _surveyAssignmentRepository.GetAllAsync();

            return assignments.Select(MapToDto);
        }

        public async Task<SurveyAssignmentResponseDto?> GetByIdAsync(
            Guid surveyAssignmentId)
        {
            var assignment =
                await _surveyAssignmentRepository.GetByIdAsync(
                    surveyAssignmentId);

            if (assignment == null)
                return null;

            return MapToDto(assignment);
        }

        public async Task<IEnumerable<SurveyAssignmentResponseDto>> GetByClaimAsync(
            Guid claimId)
        {
            var assignments =
                await _surveyAssignmentRepository.GetByClaimAsync(claimId);

            return assignments.Select(MapToDto);
        }

        public async Task<IEnumerable<SurveyAssignmentResponseDto>> GetBySurveyorAsync(
            Guid surveyorId)
        {
            var assignments =
                await _surveyAssignmentRepository.GetBySurveyorAsync(surveyorId);

            return assignments.Select(MapToDto);
        }

        public async Task<SurveyAssignmentResponseDto> CreateAsync(
            CreateSurveyAssignmentRequest request)
        {
            var surveyAssignment = new SurveyAssignment
            {
                SurveyAssignmentId = Guid.NewGuid(),

                ClaimId = request.ClaimId,

                SurveyorId = request.SurveyorId,

                AssignedBy = request.AssignedBy,

                AssignedDate = request.AssignedDate ?? DateTime.UtcNow,

                DueDate = request.DueDate,

                AssignmentStatusId = request.AssignmentStatusId,

                InspectionMode = request.InspectionMode,

                Remarks = request.Remarks,

                CreatedDate = DateTime.UtcNow
            };

            await _surveyAssignmentRepository.AddAsync(
                surveyAssignment);

            // Phase 13 fix: this was the confirmed gap where
            // ClaimStatusConstants.SurveyAssigned was never actually set
            // anywhere in the backend, leaving the whole downstream
            // Surveyor/Approver decision pipeline unreachable. Forward-only
            // guard so this never regresses a claim that's already moved
            // further along.
            var claim = await _claimRepository.GetByIdAsync(request.ClaimId);

            if (claim != null &&
                (claim.StatusId == null ||
                 claim.StatusId.Value < ClaimStatusConstants.SurveyAssigned))
            {
                claim.StatusId = ClaimStatusConstants.SurveyAssigned;
                claim.UpdatedDate = DateTime.UtcNow;

                await _claimRepository.UpdateAsync(claim);
            }

            return MapToDto(surveyAssignment);
        }

        public async Task<bool> UpdateAsync(
            UpdateSurveyAssignmentRequest request)
        {
            var surveyAssignment =
                await _surveyAssignmentRepository.GetByIdAsync(
                    request.SurveyAssignmentId);

            if (surveyAssignment == null)
                return false;

            surveyAssignment.ClaimId = request.ClaimId;

            surveyAssignment.SurveyorId = request.SurveyorId;

            surveyAssignment.AssignedBy = request.AssignedBy;

            surveyAssignment.AssignedDate = request.AssignedDate;

            surveyAssignment.DueDate = request.DueDate;

            surveyAssignment.AssignmentStatusId =
                request.AssignmentStatusId;

            surveyAssignment.InspectionMode = request.InspectionMode;

            surveyAssignment.Remarks = request.Remarks;

            surveyAssignment.UpdatedDate = DateTime.UtcNow;

            await _surveyAssignmentRepository.UpdateAsync(
                surveyAssignment);

            return true;
        }

        public async Task<bool> DeleteAsync(Guid surveyAssignmentId)
        {
            var surveyAssignment =
                await _surveyAssignmentRepository.GetByIdAsync(
                    surveyAssignmentId);

            if (surveyAssignment == null)
                return false;

            await _surveyAssignmentRepository.DeleteAsync(
                surveyAssignmentId);

            return true;
        }

        private static SurveyAssignmentResponseDto MapToDto(
            SurveyAssignment surveyAssignment)
        {
            return new SurveyAssignmentResponseDto
            {
                SurveyAssignmentId =
                    surveyAssignment.SurveyAssignmentId,

                ClaimId =
                    surveyAssignment.ClaimId,

                SurveyorId =
                    surveyAssignment.SurveyorId,

                AssignedBy =
                    surveyAssignment.AssignedBy,

                AssignedDate =
                    surveyAssignment.AssignedDate,

                DueDate =
                    surveyAssignment.DueDate,

                AssignmentStatusId =
                    surveyAssignment.AssignmentStatusId,

                InspectionMode =
                    surveyAssignment.InspectionMode,

                Remarks =
                    surveyAssignment.Remarks,

                CreatedDate =
                    surveyAssignment.CreatedDate,

                UpdatedDate =
                    surveyAssignment.UpdatedDate
            };
        }
    }
}