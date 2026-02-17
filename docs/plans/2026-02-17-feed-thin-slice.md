# Feed Thin Slice Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build a public, chronological feed endpoint (`GET /api/v1/feed`) with cursor-based pagination.

**Architecture:** Single vertical slice file (`Features/Feed/GetFeed.cs`) with request, response records, validator, and minimal API handler. EF Core eager loading query filtered to active posts from verified orgs, ordered newest-first. Cursor is base64-encoded `(CreatedAt, PostId)` for stable pagination.

**Tech Stack:** C# .NET 8 Minimal API, EF Core, FluentValidation, xUnit, InMemory DB for tests.

**Design doc:** `docs/plans/2026-02-17-feed-thin-slice-design.md`

---

### Task 1: Create GitHub Issues

**Step 1: Create the thin-slice issue**

Create a new GitHub issue for this thin slice. Use `gh` CLI:

```bash
export PATH="/c/Program Files/GitHub CLI:$PATH"
gh issue create --repo jim-hart-dev/ScrollForCause \
  --title "Build chronological feed endpoint (thin slice)" \
  --label "epic:feed,priority:p0,slice:backend" \
  --body "$(cat <<'EOF'
## User Story
As a developer, I want a simple chronological feed endpoint so the frontend team can build the feed UI against real data.

## Acceptance Criteria
- [ ] `GET /api/v1/feed` endpoint returns paginated feed items (public, no auth)
- [ ] Cursor-based pagination with configurable limit (default 10, max 20)
- [ ] Each feed item includes: post data, media URLs, organization info, opportunity info (if linked)
- [ ] Feed is chronological (newest first), no personalization
- [ ] Only active posts from verified, active organizations appear
- [ ] Invalid cursor returns 400

## Scope
Thin slice of #13. No personalization, no view tracking, no auth-dependent user state fields. Those remain in #13.

## Technical Spec
See `docs/plans/2026-02-17-feed-thin-slice-design.md`
EOF
)"
```

Note the returned issue number — use it for the worktree branch name.

**Step 2: Update issue #13 body**

Edit issue #13 to note the thin slice was extracted:

```bash
gh issue comment 13 --repo jim-hart-dev/ScrollForCause \
  --body "Thin slice extracted to #<new-issue-number>: chronological feed with no personalization. This issue now covers only the personalization layer (interest boosting, view deduplication, user state fields, auth behavior, rate limiting)."
```

**Step 3: Commit the design and plan docs**

```bash
git add docs/plans/2026-02-17-feed-thin-slice-design.md docs/plans/2026-02-17-feed-thin-slice.md
git commit -m "docs: add feed thin slice design and implementation plan"
```

---

### Task 2: Set Up Worktree and Feature Branch

**Step 1: Create worktree**

From the repo root (`/c/Code/SwipeForCause`):

```bash
git worktree add /c/Code/ScrollForCause-wt-<issue-number> -b feature/<issue-number>-feed-thin-slice main
```

Replace `<issue-number>` with the number from Task 1.

**Step 2: Verify worktree**

```bash
cd /c/Code/ScrollForCause-wt-<issue-number>
git branch --show-current
# Expected: feature/<issue-number>-feed-thin-slice
```

All remaining tasks run from this worktree.

---

### Task 3: Write the GetFeed Vertical Slice

**Files:**
- Create: `src/api/ScrollForCause.Api/Features/Feed/GetFeed.cs`
- Modify: `src/api/ScrollForCause.Api/Program.cs`

**Step 1: Create the feature file**

Create `src/api/ScrollForCause.Api/Features/Feed/GetFeed.cs` with the following content:

```csharp
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
            var doc = JsonDocument.Parse(json);
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
            var doc = JsonDocument.Parse(json);
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
                    Errors = validation.Errors.Select(e => new ErrorDetail
                    {
                        Code = "VALIDATION_ERROR",
                        Message = e.ErrorMessage
                    }).ToList()
                });
            }

            var pageSize = Math.Clamp(limit ?? 10, 1, 20);
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
                    (p.CreatedAt == decoded.CreatedAt && p.Id.CompareTo(decoded.PostId) < 0));
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
```

**Step 2: Register the endpoint in Program.cs**

Add `using ScrollForCause.Api.Features.Feed;` to the top of `Program.cs`.

Then add `app.MapGetFeed();` after the existing endpoint registrations (after `app.MapCreateOpportunity();`, before `app.Run();`).

**Step 3: Verify it builds**

```bash
cd src/api && dotnet build
```

Expected: Build succeeded.

**Step 4: Commit**

```bash
git add src/api/ScrollForCause.Api/Features/Feed/GetFeed.cs src/api/ScrollForCause.Api/Program.cs
git commit -m "feat: add GET /api/v1/feed chronological endpoint"
```

---

### Task 4: Write Tests — Empty Feed and Ordering

**Files:**
- Create: `src/api/ScrollForCause.Api.Tests/GetFeedTests.cs`

**Step 1: Create the test file with fixtures and first two tests**

Create `src/api/ScrollForCause.Api.Tests/GetFeedTests.cs`:

```csharp
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ScrollForCause.Api.Database;
using ScrollForCause.Api.Database.Entities;

namespace ScrollForCause.Api.Tests;

[Collection("Sequential")]
public class GetFeedTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _dbName;

    public GetFeedTests(WebApplicationFactory<Program> factory)
    {
        _dbName = Guid.NewGuid().ToString();
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(_dbName));

                services.AddAuthentication("TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });
            });
        });
    }

    private HttpClient CreateClient() => _factory.CreateClient();

    private async Task SeedOrganizationAsync(Guid id, string verificationStatus = "verified", bool isActive = true)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Organizations.Add(new Organization
        {
            Id = id,
            ClerkUserId = $"clerk_{id}",
            Name = $"Org {id.ToString()[..8]}",
            Ein = "12-3456789",
            Description = "Test org",
            ContactName = "Test",
            ContactEmail = "test@test.com",
            VerificationStatus = verificationStatus,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
    }

    private async Task SeedPostAsync(Guid id, Guid orgId, string status = "active", DateTime? createdAt = null, Guid? opportunityId = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Posts.Add(new Post
        {
            Id = id,
            OrganizationId = orgId,
            Title = $"Post {id.ToString()[..8]}",
            Description = "Test post",
            MediaType = "video",
            Status = status,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            OpportunityId = opportunityId,
        });
        await db.SaveChangesAsync();
    }

    private async Task SeedPostMediaAsync(Guid postId, string url = "https://cdn.test.com/video.mp4")
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Set<PostMedia>().Add(new PostMedia
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            MediaUrl = url,
            ThumbnailUrl = "https://cdn.test.com/thumb.jpg",
            MediaType = "video",
            DurationSeconds = 30,
            Width = 1080,
            Height = 1920,
            DisplayOrder = 0,
            ProcessingStatus = "completed",
        });
        await db.SaveChangesAsync();
    }

    private async Task SeedOpportunityAsync(Guid id, Guid orgId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Set<Opportunity>().Add(new Opportunity
        {
            Id = id,
            OrganizationId = orgId,
            Title = "Test Opportunity",
            Description = "Help out",
            ScheduleType = "one_time",
            IsRemote = true,
            Status = "active",
        });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetFeed_EmptyDatabase_Returns200WithEmptyData()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/v1/feed");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var feed = await response.Content.ReadFromJsonAsync<FeedResponse>();
        Assert.NotNull(feed);
        Assert.Empty(feed!.Data);
        Assert.False(feed.HasMore);
        Assert.Null(feed.Cursor);
    }

    [Fact]
    public async Task GetFeed_ReturnsPosts_OrderedByNewestFirst()
    {
        var orgId = Guid.NewGuid();
        await SeedOrganizationAsync(orgId);

        var oldPostId = Guid.NewGuid();
        var newPostId = Guid.NewGuid();
        await SeedPostAsync(oldPostId, orgId, createdAt: DateTime.UtcNow.AddHours(-2));
        await SeedPostAsync(newPostId, orgId, createdAt: DateTime.UtcNow.AddHours(-1));

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/feed");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var feed = await response.Content.ReadFromJsonAsync<FeedResponse>();
        Assert.NotNull(feed);
        Assert.Equal(2, feed!.Data.Count);
        Assert.Equal(newPostId, feed.Data[0].PostId);
        Assert.Equal(oldPostId, feed.Data[1].PostId);
    }
}

// DTOs for deserializing test responses
internal record FeedResponse(List<FeedItemDto> Data, string? Cursor, bool HasMore);
internal record FeedItemDto(
    Guid PostId, string Title, string? Description, string MediaType, DateTime CreatedAt,
    List<FeedMediaDto> Media, FeedOrgDto Organization, FeedOpportunityDto? Opportunity);
internal record FeedMediaDto(Guid Id, string Url, string? ThumbnailUrl, int? Duration, int? Width, int? Height);
internal record FeedOrgDto(Guid Id, string Name, string? LogoUrl, bool IsVerified);
internal record FeedOpportunityDto(Guid Id, string Title, string ScheduleType, DateTime? StartDate, string? Location, bool IsRemote, string? TimeCommitment);
```

**Step 2: Run the tests**

```bash
cd src/api && dotnet test --filter "FullyQualifiedName~GetFeedTests"
```

Expected: 2 tests pass.

**Step 3: Commit**

```bash
git add src/api/ScrollForCause.Api.Tests/GetFeedTests.cs
git commit -m "test: add feed tests for empty response and ordering"
```

---

### Task 5: Write Tests — Filtering (Active Posts, Verified Orgs)

**Files:**
- Modify: `src/api/ScrollForCause.Api.Tests/GetFeedTests.cs`

**Step 1: Add filtering tests**

Add these tests to the `GetFeedTests` class:

```csharp
[Fact]
public async Task GetFeed_ExcludesInactivePosts()
{
    var orgId = Guid.NewGuid();
    await SeedOrganizationAsync(orgId);

    var activePostId = Guid.NewGuid();
    var inactivePostId = Guid.NewGuid();
    await SeedPostAsync(activePostId, orgId, status: "active");
    await SeedPostAsync(inactivePostId, orgId, status: "draft");

    var client = CreateClient();
    var response = await client.GetAsync("/api/v1/feed");

    var feed = await response.Content.ReadFromJsonAsync<FeedResponse>();
    Assert.Single(feed!.Data);
    Assert.Equal(activePostId, feed.Data[0].PostId);
}

[Fact]
public async Task GetFeed_ExcludesPostsFromUnverifiedOrgs()
{
    var verifiedOrgId = Guid.NewGuid();
    var pendingOrgId = Guid.NewGuid();
    await SeedOrganizationAsync(verifiedOrgId, verificationStatus: "verified");
    await SeedOrganizationAsync(pendingOrgId, verificationStatus: "pending");

    var verifiedPostId = Guid.NewGuid();
    var pendingPostId = Guid.NewGuid();
    await SeedPostAsync(verifiedPostId, verifiedOrgId);
    await SeedPostAsync(pendingPostId, pendingOrgId);

    var client = CreateClient();
    var response = await client.GetAsync("/api/v1/feed");

    var feed = await response.Content.ReadFromJsonAsync<FeedResponse>();
    Assert.Single(feed!.Data);
    Assert.Equal(verifiedPostId, feed.Data[0].PostId);
}

[Fact]
public async Task GetFeed_ExcludesPostsFromInactiveOrgs()
{
    var activeOrgId = Guid.NewGuid();
    var inactiveOrgId = Guid.NewGuid();
    await SeedOrganizationAsync(activeOrgId, isActive: true);
    await SeedOrganizationAsync(inactiveOrgId, isActive: false);

    var activePostId = Guid.NewGuid();
    var inactivePostId = Guid.NewGuid();
    await SeedPostAsync(activePostId, activeOrgId);
    await SeedPostAsync(inactivePostId, inactiveOrgId);

    var client = CreateClient();
    var response = await client.GetAsync("/api/v1/feed");

    var feed = await response.Content.ReadFromJsonAsync<FeedResponse>();
    Assert.Single(feed!.Data);
    Assert.Equal(activePostId, feed.Data[0].PostId);
}
```

**Step 2: Run the tests**

```bash
cd src/api && dotnet test --filter "FullyQualifiedName~GetFeedTests"
```

Expected: 5 tests pass.

**Step 3: Commit**

```bash
git add src/api/ScrollForCause.Api.Tests/GetFeedTests.cs
git commit -m "test: add feed filtering tests for active/verified status"
```

---

### Task 6: Write Tests — Cursor Pagination

**Files:**
- Modify: `src/api/ScrollForCause.Api.Tests/GetFeedTests.cs`

**Step 1: Add pagination tests**

Add these tests to the `GetFeedTests` class:

```csharp
[Fact]
public async Task GetFeed_CursorPagination_ReturnsNonOverlappingPages()
{
    var orgId = Guid.NewGuid();
    await SeedOrganizationAsync(orgId);

    // Create 5 posts with distinct timestamps
    var postIds = new List<Guid>();
    for (var i = 0; i < 5; i++)
    {
        var id = Guid.NewGuid();
        postIds.Add(id);
        await SeedPostAsync(id, orgId, createdAt: DateTime.UtcNow.AddMinutes(-i));
    }

    var client = CreateClient();

    // Page 1: get first 2
    var response1 = await client.GetAsync("/api/v1/feed?limit=2");
    var feed1 = await response1.Content.ReadFromJsonAsync<FeedResponse>();
    Assert.Equal(2, feed1!.Data.Count);
    Assert.True(feed1.HasMore);
    Assert.NotNull(feed1.Cursor);

    // Page 2: use cursor
    var response2 = await client.GetAsync($"/api/v1/feed?limit=2&cursor={feed1.Cursor}");
    var feed2 = await response2.Content.ReadFromJsonAsync<FeedResponse>();
    Assert.Equal(2, feed2!.Data.Count);
    Assert.True(feed2.HasMore);

    // Page 3: last page
    var response3 = await client.GetAsync($"/api/v1/feed?limit=2&cursor={feed2.Cursor}");
    var feed3 = await response3.Content.ReadFromJsonAsync<FeedResponse>();
    Assert.Single(feed3!.Data);
    Assert.False(feed3.HasMore);
    Assert.Null(feed3.Cursor);

    // Verify no overlaps
    var allIds = feed1.Data.Select(f => f.PostId)
        .Concat(feed2.Data.Select(f => f.PostId))
        .Concat(feed3.Data.Select(f => f.PostId))
        .ToList();
    Assert.Equal(5, allIds.Distinct().Count());
}

[Fact]
public async Task GetFeed_DefaultLimit_Returns10Items()
{
    var orgId = Guid.NewGuid();
    await SeedOrganizationAsync(orgId);

    for (var i = 0; i < 15; i++)
    {
        await SeedPostAsync(Guid.NewGuid(), orgId, createdAt: DateTime.UtcNow.AddMinutes(-i));
    }

    var client = CreateClient();
    var response = await client.GetAsync("/api/v1/feed");

    var feed = await response.Content.ReadFromJsonAsync<FeedResponse>();
    Assert.Equal(10, feed!.Data.Count);
    Assert.True(feed.HasMore);
}

[Fact]
public async Task GetFeed_LimitClamped_ToMax20()
{
    var orgId = Guid.NewGuid();
    await SeedOrganizationAsync(orgId);

    for (var i = 0; i < 25; i++)
    {
        await SeedPostAsync(Guid.NewGuid(), orgId, createdAt: DateTime.UtcNow.AddMinutes(-i));
    }

    var client = CreateClient();
    var response = await client.GetAsync("/api/v1/feed?limit=50");

    // Limit > 20 should be rejected by validator
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}

[Fact]
public async Task GetFeed_InvalidCursor_Returns400()
{
    var client = CreateClient();
    var response = await client.GetAsync("/api/v1/feed?cursor=not-valid-base64!!!");

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}
```

**Step 2: Run the tests**

```bash
cd src/api && dotnet test --filter "FullyQualifiedName~GetFeedTests"
```

Expected: 9 tests pass.

**Step 3: Commit**

```bash
git add src/api/ScrollForCause.Api.Tests/GetFeedTests.cs
git commit -m "test: add feed pagination and validation tests"
```

---

### Task 7: Write Tests — Response Shape (Media, Org, Opportunity)

**Files:**
- Modify: `src/api/ScrollForCause.Api.Tests/GetFeedTests.cs`

**Step 1: Add response shape test**

Add this test to the `GetFeedTests` class:

```csharp
[Fact]
public async Task GetFeed_IncludesMediaOrgAndOpportunityData()
{
    var orgId = Guid.NewGuid();
    await SeedOrganizationAsync(orgId);

    var opportunityId = Guid.NewGuid();
    await SeedOpportunityAsync(opportunityId, orgId);

    var postId = Guid.NewGuid();
    await SeedPostAsync(postId, orgId, opportunityId: opportunityId);
    await SeedPostMediaAsync(postId);

    var client = CreateClient();
    var response = await client.GetAsync("/api/v1/feed");

    var feed = await response.Content.ReadFromJsonAsync<FeedResponse>();
    Assert.Single(feed!.Data);

    var item = feed.Data[0];

    // Post fields
    Assert.Equal(postId, item.PostId);
    Assert.StartsWith("Post ", item.Title);
    Assert.Equal("video", item.MediaType);

    // Organization
    Assert.Equal(orgId, item.Organization.Id);
    Assert.True(item.Organization.IsVerified);

    // Media
    Assert.Single(item.Media);
    Assert.Equal("https://cdn.test.com/video.mp4", item.Media[0].Url);
    Assert.Equal(1080, item.Media[0].Width);
    Assert.Equal(1920, item.Media[0].Height);

    // Opportunity
    Assert.NotNull(item.Opportunity);
    Assert.Equal(opportunityId, item.Opportunity!.Id);
    Assert.Equal("one_time", item.Opportunity.ScheduleType);
    Assert.True(item.Opportunity.IsRemote);
}

[Fact]
public async Task GetFeed_PostWithoutOpportunity_ReturnsNullOpportunity()
{
    var orgId = Guid.NewGuid();
    await SeedOrganizationAsync(orgId);

    var postId = Guid.NewGuid();
    await SeedPostAsync(postId, orgId);

    var client = CreateClient();
    var response = await client.GetAsync("/api/v1/feed");

    var feed = await response.Content.ReadFromJsonAsync<FeedResponse>();
    Assert.Single(feed!.Data);
    Assert.Null(feed.Data[0].Opportunity);
}
```

**Step 2: Run the tests**

```bash
cd src/api && dotnet test --filter "FullyQualifiedName~GetFeedTests"
```

Expected: 11 tests pass.

**Step 3: Commit**

```bash
git add src/api/ScrollForCause.Api.Tests/GetFeedTests.cs
git commit -m "test: add feed response shape tests for media, org, opportunity"
```

---

### Task 8: Final Verification and PR

**Step 1: Run all tests in the project**

```bash
cd src/api && dotnet test
```

Expected: All tests pass (existing + 11 new).

**Step 2: Build the full solution**

```bash
cd src/api && dotnet build
```

Expected: Build succeeded.

**Step 3: Push and create PR**

```bash
export PATH="/c/Program Files/GitHub CLI:$PATH"
git push -u origin feature/<issue-number>-feed-thin-slice

gh pr create --repo jim-hart-dev/ScrollForCause \
  --title "feat: add chronological feed endpoint (GET /api/v1/feed)" \
  --body "$(cat <<'EOF'
## Summary
- Adds `GET /api/v1/feed` — public, chronological, cursor-paginated feed endpoint
- Returns active posts from verified orgs with media, org info, and optional opportunity info
- Thin slice of #13; no personalization, no auth, no view tracking

Closes #<issue-number>

## Test plan
- [x] 11 integration tests covering: empty feed, ordering, filtering (active/verified/active-org), cursor pagination, limit validation, invalid cursor, response shape (media, org, opportunity), null opportunity

🤖 Generated with [Claude Code](https://claude.com/claude-code)
EOF
)"
```
