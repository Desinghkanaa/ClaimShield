namespace ClaimShield.Api.Constants
{
    public static class RoleConstants
    {
        // =========================================================
        // ROLE IDS (Masters.Roles.RoleId)
        // =========================================================

        public const int CustomerId = 1;

        public const int RepairerId = 2;

        public const int SurveyorId = 3;

        public const int ApproverId = 4;

        public const int AdminId = 5;

        // =========================================================
        // ROLE NAMES (JWT ClaimTypes.Role / [Authorize(Roles = ...)])
        // =========================================================

        public const string Customer = "Customer";

        public const string Repairer = "Repairer";

        public const string Surveyor = "Surveyor";

        public const string Approver = "Approver";

        public const string Admin = "Admin";
    }
}
