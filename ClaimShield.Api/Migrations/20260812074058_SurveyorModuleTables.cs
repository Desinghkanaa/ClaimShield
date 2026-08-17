using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimShield.Api.Migrations
{
    /// <inheritdoc />
    public partial class SurveyorModuleTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InspectionMode",
                schema: "dbo",
                table: "SurveyAssignments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ReserveAmount",
                schema: "dbo",
                table: "Claims",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                schema: "dbo",
                columns: table => new
                {
                    AuditLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    BeforeState = table.Column<string>(type: "jsonb", nullable: true),
                    AfterState = table.Column<string>(type: "jsonb", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditLogId);
                    table.ForeignKey(
                        name: "FK_AuditLogs_profiles_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuthorityLimits",
                schema: "Masters",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    MaxApprovalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MaxRiskScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorityLimits", x => x.RoleId);
                    table.ForeignKey(
                        name: "FK_AuthorityLimits_profiles_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalSchema: "public",
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClaimAiInsights",
                schema: "dbo",
                columns: table => new
                {
                    ClaimAiInsightId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimReadinessScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    RiskScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    FraudScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    ScoreReasoning = table.Column<string>(type: "text", nullable: true),
                    ScoredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimAiInsights", x => x.ClaimAiInsightId);
                    table.ForeignKey(
                        name: "FK_ClaimAiInsights_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalSchema: "dbo",
                        principalTable: "Claims",
                        principalColumn: "ClaimId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClaimDecisions",
                schema: "dbo",
                columns: table => new
                {
                    ClaimDecisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false),
                    DecidedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    Decision = table.Column<int>(type: "integer", nullable: false),
                    Reasoning = table.Column<string>(type: "text", nullable: false),
                    AiScoresSnapshot = table.Column<string>(type: "jsonb", nullable: true),
                    DecisionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimDecisions", x => x.ClaimDecisionId);
                    table.ForeignKey(
                        name: "FK_ClaimDecisions_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalSchema: "dbo",
                        principalTable: "Claims",
                        principalColumn: "ClaimId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClaimDecisions_profiles_DecidedBy",
                        column: x => x.DecidedBy,
                        principalSchema: "public",
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReassessmentComments",
                schema: "dbo",
                columns: table => new
                {
                    ReassessmentCommentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReassessmentComments", x => x.ReassessmentCommentId);
                    table.ForeignKey(
                        name: "FK_ReassessmentComments_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalSchema: "dbo",
                        principalTable: "Claims",
                        principalColumn: "ClaimId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReassessmentComments_profiles_AuthorId",
                        column: x => x.AuthorId,
                        principalSchema: "public",
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                schema: "dbo",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorityLimits_UpdatedBy",
                schema: "Masters",
                table: "AuthorityLimits",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimAiInsights_ClaimId",
                schema: "dbo",
                table: "ClaimAiInsights",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimDecisions_ClaimId",
                schema: "dbo",
                table: "ClaimDecisions",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimDecisions_DecidedBy",
                schema: "dbo",
                table: "ClaimDecisions",
                column: "DecidedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReassessmentComments_AuthorId",
                schema: "dbo",
                table: "ReassessmentComments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_ReassessmentComments_ClaimId",
                schema: "dbo",
                table: "ReassessmentComments",
                column: "ClaimId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AuthorityLimits",
                schema: "Masters");

            migrationBuilder.DropTable(
                name: "ClaimAiInsights",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ClaimDecisions",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ReassessmentComments",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "InspectionMode",
                schema: "dbo",
                table: "SurveyAssignments");

            migrationBuilder.DropColumn(
                name: "ReserveAmount",
                schema: "dbo",
                table: "Claims");
        }
    }
}
