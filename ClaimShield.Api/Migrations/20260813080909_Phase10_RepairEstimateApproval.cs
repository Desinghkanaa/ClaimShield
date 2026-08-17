using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimShield.Api.Migrations
{
    /// <inheritdoc />
    public partial class Phase10_RepairEstimateApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalRemarks",
                schema: "dbo",
                table: "RepairEstimates",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatusId",
                schema: "dbo",
                table: "RepairEstimates",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalRemarks",
                schema: "dbo",
                table: "RepairEstimates");

            migrationBuilder.DropColumn(
                name: "ApprovalStatusId",
                schema: "dbo",
                table: "RepairEstimates");
        }
    }
}
