using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimShield.Api.Migrations
{
    /// <inheritdoc />
    public partial class Phase12_CustomerRaiseClaimJourney : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RCStatus",
                schema: "dbo",
                table: "Vehicles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddOns",
                schema: "dbo",
                table: "Policies",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Excess",
                schema: "dbo",
                table: "Policies",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "IDV",
                schema: "dbo",
                table: "Policies",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClaimEstimateResults",
                schema: "dbo",
                columns: table => new
                {
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineItems = table.Column<string>(type: "jsonb", nullable: false),
                    NetAssessmentAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    RuleSetVersion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CustomerDecision = table.Column<int>(type: "integer", nullable: true),
                    DecisionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OtpVerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimEstimateResults", x => x.ClaimId);
                    table.ForeignKey(
                        name: "FK_ClaimEstimateResults_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalSchema: "dbo",
                        principalTable: "Claims",
                        principalColumn: "ClaimId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClaimIntakes",
                schema: "dbo",
                columns: table => new
                {
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleLocationAtLoss = table.Column<int>(type: "integer", nullable: false),
                    LossType = table.Column<int>(type: "integer", nullable: false),
                    InstantClaimToggle = table.Column<bool>(type: "boolean", nullable: false),
                    InstantClaimParts = table.Column<string>(type: "jsonb", nullable: false),
                    CustomerEstimatedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    VehicleParkedSafely = table.Column<bool>(type: "boolean", nullable: true),
                    DeathOccurred = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimIntakes", x => x.ClaimId);
                    table.ForeignKey(
                        name: "FK_ClaimIntakes_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalSchema: "dbo",
                        principalTable: "Claims",
                        principalColumn: "ClaimId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClaimRcOcrResults",
                schema: "dbo",
                columns: table => new
                {
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtractedRegNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ExtractedOwnerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ExtractedChassisNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PlatePhotoExtractedRegNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PolicyRegNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    MatchStatus = table.Column<int>(type: "integer", nullable: false),
                    RawOcrConfidence = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimRcOcrResults", x => x.ClaimId);
                    table.ForeignKey(
                        name: "FK_ClaimRcOcrResults_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalSchema: "dbo",
                        principalTable: "Claims",
                        principalColumn: "ClaimId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InstantClaimEligibility",
                schema: "Masters",
                columns: table => new
                {
                    EligibilitySet = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MinEligibleBand = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstantClaimEligibility", x => x.EligibilitySet);
                });

            migrationBuilder.CreateTable(
                name: "InstantClaimPartsPricing",
                schema: "Masters",
                columns: table => new
                {
                    PartsPricingId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PartType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MakeId = table.Column<int>(type: "integer", nullable: true),
                    ModelId = table.Column<int>(type: "integer", nullable: true),
                    PartsAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstantClaimPartsPricing", x => x.PartsPricingId);
                });

            migrationBuilder.CreateTable(
                name: "InstantClaimRateCards",
                schema: "Masters",
                columns: table => new
                {
                    RateCardId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PartType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RemoveRefitCharge = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DentingCharge = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaintingCharge = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SalvagePercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstantClaimRateCards", x => x.RateCardId);
                });

            migrationBuilder.CreateTable(
                name: "OtpVerifications",
                schema: "dbo",
                columns: table => new
                {
                    OtpVerificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Purpose = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodeHash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConsumedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpVerifications", x => x.OtpVerificationId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClaimEstimateResults",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ClaimIntakes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ClaimRcOcrResults",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "InstantClaimEligibility",
                schema: "Masters");

            migrationBuilder.DropTable(
                name: "InstantClaimPartsPricing",
                schema: "Masters");

            migrationBuilder.DropTable(
                name: "InstantClaimRateCards",
                schema: "Masters");

            migrationBuilder.DropTable(
                name: "OtpVerifications",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "RCStatus",
                schema: "dbo",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "AddOns",
                schema: "dbo",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "Excess",
                schema: "dbo",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "IDV",
                schema: "dbo",
                table: "Policies");
        }
    }
}
