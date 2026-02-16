using System.Security.Claims;
using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ScrollForCause.Api.Common;
using ScrollForCause.Api.Database;
using ScrollForCause.Api.Database.Entities;
using ScrollForCause.Api.Infrastructure.Auth;

namespace ScrollForCause.Api.Features.Opportunities;

public record CreateOpportunityRequest(
    string Title,
    string Description,
    string? LocationAddress,
    bool IsRemote,
    decimal? Latitude,
    decimal? Longitude,
    string ScheduleType,
    DateTime? StartDate,
    DateTime? EndDate,
    string? RecurrenceDesc,
    int? VolunteersNeeded,
    string? TimeCommitment,
    string? SkillsRequired,
    int? MinimumAge);

public record CreateOpportunityResponse(Guid OpportunityId, string Status);

public class CreateOpportunityValidator : AbstractValidator<CreateOpportunityRequest>
{
    private static readonly string[] ValidScheduleTypes = ["one_time", "recurring", "flexible"];

    public CreateOpportunityValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.ScheduleType).NotEmpty()
            .Must(s => ValidScheduleTypes.Contains(s))
            .WithMessage("ScheduleType must be one of: one_time, recurring, flexible.");
        RuleFor(x => x.StartDate).NotEmpty()
            .When(x => x.ScheduleType == "one_time")
            .WithMessage("StartDate is required for one_time schedule type.");
        RuleFor(x => x.LocationAddress).MaximumLength(500);
        RuleFor(x => x.RecurrenceDesc).MaximumLength(500);
        RuleFor(x => x.TimeCommitment).MaximumLength(100);
        RuleFor(x => x.SkillsRequired).MaximumLength(500);
        RuleFor(x => x.MinimumAge)
            .InclusiveBetween(1, 120).When(x => x.MinimumAge.HasValue);
        RuleFor(x => x.VolunteersNeeded)
            .GreaterThan(0).When(x => x.VolunteersNeeded.HasValue);
    }
}

public static partial class CreateOpportunity
{
    [GeneratedRegex("<[^>]*>")]
    private static partial Regex HtmlTagRegex();

    public static void MapCreateOpportunity(this WebApplication app)
    {
        app.MapPost("/api/v1/opportunities", async (
            CreateOpportunityRequest request,
            IValidator<CreateOpportunityRequest> validator,
            ClaimsPrincipal user,
            AppDbContext db) =>
        {
            // Validate request
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "VALIDATION_ERROR",
                        Message = "Validation failed.",
                        Details = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }),
                    },
                });
            }

            // Get current user
            var currentUser = user.ToCurrentUser();

            // Find organization by ClerkUserId
            var organization = await db.Organizations
                .FirstOrDefaultAsync(o => o.ClerkUserId == currentUser.ClerkUserId && o.IsActive);

            if (organization is null)
            {
                return Results.NotFound(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "ORG_NOT_FOUND",
                        Message = "Organization not found.",
                    },
                });
            }

            // Enforce verified status
            if (organization.VerificationStatus != "verified")
            {
                return Results.Json(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "ORG_NOT_VERIFIED",
                        Message = "Only verified organizations can create opportunities.",
                    },
                }, statusCode: 403);
            }

            // Sanitize text inputs
            static string Sanitize(string input) =>
                HtmlTagRegex().Replace(input, string.Empty).Trim();

            // Create opportunity
            var opportunity = new Opportunity
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                Title = Sanitize(request.Title),
                Description = Sanitize(request.Description),
                LocationAddress = request.LocationAddress is not null ? Sanitize(request.LocationAddress) : null,
                IsRemote = request.IsRemote,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                ScheduleType = request.ScheduleType,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                RecurrenceDesc = request.RecurrenceDesc is not null ? Sanitize(request.RecurrenceDesc) : null,
                VolunteersNeeded = request.VolunteersNeeded,
                TimeCommitment = request.TimeCommitment is not null ? Sanitize(request.TimeCommitment) : null,
                SkillsRequired = request.SkillsRequired is not null ? Sanitize(request.SkillsRequired) : null,
                MinimumAge = request.MinimumAge,
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            db.Opportunities.Add(opportunity);
            await db.SaveChangesAsync();

            var response = new CreateOpportunityResponse(opportunity.Id, opportunity.Status);
            return Results.Created($"/api/v1/opportunities/{opportunity.Id}", response);
        })
        .RequireAuthorization("Organization")
        .WithTags("Opportunities")
        .WithName("CreateOpportunity");
    }
}
