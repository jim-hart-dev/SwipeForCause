using Microsoft.EntityFrameworkCore;
using SwipeForCause.Api.Common;
using SwipeForCause.Api.Database;

namespace SwipeForCause.Api.Features.Moderation;

public record AdminOrganizationDetailResponse(
    Guid Id,
    string Name,
    string Ein,
    string Description,
    string ContactName,
    string ContactEmail,
    string? WebsiteUrl,
    string? City,
    string? State,
    string VerificationStatus,
    DateTime? VerifiedAt,
    DateTime CreatedAt,
    string? LogoUrl,
    string? CoverImageUrl,
    List<AdminCategoryResponse> Categories);

public record AdminCategoryResponse(Guid Id, string Name, string Slug);

public static class GetOrganizationDetail
{
    public static void MapGetOrganizationDetail(this WebApplication app)
    {
        app.MapGet("/api/v1/admin/organizations/{id:guid}", async (
            Guid id,
            AppDbContext db) =>
        {
            var org = await db.Organizations
                .Include(o => o.OrganizationCategories)
                    .ThenInclude(oc => oc.Category)
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

            var categories = org.OrganizationCategories
                .Select(oc => new AdminCategoryResponse(oc.Category.Id, oc.Category.Name, oc.Category.Slug))
                .ToList();

            return Results.Ok(new AdminOrganizationDetailResponse(
                org.Id,
                org.Name,
                org.Ein,
                org.Description,
                org.ContactName,
                org.ContactEmail,
                org.WebsiteUrl,
                org.City,
                org.State,
                org.VerificationStatus,
                org.VerifiedAt,
                org.CreatedAt,
                org.LogoUrl,
                org.CoverImageUrl,
                categories));
        })
        .RequireAuthorization("Admin")
        .WithTags("Admin")
        .WithName("GetOrganizationDetail");
    }
}
