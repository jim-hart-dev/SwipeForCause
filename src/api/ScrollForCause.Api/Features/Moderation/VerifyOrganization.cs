using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ScrollForCause.Api.Common;
using ScrollForCause.Api.Database;
using ScrollForCause.Api.Infrastructure.Email;

namespace ScrollForCause.Api.Features.Moderation;

public record VerifyOrganizationRequest(string Status, string? Reason);

public record VerifyOrganizationResponse(
    Guid Id,
    string Name,
    string VerificationStatus,
    DateTime? VerifiedAt,
    DateTime UpdatedAt);

public class VerifyOrganizationValidator : AbstractValidator<VerifyOrganizationRequest>
{
    public VerifyOrganizationValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => s is "verified" or "rejected")
            .WithMessage("Status must be 'verified' or 'rejected'.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .When(x => x.Status == "rejected")
            .WithMessage("Reason is required when rejecting an organization.");
    }
}

public static class VerifyOrganization
{
    public static void MapVerifyOrganization(this WebApplication app)
    {
        app.MapPut("/api/v1/admin/organizations/{id:guid}/verify", async (
            Guid id,
            VerifyOrganizationRequest request,
            IValidator<VerifyOrganizationRequest> validator,
            AppDbContext db,
            IEmailService emailService) =>
        {
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

            var org = await db.Organizations
                .Where(o => o.Id == id && o.IsActive)
                .FirstOrDefaultAsync();

            if (org is null)
            {
                return Results.NotFound(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "ORGANIZATION_NOT_FOUND",
                        Message = "Organization not found.",
                    },
                });
            }

            org.VerificationStatus = request.Status;
            org.UpdatedAt = DateTime.UtcNow;

            if (request.Status == "verified")
            {
                org.VerifiedAt = DateTime.UtcNow;
            }
            else
            {
                org.VerifiedAt = null;
            }

            await db.SaveChangesAsync();

            // Send email after successful DB save to avoid notifying on failed updates
            if (request.Status == "verified")
            {
                await emailService.SendVerificationApprovedAsync(org.ContactEmail, org.Name);
            }
            else
            {
                await emailService.SendVerificationRejectedAsync(org.ContactEmail, org.Name, request.Reason!);
            }

            return Results.Ok(new VerifyOrganizationResponse(
                org.Id,
                org.Name,
                org.VerificationStatus,
                org.VerifiedAt,
                org.UpdatedAt));
        })
        .RequireAuthorization("Admin")
        .WithTags("Admin")
        .WithName("VerifyOrganization");
    }
}
