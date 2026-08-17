using ClaimShield.Api.AI.Interfaces;
using ClaimShield.Api.AI.Services;
using ClaimShield.Api.Authentication;
using ClaimShield.Api.Data.Context;
using ClaimShield.Api.Interfaces.Repositories;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Repositories;
using ClaimShield.Api.Services;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// ============================================================
// DISABLE CONFIGURATION FILE WATCHING
// ============================================================
//
// Render's container has a low inotify/file-watcher limit.
// ASP.NET Core normally watches appsettings.json for changes.
// This is useful during local development but is not required
// in the Render production container.
//

Environment.SetEnvironmentVariable(
    "DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE",
    "false");

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// CONTROLLERS
// ============================================================

builder.Services.AddControllers();

// ============================================================
// DATABASE
// ============================================================

builder.Services.AddDbContext<ClaimShieldDbContext>(
    options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString(
                "SupabaseConnection"),
            npgsqlOptions =>
                npgsqlOptions.EnableRetryOnFailure()
        )
);

// ============================================================
// SUPABASE JWT CONFIGURATION
// ============================================================

// Supabase's Auth server signs tokens asymmetrically (ES256)
// and only exposes a raw JWKS document - no OIDC discovery
// document - so token validation is wired to that JWKS URL
// directly via SupabaseJwksConfigurationRetriever instead of
// the usual Authority auto-discovery.

var supabaseUrl =
    builder.Configuration["Supabase:Url"];

if (string.IsNullOrWhiteSpace(supabaseUrl))
{
    throw new InvalidOperationException(
        "Supabase:Url is not configured.");
}

var supabaseAuthIssuer =
    $"{supabaseUrl}/auth/v1";

var supabaseJwksUri =
    $"{supabaseAuthIssuer}/.well-known/jwks.json";

// ============================================================
// JWT AUTHENTICATION
// ============================================================

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.ConfigurationManager =
            new ConfigurationManager<OpenIdConnectConfiguration>(
                supabaseJwksUri,
                new SupabaseJwksConfigurationRetriever(),
                new HttpDocumentRetriever
                {
                    RequireHttps = true
                });

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidIssuer =
                    supabaseAuthIssuer,

                ValidateAudience = true,

                ValidAudience =
                    "authenticated",

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true
            };
    });

// ============================================================
// AUTHORIZATION
// ============================================================

builder.Services.AddAuthorization();

// ------------------------------------------------------------
// Role claims transformation
// ------------------------------------------------------------

builder.Services.AddTransient<
    IClaimsTransformation,
    SupabaseRoleClaimsTransformation>();

// ============================================================
// HTTP CONTEXT ACCESSOR
// ============================================================

builder.Services.AddHttpContextAccessor();

// ------------------------------------------------------------
// Current User
// ------------------------------------------------------------

builder.Services.AddScoped<
    ICurrentUserService,
    CurrentUserService>();

// ============================================================
// SWAGGER
// ============================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title =
                "ClaimShield API",

            Version =
                "v1",

            Description =
                "ClaimShield Insurance Claim Management API"
        });

    // --------------------------------------------------------
    // JWT SECURITY DEFINITION
    // --------------------------------------------------------

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name =
                "Authorization",

            Type =
                SecuritySchemeType.Http,

            Scheme =
                "bearer",

            BearerFormat =
                "JWT",

            In =
                ParameterLocation.Header,

            Description =
                "Enter your JWT token. Example: Bearer {token}"
        });

    // --------------------------------------------------------
    // JWT SECURITY REQUIREMENT
    // --------------------------------------------------------

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType.SecurityScheme,

                            Id =
                                "Bearer"
                        }
                },

                Array.Empty<string>()
            }
        });
});

// ============================================================
// REPOSITORIES
// ============================================================

builder.Services.AddScoped<
    IUserRepository,
    UserRepository>();

builder.Services.AddScoped<
    ICustomerRepository,
    CustomerRepository>();

builder.Services.AddScoped<
    IClaimRepository,
    ClaimRepository>();

builder.Services.AddScoped<
    IClaimDocumentRepository,
    ClaimDocumentRepository>();

builder.Services.AddScoped<
    IPaymentRepository,
    PaymentRepository>();

builder.Services.AddScoped<
    ISurveyAssignmentRepository,
    SurveyAssignmentRepository>();

builder.Services.AddScoped<
    IRepairAssignmentRepository,
    RepairAssignmentRepository>();

builder.Services.AddScoped<
    IRepairEstimateRepository,
    RepairEstimateRepository>();

builder.Services.AddScoped<
    ISurveyReportRepository,
    SurveyReportRepository>();

builder.Services.AddScoped<
    IPolicyRepository,
    PolicyRepository>();

builder.Services.AddScoped<
    IVehicleRepository,
    VehicleRepository>();

builder.Services.AddScoped<
    IRoleRepository,
    RoleRepository>();

// ============================================================
// SERVICES
// ============================================================

builder.Services.AddScoped<
    ICustomerService,
    CustomerService>();

builder.Services.AddScoped<
    IClaimService,
    ClaimService>();

builder.Services.AddScoped<
    IClaimDocumentService,
    ClaimDocumentService>();

builder.Services.AddScoped<
    IPaymentService,
    PaymentService>();

builder.Services.AddScoped<
    ISurveyAssignmentService,
    SurveyAssignmentService>();

builder.Services.AddScoped<
    IRepairAssignmentService,
    RepairAssignmentService>();

builder.Services.AddScoped<
    IRepairEstimateService,
    RepairEstimateService>();

builder.Services.AddScoped<
    ISurveyReportService,
    SurveyReportService>();

builder.Services.AddScoped<
    IClaimApprovalService,
    ClaimApprovalService>();

builder.Services.AddScoped<
    IClaimScoringService,
    ClaimScoringService>();

builder.Services.AddScoped<
    IScoringRuleService,
    ScoringRuleService>();

builder.Services.AddScoped<
    IScoringThresholdService,
    ScoringThresholdService>();

builder.Services.AddScoped<
    IDashboardService,
    DashboardService>();

// ============================================================
// OTP
// ============================================================

builder.Services.AddScoped<
    IOtpService,
    OtpService>();

// ============================================================
// OCR
// ============================================================

builder.Services.AddScoped<
    IOcrService,
    TesseractOcrService>();

// ============================================================
// INSTANT CLAIM
// ============================================================

builder.Services.AddScoped<
    IEstimateEngineService,
    EstimateEngineService>();

builder.Services.AddScoped<
    IInstantClaimConfigService,
    InstantClaimConfigService>();

builder.Services.AddScoped<
    IClaimRaiseService,
    ClaimRaiseService>();

// ============================================================
// AUDIT LOG
// ============================================================

builder.Services.AddScoped<
    IAuditLogService,
    AuditLogService>();

// ============================================================
// CLAIM DECISIONS
// ============================================================

builder.Services.AddScoped<
    IClaimDecisionService,
    ClaimDecisionService>();

// ============================================================
// REASSESSMENT COMMENTS
// ============================================================

builder.Services.AddScoped<
    IReassessmentCommentService,
    ReassessmentCommentService>();

// ============================================================
// AUTHORITY LIMITS
// ============================================================

builder.Services.AddScoped<
    IAuthorityLimitService,
    AuthorityLimitService>();

// ============================================================
// USERS
// ============================================================

builder.Services.AddScoped<
    IUserService,
    UserService>();

// ============================================================
// SUPABASE ADMIN API
// ============================================================

builder.Services.AddHttpClient<
    ISupabaseAdminService,
    SupabaseAdminService>();

// ============================================================
// SUPABASE STORAGE API
// ============================================================

builder.Services.AddHttpClient<
    ISupabaseStorageService,
    SupabaseStorageService>();

// ============================================================
// ROLES
// ============================================================

builder.Services.AddScoped<
    IRoleService,
    RoleService>();

// ============================================================
// VEHICLES
// ============================================================

builder.Services.AddScoped<
    IVehicleService,
    VehicleService>();

// ============================================================
// POLICIES
// ============================================================

builder.Services.AddScoped<
    IPolicyService,
    PolicyService>();

// ============================================================
// CLAIM CLOSURE
// ============================================================

builder.Services.AddScoped<
    IClaimClosureService,
    ClaimClosureService>();

// ============================================================
// AI SERVICE
// ============================================================

// Development AI uses ClaimShield's existing services.
// No OpenAI API key is required.

builder.Services.AddScoped<
    IAiService,
    MockAiService>();

// ============================================================
// CORS
// ============================================================

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            "ClaimShieldPolicy",
            policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
    });

// ============================================================
// BUILD APPLICATION
// ============================================================

var app =
    builder.Build();

// ============================================================
// DEFAULT FILES
// ============================================================

app.UseDefaultFiles();

// ============================================================
// STATIC FILES
// ============================================================

app.UseStaticFiles();

// ============================================================
// SWAGGER
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger();

    app.UseSwaggerUI(
        options =>
        {
            options.DocumentTitle =
                "ClaimShield API";

            options.SwaggerEndpoint(
                "/swagger/v1/swagger.json",
                "ClaimShield API v1");
        });
}

// ============================================================
// HTTPS
// ============================================================

app.UseHttpsRedirection();

// ============================================================
// CORS
// ============================================================

app.UseCors(
    "ClaimShieldPolicy");

// ============================================================
// AUTHENTICATION
// ============================================================

app.UseAuthentication();

// ============================================================
// AUTHORIZATION
// ============================================================

app.UseAuthorization();

// ============================================================
// CONTROLLERS
// ============================================================

app.MapControllers();

// ============================================================
// RUN
// ============================================================

app.Run();
