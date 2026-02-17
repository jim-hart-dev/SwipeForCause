using System.Text;
using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ScrollForCause.Api.Common;
using ScrollForCause.Api.Database;

namespace ScrollForCause.Api.Features.Feed;

public record GetFeedRequest(string? Cursor, int? Limit);

public record FeedMediaInfo(
    Guid Id,
    string Url,
    string? ThumbnailUrl,
    int? Duration,
    int? Width,
    int? Height);

public record FeedOrganizationInfo(
    Guid Id,
    string Name,
    string? LogoUrl,
    bool IsVerified);

public record FeedOpportunityInfo(
    Guid Id,
    string Title,
    string ScheduleType,
    DateTime? StartDate,
    string? Location,
    bool IsRemote,
    string? TimeCommitment);

public record FeedItem(
    Guid PostId,
    string Title,
    string? Description,
    string MediaType,
    DateTime CreatedAt,
    List<FeedMediaInfo> Media,
    FeedOrganizationInfo Organization,
    FeedOpportunityInfo? Opportunity);

public class GetFeedValidator : AbstractValidator<GetFeedRequest>
{
    public GetFeedValidator()
    {
        When(x => x.Cursor != null, () =>
        {
            RuleFor(x => x.Cursor).Must(BeValidCursor)
                .WithMessage("Invalid cursor format.");
        });

        When(x => x.Limit.HasValue, () =>
        {
            RuleFor(x => x.Limit!.Value)
                .InclusiveBetween(1, 20)
                .WithMessage("Limit must be between 1 and 20.");
        });
    }

    private static bool BeValidCursor(string? cursor)
    {
        if (string.IsNullOrEmpty(cursor)) return false;
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("createdAt", out var ca)
                && doc.RootElement.TryGetProperty("postId", out var pi)
                && DateTime.TryParse(ca.GetString(), out _)
                && Guid.TryParse(pi.GetString(), out _);
        }
        catch
        {
            return false;
        }
    }
}

public static class GetFeed
{
    private record CursorPayload(DateTime CreatedAt, Guid PostId);

    private static CursorPayload? DecodeCursor(string? cursor)
    {
        if (string.IsNullOrEmpty(cursor)) return null;
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            using var doc = JsonDocument.Parse(json);
            var createdAt = DateTime.Parse(doc.RootElement.GetProperty("createdAt").GetString()!).ToUniversalTime();
            var postId = Guid.Parse(doc.RootElement.GetProperty("postId").GetString()!);
            return new CursorPayload(createdAt, postId);
        }
        catch
        {
            return null;
        }
    }

    private static string EncodeCursor(DateTime createdAt, Guid postId)
    {
        var json = JsonSerializer.Serialize(new { createdAt = createdAt.ToString("O"), postId = postId.ToString() });
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    public static void MapGetFeed(this WebApplication app)
    {
        app.MapGet("/api/v1/feed", async (
            string? cursor,
            int? limit,
            IValidator<GetFeedRequest> validator,
            AppDbContext db) =>
        {
            var request = new GetFeedRequest(cursor, limit);
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "VALIDATION_ERROR",
                        Message = "Validation failed.",
                        Details = validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }),
                    },
                });
            }

            var pageSize = limit ?? 10;
            var decoded = DecodeCursor(cursor);

            var query = db.Posts
                .Include(p => p.Organization)
                .Include(p => p.Opportunity)
                .Include(p => p.Media.OrderBy(m => m.DisplayOrder))
                .Where(p => p.Status == "active")
                .Where(p => p.Organization.VerificationStatus == "verified")
                .Where(p => p.Organization.IsActive)
                .AsQueryable();

            if (decoded != null)
            {
                query = query.Where(p =>
                    p.CreatedAt < decoded.CreatedAt ||
                    (p.CreatedAt == decoded.CreatedAt && p.Id < decoded.PostId));
            }

            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.Id)
                .Take(pageSize + 1)
                .ToListAsync();

            var hasMore = posts.Count > pageSize;
            var page = hasMore ? posts.Take(pageSize).ToList() : posts;

            var items = page.Select(p => new FeedItem(
                p.Id,
                p.Title,
                p.Description,
                p.MediaType,
                p.CreatedAt,
                p.Media.Select(m => new FeedMediaInfo(
                    m.Id,
                    m.MediaUrl,
                    m.ThumbnailUrl,
                    m.DurationSeconds,
                    m.Width,
                    m.Height)).ToList(),
                new FeedOrganizationInfo(
                    p.Organization.Id,
                    p.Organization.Name,
                    p.Organization.LogoUrl,
                    p.Organization.VerificationStatus == "verified"),
                p.Opportunity == null ? null : new FeedOpportunityInfo(
                    p.Opportunity.Id,
                    p.Opportunity.Title,
                    p.Opportunity.ScheduleType,
                    p.Opportunity.StartDate,
                    p.Opportunity.LocationAddress,
                    p.Opportunity.IsRemote,
                    p.Opportunity.TimeCommitment)
            )).ToList();

            var nextCursor = hasMore
                ? EncodeCursor(page.Last().CreatedAt, page.Last().Id)
                : null;

            return Results.Ok(new PagedResponse<FeedItem>
            {
                Data = items,
                Cursor = nextCursor,
                HasMore = hasMore,
            });
        })
        .WithTags("Feed")
        .WithName("GetFeed");
    }
}
