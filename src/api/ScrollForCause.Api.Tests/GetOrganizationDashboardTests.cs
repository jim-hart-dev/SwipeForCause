using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ScrollForCause.Api.Database;
using ScrollForCause.Api.Database.Entities;

namespace ScrollForCause.Api.Tests;

[Collection("Sequential")]
public class GetOrganizationDashboardTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _dbName;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public GetOrganizationDashboardTests(WebApplicationFactory<Program> factory)
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

    private void ConfigureAsOrg(string clerkUserId, Guid orgId)
    {
        TestAuthHandler.ClerkUserId = clerkUserId;
        TestAuthHandler.UserType = "organization";
        TestAuthHandler.ProfileId = orgId.ToString();
    }

    private async Task<Organization> SeedOrganizationAsync(string clerkUserId, string verificationStatus = "verified", string? coverImageUrl = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            ClerkUserId = clerkUserId,
            Name = "Test Nonprofit",
            Ein = "12-3456789",
            Description = "A test organization",
            ContactName = "Jane Doe",
            ContactEmail = "jane@test.org",
            VerificationStatus = verificationStatus,
            VerifiedAt = verificationStatus == "verified" ? DateTime.UtcNow : null,
            CoverImageUrl = coverImageUrl,
            FollowerCount = 42,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();
        return org;
    }

    private async Task<Opportunity> SeedOpportunityAsync(Guid orgId, string status = "active")
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var opp = new Opportunity
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            Title = "Test Opportunity",
            Description = "Help out",
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        db.Opportunities.Add(opp);
        await db.SaveChangesAsync();
        return opp;
    }

    private async Task<Post> SeedPostAsync(Guid orgId, string title = "Test Post", int viewCount = 10, DateTime? createdAt = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var post = new Post
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            Title = title,
            MediaType = "image",
            Status = "active",
            ViewCount = viewCount,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        db.Posts.Add(post);
        await db.SaveChangesAsync();
        return post;
    }

    private async Task<Volunteer> SeedVolunteerAsync(string clerkUserId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var volunteer = new Volunteer
        {
            Id = Guid.NewGuid(),
            ClerkUserId = clerkUserId,
            Email = $"{clerkUserId}@test.com",
            DisplayName = "Volunteer " + clerkUserId[..8],
            AvatarUrl = "https://example.com/avatar.jpg",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        db.Volunteers.Add(volunteer);
        await db.SaveChangesAsync();
        return volunteer;
    }

    private async Task<VolunteerInterest> SeedInterestAsync(Guid volunteerId, Guid opportunityId, string status = "pending", DateTime? createdAt = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var interest = new VolunteerInterest
        {
            Id = Guid.NewGuid(),
            VolunteerId = volunteerId,
            OpportunityId = opportunityId,
            Status = status,
            CreatedAt = createdAt ?? DateTime.UtcNow,
        };
        db.VolunteerInterests.Add(interest);
        await db.SaveChangesAsync();
        return interest;
    }

    [Fact]
    public async Task GetDashboard_VerifiedOrg_ReturnsOkWithStats()
    {
        var clerkUserId = "user_orgdash_" + Guid.NewGuid().ToString()[..8];
        var org = await SeedOrganizationAsync(clerkUserId, "verified");
        ConfigureAsOrg(clerkUserId, org.Id);

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/organizations/dashboard");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<DashboardResponseDto>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal(org.Id, body!.OrganizationId);
        Assert.Equal("Test Nonprofit", body.OrganizationName);
        Assert.Equal("verified", body.VerificationStatus);
        Assert.NotNull(body.Stats);
        Assert.Equal(42, body.Stats!.FollowerCount);
        Assert.NotNull(body.SetupChecklist);
    }

    [Fact]
    public async Task GetDashboard_PendingOrg_ReturnsPendingStatus()
    {
        var clerkUserId = "user_orgpend_" + Guid.NewGuid().ToString()[..8];
        var org = await SeedOrganizationAsync(clerkUserId, "pending");
        ConfigureAsOrg(clerkUserId, org.Id);

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/organizations/dashboard");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<DashboardResponseDto>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal("pending", body!.VerificationStatus);
        Assert.Null(body.Stats);
        Assert.Null(body.SetupChecklist);
        Assert.Empty(body.RecentInterests);
        Assert.Empty(body.RecentPosts);
    }

    [Fact]
    public async Task GetDashboard_RejectedOrg_ReturnsRejectedStatus()
    {
        var clerkUserId = "user_orgrej_" + Guid.NewGuid().ToString()[..8];
        var org = await SeedOrganizationAsync(clerkUserId, "rejected");
        ConfigureAsOrg(clerkUserId, org.Id);

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/organizations/dashboard");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<DashboardResponseDto>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal("rejected", body!.VerificationStatus);
        Assert.Null(body.Stats);
    }

    [Fact]
    public async Task GetDashboard_StatsReflectActualData()
    {
        var clerkUserId = "user_orgstats_" + Guid.NewGuid().ToString()[..8];
        var org = await SeedOrganizationAsync(clerkUserId, "verified");
        ConfigureAsOrg(clerkUserId, org.Id);

        // Seed 2 active opportunities, 1 closed
        var opp1 = await SeedOpportunityAsync(org.Id, "active");
        var opp2 = await SeedOpportunityAsync(org.Id, "active");
        var oppClosed = await SeedOpportunityAsync(org.Id, "closed");

        // Seed interests - 2 pending on active opps, 1 accepted
        var vol1 = await SeedVolunteerAsync("vol_stats1_" + Guid.NewGuid().ToString()[..8]);
        var vol2 = await SeedVolunteerAsync("vol_stats2_" + Guid.NewGuid().ToString()[..8]);
        var vol3 = await SeedVolunteerAsync("vol_stats3_" + Guid.NewGuid().ToString()[..8]);
        await SeedInterestAsync(vol1.Id, opp1.Id, "pending");
        await SeedInterestAsync(vol2.Id, opp2.Id, "pending");
        await SeedInterestAsync(vol3.Id, opp1.Id, "accepted");

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/organizations/dashboard");
        var body = await response.Content.ReadFromJsonAsync<DashboardResponseDto>(JsonOptions);

        Assert.Equal(2, body!.Stats!.ActiveOpportunityCount);
        Assert.Equal(2, body.Stats.NewInterestCount);
        Assert.Equal(42, body.Stats.FollowerCount);
    }

    [Fact]
    public async Task GetDashboard_RecentInterests_ReturnsLatest5OrderedByDate()
    {
        var clerkUserId = "user_orgint_" + Guid.NewGuid().ToString()[..8];
        var org = await SeedOrganizationAsync(clerkUserId, "verified");
        ConfigureAsOrg(clerkUserId, org.Id);

        var opp = await SeedOpportunityAsync(org.Id);

        // Seed 7 interests with different dates
        for (int i = 0; i < 7; i++)
        {
            var vol = await SeedVolunteerAsync($"vol_int{i}_" + Guid.NewGuid().ToString()[..8]);
            await SeedInterestAsync(vol.Id, opp.Id, "pending", DateTime.UtcNow.AddHours(-(6 - i)));
        }

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/organizations/dashboard");
        var body = await response.Content.ReadFromJsonAsync<DashboardResponseDto>(JsonOptions);

        Assert.Equal(5, body!.RecentInterests.Count);
        // Verify ordered by date descending
        for (int i = 0; i < body.RecentInterests.Count - 1; i++)
        {
            Assert.True(body.RecentInterests[i].CreatedAt >= body.RecentInterests[i + 1].CreatedAt);
        }
    }

    [Fact]
    public async Task GetDashboard_RecentPosts_ReturnsLatest3OrderedByDate()
    {
        var clerkUserId = "user_orgposts_" + Guid.NewGuid().ToString()[..8];
        var org = await SeedOrganizationAsync(clerkUserId, "verified");
        ConfigureAsOrg(clerkUserId, org.Id);

        // Seed 5 posts with distinct timestamps
        for (int i = 0; i < 5; i++)
        {
            await SeedPostAsync(org.Id, $"Post {i}", viewCount: i * 10, createdAt: DateTime.UtcNow.AddHours(-(4 - i)));
        }

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/organizations/dashboard");
        var body = await response.Content.ReadFromJsonAsync<DashboardResponseDto>(JsonOptions);

        Assert.Equal(3, body!.RecentPosts.Count);
        // Verify ordered by date descending
        for (int i = 0; i < body.RecentPosts.Count - 1; i++)
        {
            Assert.True(body.RecentPosts[i].CreatedAt >= body.RecentPosts[i + 1].CreatedAt);
        }
    }

    [Fact]
    public async Task GetDashboard_SetupChecklist_ReflectsActualState()
    {
        var clerkUserId = "user_orgcheck_" + Guid.NewGuid().ToString()[..8];
        var org = await SeedOrganizationAsync(clerkUserId, "verified", coverImageUrl: null);
        ConfigureAsOrg(clerkUserId, org.Id);

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/organizations/dashboard");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<DashboardResponseDto>(JsonOptions);

        Assert.False(body!.SetupChecklist!.HasCoverImage);
        Assert.False(body.SetupChecklist.HasOpportunity);
        Assert.False(body.SetupChecklist.HasPost);
    }

    [Fact]
    public async Task GetDashboard_SetupChecklist_TrueWhenDataExists()
    {
        var clerkUserId = "user_orgchk2_" + Guid.NewGuid().ToString()[..8];
        var org = await SeedOrganizationAsync(clerkUserId, "verified", coverImageUrl: "https://example.com/cover.jpg");
        ConfigureAsOrg(clerkUserId, org.Id);

        await SeedOpportunityAsync(org.Id);
        await SeedPostAsync(org.Id);

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/organizations/dashboard");
        var body = await response.Content.ReadFromJsonAsync<DashboardResponseDto>(JsonOptions);

        Assert.True(body!.SetupChecklist!.HasCoverImage);
        Assert.True(body.SetupChecklist.HasOpportunity);
        Assert.True(body.SetupChecklist.HasPost);
    }

    [Fact]
    public async Task GetDashboard_NonOrgUser_Returns403()
    {
        TestAuthHandler.ClerkUserId = "user_volunteer_" + Guid.NewGuid().ToString()[..8];
        TestAuthHandler.UserType = "volunteer";

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/organizations/dashboard");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetDashboard_Unauthenticated_Returns401()
    {
        TestAuthHandler.ClerkUserId = null;

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/organizations/dashboard");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDashboard_OrgNotFound_Returns404()
    {
        var clerkUserId = "user_noorg_" + Guid.NewGuid().ToString()[..8];
        TestAuthHandler.ClerkUserId = clerkUserId;
        TestAuthHandler.UserType = "organization";

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/organizations/dashboard");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

// DTOs for deserializing camelCase JSON responses in tests
internal record DashboardResponseDto(
    Guid OrganizationId,
    string OrganizationName,
    string VerificationStatus,
    DashboardStatsDto? Stats,
    List<InterestSummaryDto> RecentInterests,
    List<PostSummaryDto> RecentPosts,
    SetupChecklistDto? SetupChecklist);

internal record DashboardStatsDto(
    int NewInterestCount,
    int ActiveOpportunityCount,
    int FollowerCount);

internal record InterestSummaryDto(
    Guid InterestId,
    string VolunteerName,
    string? VolunteerAvatarUrl,
    string OpportunityTitle,
    string Status,
    DateTime CreatedAt);

internal record PostSummaryDto(
    Guid PostId,
    string Title,
    string? ThumbnailUrl,
    int ViewCount,
    DateTime CreatedAt);

internal record SetupChecklistDto(
    bool HasCoverImage,
    bool HasOpportunity,
    bool HasPost);
