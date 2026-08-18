using ClaimShield.Api.Constants;
using ClaimShield.Api.Data.Context;
using ClaimShield.Api.Interfaces.Repositories;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.RepairAssignments;
using ClaimShield.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaimShield.Api.Services
{
    public class RepairAssignmentService : IRepairAssignmentService
    {
        private readonly IRepairAssignmentRepository _repairAssignmentRepository;
        private readonly ClaimShieldDbContext _context;

        public RepairAssignmentService(
            IRepairAssignmentRepository repairAssignmentRepository,
            ClaimShieldDbContext context)
        {
            _repairAssignmentRepository = repairAssignmentRepository;
            _context = context;
        }

        // =========================================================
        // GET ALL
        // =========================================================

        public async Task<IEnumerable<RepairAssignmentResponseDto>> GetAllAsync()
        {
            var assignments =
                await _repairAssignmentRepository.GetAllAsync();

            return assignments.Select(MapToDto);
        }

        // =========================================================
        // GET BY ID
        // =========================================================

        public async Task<RepairAssignmentResponseDto?> GetByIdAsync(
            Guid repairAssignmentId)
        {
            var assignment =
                await _repairAssignmentRepository.GetByIdAsync(
                    repairAssignmentId);

            if (assignment == null)
            {
                return null;
            }

            return MapToDto(assignment);
        }

        // =========================================================
        // GET BY CLAIM
        // =========================================================

        public async Task<IEnumerable<RepairAssignmentResponseDto>> GetByClaimAsync(
            Guid claimId)
        {
            var assignments =
                await _repairAssignmentRepository.GetByClaimAsync(
                    claimId);

            return assignments.Select(MapToDto);
        }

        // =========================================================
        // GET BY REPAIRER
        // =========================================================

        public async Task<IEnumerable<RepairAssignmentResponseDto>> GetByRepairerAsync(
            Guid repairerId)
        {
            var assignments =
                await _repairAssignmentRepository.GetByRepairerAsync(
                    repairerId);

            return assignments.Select(MapToDto);
        }

        // =========================================================
        // CREATE
        // =========================================================

        public async Task<RepairAssignmentResponseDto> CreateAsync(
            CreateRepairAssignmentRequest request)
        {
            var repairAssignment = new RepairAssignment
            {
                RepairAssignmentId = Guid.NewGuid(),

                ClaimId = request.ClaimId,

                RepairerId = request.RepairerId,

                AssignedBy = request.AssignedBy,

                AssignedDate =
                    request.AssignedDate ?? DateTime.UtcNow,

                ExpectedCompletionDate =
                    request.ExpectedCompletionDate,

                AssignmentStatusId =
                    request.AssignmentStatusId,

                Remarks =
                    request.Remarks,

                CreatedDate =
                    DateTime.UtcNow,

                UpdatedDate = null
            };

            await _repairAssignmentRepository.AddAsync(
                repairAssignment);

            return MapToDto(repairAssignment);
        }

        // =========================================================
        // UPDATE
        // =========================================================

        public async Task<bool> UpdateAsync(
            UpdateRepairAssignmentRequest request)
        {
            var repairAssignment =
                await _repairAssignmentRepository.GetByIdAsync(
                    request.RepairAssignmentId);

            if (repairAssignment == null)
            {
                return false;
            }

            repairAssignment.ClaimId =
                request.ClaimId;

            repairAssignment.RepairerId =
                request.RepairerId;

            repairAssignment.AssignedBy =
                request.AssignedBy;

            repairAssignment.AssignedDate =
                request.AssignedDate;

            repairAssignment.ExpectedCompletionDate =
                request.ExpectedCompletionDate;

            repairAssignment.AssignmentStatusId =
                request.AssignmentStatusId;

            repairAssignment.Remarks =
                request.Remarks;

            repairAssignment.UpdatedDate =
                DateTime.UtcNow;

            await _repairAssignmentRepository.UpdateAsync(
                repairAssignment);

            // -----------------------------------------------------
            // The claim itself has no other trigger that advances it
            // past RepairAssigned - a Repairer marking their
            // assignment In Progress is what moves the claim into
            // RepairInProgress, the status repair estimate approval
            // and the Approver's claim-detail view both key off.
            // -----------------------------------------------------

            if (request.AssignmentStatusId ==
                AssignmentStatusConstants.InProgress)
            {
                var claim =
                    await _context.Claims
                        .FirstOrDefaultAsync(
                            x => x.ClaimId == repairAssignment.ClaimId);

                if (claim != null &&
                    claim.StatusId == ClaimStatusConstants.RepairAssigned)
                {
                    claim.StatusId =
                        ClaimStatusConstants.RepairInProgress;

                    claim.UpdatedDate =
                        DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                }
            }

            return true;
        }

        // =========================================================
        // DELETE
        // =========================================================

        public async Task<bool> DeleteAsync(
            Guid repairAssignmentId)
        {
            var repairAssignment =
                await _repairAssignmentRepository.GetByIdAsync(
                    repairAssignmentId);

            if (repairAssignment == null)
            {
                return false;
            }

            await _repairAssignmentRepository.DeleteAsync(
                repairAssignmentId);

            return true;
        }

        // =========================================================
        // MAPPING
        // =========================================================

        private static RepairAssignmentResponseDto MapToDto(
            RepairAssignment repairAssignment)
        {
            return new RepairAssignmentResponseDto
            {
                RepairAssignmentId =
                    repairAssignment.RepairAssignmentId,

                ClaimId =
                    repairAssignment.ClaimId,

                RepairerId =
                    repairAssignment.RepairerId,

                AssignedBy =
                    repairAssignment.AssignedBy,

                AssignedDate =
                    repairAssignment.AssignedDate,

                ExpectedCompletionDate =
                    repairAssignment.ExpectedCompletionDate,

                AssignmentStatusId =
                    repairAssignment.AssignmentStatusId,

                Remarks =
                    repairAssignment.Remarks,

                CreatedDate =
                    repairAssignment.CreatedDate,

                UpdatedDate =
                    repairAssignment.UpdatedDate
            };
        }
    }
}