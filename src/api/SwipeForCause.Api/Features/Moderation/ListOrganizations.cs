using Microsoft.EntityFrameworkCore;
using SwipeForCause.Api.Common;
using SwipeForCause.Api.Database;

namespace SwipeForCause.Api.Features.Moderation;

public record AdminOrganizationResponse(
    Guid Id,
    string Name,
    string Ein,
    string ContactEmail,
    string? WebsiteUrl,
    string? City,
    string? State,
    string VerificationStatus,
    DateTime CreatedAt,
    string? LogoUrl);

public static class ListOrganizations
{
    public static void MapListOrganizations(this WebApplication app)
    {
        app.MapGet("/api/v1/admin/organizations", async (
            string? status,
            string? cursor,
            int? limit,
            AppDbContext db) =>
        {
            var pageSize = Math.Clamp(limit ?? 20, 1, 100);

            var query = db.Organizations
                .Where(o => o.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.VerificationStatus == status);
            }

            if (!string.IsNullOrEmpty(cursor) && DateTime.TryParse(cursor, out var cursorDate))
            {
                query = query.Where(o => o.CreatedAt < cursorDate);
            }

            var orgs = await query
                .OrderByDescending(o => o.CreatedAt)
                .Take(pageSize + 1)
                .Select(o => new AdminOrganizationResponse(
                    o.Id,
                    o.Name,
                    o.Ein,
                    o.ContactEmail,
                    o.WebsiteUrl,
                    o.City,
                    o.State,
                    o.VerificationStatus,
                    o.CreatedAt,
                    o.LogoUrl))
                .ToListAsync();

            var hasMore = orgs.Count > pageSize;
            var data = hasMore ? orgs.Take(pageSize).ToList() : orgs;
            var nextCursor = hasMore ? data.Last().CreatedAt.ToString("O") : null;

            return Results.Ok(new PagedResponse<AdminOrganizationResponse>
            {
                Data = data,
                Cursor = nextCursor,
                HasMore = hasMore,
            });
        })
        .RequireAuthorization("Admin")
        .WithTags("Admin")
        .WithName("ListOrganizations");
    }
}
