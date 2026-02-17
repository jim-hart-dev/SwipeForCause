using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ScrollForCause.Api.Database;
using ScrollForCause.Api.Database.Entities;

namespace ScrollForCause.Api.Tests;

[Collection("Sequential")]
public class GetFeedTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
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

        TestAuthHandler.Reset();
    }

    public void Dispose()
    {
        TestAuthHandler.Reset();
    }

    private HttpClient CreateClient() => _factory.CreateClient();

    private async Task ClearSeedDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.PostMedia.RemoveRange(db.PostMedia);
        db.Posts.RemoveRange(db.Posts);
        db.Opportunities.RemoveRange(db.Opportunities);
        db.Organizations.RemoveRange(db.Organizations);
        await db.SaveChangesAsync();
    }

    private async Task<Organization> SeedOrganizationAsync(
        string verificationStatus = "verified",
        bool isActive = true,
        string? name = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            ClerkUserId = "clerk_" + Guid.NewGuid().ToString()[..8],
            Name = name ?? "Test Org " + Guid.NewGuid().ToString()[..8],
            Ein = "12-3456789",
            Description = "Test organization",
            ContactName = "Test Contact",
            ContactEmail = "test@example.com",
            VerificationStatus = verificationStatus,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();
        return org;
    }

    private async Task<Post> SeedPostAsync(
        Guid organizationId,
        string status = "active",
        DateTime? createdAt = null,
        Guid? opportunityId = null,
        string? title = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var post = new Post
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            OpportunityId = opportunityId,
            Title = title ?? "Test Post " + Guid.NewGuid().ToString()[..8],
            Description = "Test description",
            MediaType = "video",
            Status = status,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        db.Posts.Add(post);
        await db.SaveChangesAsync();
        return post;
    }

    private async Task<PostMedia> SeedPostMediaAsync(
        Guid postId,
        int displayOrder = 0)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var media = new PostMedia
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            MediaUrl = "https://cdn.example.com/video.mp4",
            ThumbnailUrl = "https://cdn.example.com/thumb.jpg",
            MediaType = "video",
            DurationSeconds = 30,
            Width = 1080,
            Height = 1920,
            DisplayOrder = displayOrder,
            ProcessingStatus = "completed",
            CreatedAt = DateTime.UtcNow,
        };
        db.PostMedia.Add(media);
        await db.SaveChangesAsync();
        return media;
    }

    private async Task<Opportunity> SeedOpportunityAsync(Guid organizationId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var opp = new Opportunity
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Title = "Test Opportunity",
            Description = "Help out",
            ScheduleType = "one-time",
            StartDate = DateTime.UtcNow.AddDays(7),
            LocationAddress = "123 Main St",
            IsRemote = false,
            TimeCommitment = "2 hours",
            Status = "active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        db.Opportunities.Add(opp);
        await db.SaveChangesAsync();
        return opp;
    }

    private static string EncodeCursor(DateTime createdAt, Guid postId)
    {
        var json = JsonSerializer.Serialize(new { createdAt = createdAt.ToString("O"), postId = postId.ToString() });
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    [Fact]
    public async Task GetFeed_EmptyDatabase_Returns200WithEmptyData()
    {
        await ClearSeedDataAsync();
        var client = CreateClient();

        var response = await client.GetAsync("/api/v1/feed");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<FeedResponse>();
        Assert.NotNull(body);
        Assert.Empty(body!.Data);
        Assert.False(body.HasMore);
        Assert.Null(body.Cursor);
    }

    [Fact]
    public async Task GetFeed_ReturnsPosts_OrderedByNewestFirst()
    {
        await ClearSeedDataAsync();
        var org = await SeedOrganizationAsync();
        var olderPost = await SeedPostAsync(org.Id, createdAt: DateTime.UtcNow.AddHours(-2), title: "Older Post");
        var newerPost = await SeedPostAsync(org.Id, createdAt: DateTime.UtcNow.AddHours(-1), title: "Newer Post");

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/feed");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<FeedResponse>();
        Assert.NotNull(body);
        Assert.Equal(2, body!.Data.Count);
        Assert.Equal(newerPost.Id, body.Data[0].PostId);
        Assert.Equal(olderPost.Id, body.Data[1].PostId);
    }

    [Fact]
    public async Task GetFeed_ExcludesInactivePosts()
    {
        await ClearSeedDataAsync();
        var org = await SeedOrganizationAsync();
        var activePost = await SeedPostAsync(org.Id, status: "active", title: "Active Post");
        await SeedPostAsync(org.Id, status: "draft", title: "Draft Post");

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/feed");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<FeedResponse>();
        Assert.NotNull(body);
        Assert.Single(body!.Data);
        Assert.Equal(activePost.Id, body.Data[0].PostId);
    }

    [Fact]
    public async Task GetFeed_ExcludesPostsFromUnverifiedOrgs()
    {
        await ClearSeedDataAsync();
        var verifiedOrg = await SeedOrganizationAsync(verificationStatus: "verified", name: "Verified Org");
        var pendingOrg = await SeedOrganizationAsync(verificationStatus: "pending", name: "Pending Org");
        var activePost = await SeedPostAsync(verifiedOrg.Id, title: "Verified Org Post");
        await SeedPostAsync(pendingOrg.Id, title: "Pending Org Post");

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/feed");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<FeedResponse>();
        Assert.NotNull(body);
        Assert.Single(body!.Data);
        Assert.Equal(activePost.Id, body.Data[0].PostId);
    }

    [Fact]
    public async Task GetFeed_ExcludesPostsFromInactiveOrgs()
    {
        await ClearSeedDataAsync();
        var activeOrg = await SeedOrganizationAsync(isActive: true, name: "Active Org");
        var inactiveOrg = await SeedOrganizationAsync(isActive: false, name: "Inactive Org");
        var activePost = await SeedPostAsync(activeOrg.Id, title: "Active Org Post");
        await SeedPostAsync(inactiveOrg.Id, title: "Inactive Org Post");

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/feed");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<FeedResponse>();
        Assert.NotNull(body);
        Assert.Single(body!.Data);
        Assert.Equal(activePost.Id, body.Data[0].PostId);
    }

    [Fact]
    public async Task GetFeed_CursorPagination_ReturnsNonOverlappingPages()
    {
        await ClearSeedDataAsync();
        var org = await SeedOrganizationAsync();
        var posts = new List<Post>();
        for (int i = 0; i < 5; i++)
        {
            var post = await SeedPostAsync(org.Id,
                createdAt: DateTime.UtcNow.AddHours(-(5 - i)),
                title: $"Post {i}");
            posts.Add(post);
        }

        var client = CreateClient();
        var allPostIds = new HashSet<Guid>();

        // Page 1
        var response1 = await client.GetAsync("/api/v1/feed?limit=2");
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        var page1 = await response1.Content.ReadFromJsonAsync<FeedResponse>();
        Assert.NotNull(page1);
        Assert.Equal(2, page1!.Data.Count);
        Assert.True(page1.HasMore);
        Assert.NotNull(page1.Cursor);
        foreach (var item in page1.Data) allPostIds.Add(item.PostId);

        // Page 2
        var response2 = await client.GetAsync($"/api/v1/feed?limit=2&cursor={page1.Cursor}");
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        var page2 = await response2.Content.ReadFromJsonAsync<FeedResponse>();
        Assert.NotNull(page2);
        Assert.Equal(2, page2!.Data.Count);
        Assert.True(page2.HasMore);
        Assert.NotNull(page2.Cursor);
        foreach (var item in page2.Data) allPostIds.Add(item.PostId);

        // Page 3
        var response3 = await client.GetAsync($"/api/v1/feed?limit=2&cursor={page2.Cursor}");
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
        var page3 = await response3.Content.ReadFromJsonAsync<FeedResponse>();
        Assert.NotNull(page3);
        Assert.Single(page3!.Data);
        Assert.False(page3.HasMore);
        Assert.Null(page3.Cursor);
        foreach (var item in page3.Data) allPostIds.Add(item.PostId);

        // All 5 posts returned, no overlap
        Assert.Equal(5, allPostIds.Count);
    }

    [Fact]
    public async Task GetFeed_DefaultLimit_Returns10Items()
    {
        await ClearSeedDataAsync();
        var org = await SeedOrganizationAsync();
        for (int i = 0; i < 15; i++)
        {
            await SeedPostAsync(org.Id,
                createdAt: DateTime.UtcNow.AddMinutes(-i),
                title: $"Post {i}");
        }

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/feed");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<FeedResponse>();
        Assert.NotNull(body);
        Assert.Equal(10, body!.Data.Count);
        Assert.True(body.HasMore);
    }

    [Fact]
    public async Task GetFeed_LimitExceedsMax_Returns400()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/feed?limit=50");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetFeed_InvalidCursor_Returns400()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/feed?cursor=not-a-valid-cursor");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetFeed_IncludesMediaOrgAndOpportunityData()
    {
        await ClearSeedDataAsync();
        var org = await SeedOrganizationAsync(name: "Media Test Org");
        var opp = await SeedOpportunityAsync(org.Id);
        var post = await SeedPostAsync(org.Id, opportunityId: opp.Id, title: "Full Post");
        var media = await SeedPostMediaAsync(post.Id);

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/feed");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<FeedResponse>();
        Assert.NotNull(body);
        Assert.Single(body!.Data);

        var item = body.Data[0];
        Assert.Equal(post.Id, item.PostId);
        Assert.Equal("Full Post", item.Title);
        Assert.Equal("video", item.MediaType);

        // Media
        Assert.Single(item.Media);
        Assert.Equal(media.Id, item.Media[0].Id);
        Assert.Equal("https://cdn.example.com/video.mp4", item.Media[0].Url);
        Assert.Equal("https://cdn.example.com/thumb.jpg", item.Media[0].ThumbnailUrl);
        Assert.Equal(30, item.Media[0].Duration);
        Assert.Equal(1080, item.Media[0].Width);
        Assert.Equal(1920, item.Media[0].Height);

        // Organization
        Assert.Equal(org.Id, item.Organization.Id);
        Assert.Equal("Media Test Org", item.Organization.Name);
        Assert.True(item.Organization.IsVerified);

        // Opportunity
        Assert.NotNull(item.Opportunity);
        Assert.Equal(opp.Id, item.Opportunity!.Id);
        Assert.Equal("Test Opportunity", item.Opportunity.Title);
        Assert.Equal("one-time", item.Opportunity.ScheduleType);
        Assert.Equal("123 Main St", item.Opportunity.Location);
        Assert.False(item.Opportunity.IsRemote);
        Assert.Equal("2 hours", item.Opportunity.TimeCommitment);
    }

    [Fact]
    public async Task GetFeed_PostWithoutOpportunity_ReturnsNullOpportunity()
    {
        await ClearSeedDataAsync();
        var org = await SeedOrganizationAsync();
        await SeedPostAsync(org.Id, title: "No Opp Post");

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/feed");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<FeedResponse>();
        Assert.NotNull(body);
        Assert.Single(body!.Data);
        Assert.Null(body.Data[0].Opportunity);
    }
}

// Test DTOs for deserialization
internal record FeedResponse
{
    [JsonPropertyName("data")]
    public List<FeedItemDto> Data { get; init; } = [];

    [JsonPropertyName("cursor")]
    public string? Cursor { get; init; }

    [JsonPropertyName("hasMore")]
    public bool HasMore { get; init; }
}

internal record FeedItemDto
{
    [JsonPropertyName("postId")]
    public Guid PostId { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("mediaType")]
    public string MediaType { get; init; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; init; }

    [JsonPropertyName("media")]
    public List<FeedMediaDto> Media { get; init; } = [];

    [JsonPropertyName("organization")]
    public FeedOrgDto Organization { get; init; } = null!;

    [JsonPropertyName("opportunity")]
    public FeedOpportunityDto? Opportunity { get; init; }
}

internal record FeedMediaDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;

    [JsonPropertyName("thumbnailUrl")]
    public string? ThumbnailUrl { get; init; }

    [JsonPropertyName("duration")]
    public int? Duration { get; init; }

    [JsonPropertyName("width")]
    public int? Width { get; init; }

    [JsonPropertyName("height")]
    public int? Height { get; init; }
}

internal record FeedOrgDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("logoUrl")]
    public string? LogoUrl { get; init; }

    [JsonPropertyName("isVerified")]
    public bool IsVerified { get; init; }
}

internal record FeedOpportunityDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("scheduleType")]
    public string ScheduleType { get; init; } = string.Empty;

    [JsonPropertyName("startDate")]
    public DateTime? StartDate { get; init; }

    [JsonPropertyName("location")]
    public string? Location { get; init; }

    [JsonPropertyName("isRemote")]
    public bool IsRemote { get; init; }

    [JsonPropertyName("timeCommitment")]
    public string? TimeCommitment { get; init; }
}
