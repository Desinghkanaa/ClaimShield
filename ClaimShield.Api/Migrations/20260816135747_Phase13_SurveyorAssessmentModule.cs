using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimShield.Api.Migrations
{
    /// <inheritdoc />
    public partial class Phase13_SurveyorAssessmentModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssessmentRemarks",
                schema: "dbo",
                table: "SurveyReports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssessmentStatusId",
                schema: "dbo",
                table: "SurveyReports",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "CashSettlementRecommended",
                schema: "dbo",
                table: "SurveyReports",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CompulsoryExcess",
                schema: "dbo",
                table: "SurveyReports",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DepreciationAmount",
                schema: "dbo",
                table: "SurveyReports",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedDurationDays",
                schema: "dbo",
                table: "SurveyReports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstimatedRepairerName",
                schema: "dbo",
                table: "SurveyReports",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossAssessmentAmount",
                schema: "dbo",
                table: "SurveyReports",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LabourCost",
                schema: "dbo",
                table: "SurveyReports",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NetAssessmentAmount",
                schema: "dbo",
                table: "SurveyReports",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OverallRecommendationId",
                schema: "dbo",
                table: "SurveyReports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PaintCost",
                schema: "dbo",
                table: "SurveyReports",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PartsCost",
                schema: "dbo",
                table: "SurveyReports",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreExistingDamageNotes",
                schema: "dbo",
                table: "SurveyReports",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RepairRecommended",
                schema: "dbo",
                table: "SurveyReports",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RepairabilityStatusId",
                schema: "dbo",
                table: "SurveyReports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReplaceRecommended",
                schema: "dbo",
                table: "SurveyReports",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalvageAmount",
                schema: "dbo",
                table: "SurveyReports",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SurveyLocation",
                schema: "dbo",
                table: "SurveyReports",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                schema: "dbo",
                table: "SurveyReports",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TotalLossRecommended",
                schema: "dbo",
                table: "SurveyReports",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TowingCharges",
                schema: "dbo",
                table: "SurveyReports",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VehicleConditionId",
                schema: "dbo",
                table: "SurveyReports",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DamageAssessmentItems",
                schema: "dbo",
                columns: table => new
                {
                    DamageAssessmentItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    SurveyReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComponentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DamageCategoryId = table.Column<int>(type: "integer", nullable: true),
                    SeverityId = table.Column<int>(type: "integer", nullable: true),
                    RepairRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReplacementRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DamageAssessmentItems", x => x.DamageAssessmentItemId);
                    table.ForeignKey(
                        name: "FK_DamageAssessmentItems_SurveyReports_SurveyReportId",
                        column: x => x.SurveyReportId,
                        principalSchema: "dbo",
                        principalTable: "SurveyReports",
                        principalColumn: "SurveyReportId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DamageAssessmentItems_SurveyReportId",
                schema: "dbo",
                table: "DamageAssessmentItems",
                column: "SurveyReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DamageAssessmentItems",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "AssessmentRemarks",
                schema: "dbo",
                table: "SurveyReports");

            migrationBuilder.DropColumn(
                name: "AssessmentStatusId",
                schema: "dbo",
                table: "SurveyReports");

            migrationBuilder.DropColumn(
                name: "CashSettlementRecommended",
                schema: "dbo",
                table: "SurveyReports");

            migrationBuilder.DropColumn(
                name: "CompulsoryExcess",
                schema: "dbo",
                table: "SurveyReports");

            migrationBuilder.DropColumn(
                name: "DepreciationAmount",
                schema: "dbo",
                table: "SurveyReports");

            migrationBuilder.DropColumn(
                name: "EstimatedDurationDays",
                schema: "dbo",
                table: "SurveyReports");

            migrationBuilder.DropColumn(
                name: "EstimatedRepairerName",
                schema: "dbo",
                table: "SurveyReports");

            migrationBuilder.DropColumn(
                name: "GrossAssessmentAmount",
                schema: "dbo",
                table: "SurveyReports");

            migrationBuilder.DropColumn(
                name: "LabourCost",
                schema: "dbo",
                table: "SurveyReports");

            migrationBuilder.DropColumn(
                name: "NetAssessmentAmount",
                schema: "dbo",
                table: "SurveyReports");

            migrationBuilder.DropColumn(
                name: "OverallRecommendationId",
                schema: "dbo",
                table: "SurveyReports");

            migrationBuilder.DropColumn(
                name: "PaintCost",
                schema: "dbo",
                table: "SurveyReports");

            migrationBuilder.DropColumn(
                name: "PartsCost",
                schema: "dbo",
                table: "SurveyReports");

            migrationBuilder.DropColumn(
                name: "PreExistingDamageNotes",
                schema: "dbo",
                table: "SurveyReports");

            migrationBuilder.DropColumn(
                name: "RepairRecommended",
                schema: "dbo",
                table: "SurveyReports");

            migrationBuilder.DropColumn(
                name: "RepairabilityStatusId",
                schema: "dbo",
                table: "SurveyReports");

            migrationBuilder.DropColumn(
                name: "ReplaceRecommended",
                schema: "dbo",
                table: "SurveyReports");

            migrationBuilder.DropColumn(
                name: "SalvageAmount",
                schema: "dbo",
                table: "SurveyReports");

            migrationBuilder.DropColumn(
                name: "SurveyLocation",
                schema: "dbo",
                table: "SurveyReports");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                schema: "dbo",
                table: "SurveyReports");

            migrationBuilder.DropColumn(
                name: "TotalLossRecommended",
                schema: "dbo",
                table: "SurveyReports");

            migrationBuilder.DropColumn(
                name: "TowingCharges",
                schema: "dbo",
                table: "SurveyReports");

            migrationBuilder.DropColumn(
                name: "VehicleConditionId",
                schema: "dbo",
                table: "SurveyReports");
        }
    }
}
