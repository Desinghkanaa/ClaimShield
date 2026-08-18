using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimShield.Api.Migrations
{
    /// <inheritdoc />
    public partial class Phase9_ScoringEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClaimAiInsights",
                schema: "dbo");

            migrationBuilder.CreateTable(
                name: "ClaimScoringResults",
                schema: "dbo",
                columns: table => new
                {
                    ClaimScoringResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false),
                    Stage = table.Column<int>(type: "integer", nullable: false),
                    ScoreValue = table.Column<int>(type: "integer", nullable: false),
                    HardFlagTriggered = table.Column<bool>(type: "boolean", nullable: false),
                    Band = table.Column<int>(type: "integer", nullable: false),
                    TriggeredRuleIds = table.Column<string>(type: "jsonb", nullable: false),
                    ReasonText = table.Column<string>(type: "text", nullable: false),
                    RuleSetVersion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ScoredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SupersededBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimScoringResults", x => x.ClaimScoringResultId);
                    table.ForeignKey(
                        name: "FK_ClaimScoringResults_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalSchema: "dbo",
                        principalTable: "Claims",
                        principalColumn: "ClaimId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScoringRules",
                schema: "Masters",
                columns: table => new
                {
                    RuleId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Stage = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ConditionField = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ConditionOperator = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ConditionThreshold = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoringRules", x => x.RuleId);
                });

            migrationBuilder.CreateTable(
                name: "ScoringThresholds",
                schema: "Masters",
                columns: table => new
                {
                    ThresholdSet = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AmberMin = table.Column<int>(type: "integer", nullable: false),
                    RedMin = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoringThresholds", x => x.ThresholdSet);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClaimScoringResults_ClaimId",
                schema: "dbo",
                table: "ClaimScoringResults",
                column: "ClaimId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClaimScoringResults",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ScoringRules",
                schema: "Masters");

            migrationBuilder.DropTable(
                name: "ScoringThresholds",
                schema: "Masters");

            migrationBuilder.CreateTable(
                name: "ClaimAiInsights",
                schema: "dbo",
                columns: table => new
                {
                    ClaimAiInsightId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimReadinessScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    FraudScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    RiskScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
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

            migrationBuilder.CreateIndex(
                name: "IX_ClaimAiInsights_ClaimId",
                schema: "dbo",
                table: "ClaimAiInsights",
                column: "ClaimId");
        }
    }
}
