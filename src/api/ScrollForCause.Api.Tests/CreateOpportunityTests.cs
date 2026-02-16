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
public class CreateOpportunityTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _dbName;

    public CreateOpportunityTests(WebApplicationFactory<Program> factory)
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

    public void Dispose() => TestAuthHandler.Reset();

    private HttpClient CreateClient() => _factory.CreateClient();

    private async Task<Guid> SeedVerifiedOrganizationAsync(string clerkUserId = "user_test123")
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            ClerkUserId = clerkUserId,
            Name = "Test Org",
            Description = "A test organization",
            Ein = "12-3456789",
            ContactName = "Test Contact",
            ContactEmail = "test@org.com",
            VerificationStatus = "verified",
            VerifiedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();
        return org.Id;
    }

    private async Task<Guid> SeedPendingOrganizationAsync(string clerkUserId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            ClerkUserId = clerkUserId,
            Name = "Pending Org",
            Description = "A pending organization",
            Ein = "98-7654321",
            ContactName = "Pending Contact",
            ContactEmail = "pending@org.com",
            VerificationStatus = "pending",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();
        return org.Id;
    }

    [Fact]
    public async Task CreateOpportunity_ValidFlexible_Returns201()
    {
        TestAuthHandler.UserType = "organization";
        await SeedVerifiedOrganizationAsync();
        var client = CreateClient();

        var request = new
        {
            Title = "Beach Cleanup",
            Description = "Help us clean the beach",
            IsRemote = false,
            LocationAddress = "123 Beach Rd, Charleston, SC",
            ScheduleType = "flexible",
            TimeCommitment = "2-3 hours per week",
        };

        var response = await client.PostAsJsonAsync("/api/v1/opportunities", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CreateOpportunityResponseDto>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body!.OpportunityId);
        Assert.Equal("active", body.Status);
    }

    [Fact]
    public async Task CreateOpportunity_ValidOneTime_Returns201()
    {
        var clerkId = "user_onetime_" + Guid.NewGuid().ToString()[..8];
        TestAuthHandler.ClerkUserId = clerkId;
        TestAuthHandler.UserType = "organization";
        await SeedVerifiedOrganizationAsync(clerkId);
        var client = CreateClient();

        var request = new
        {
            Title = "Park Day",
            Description = "One-time park restoration event",
            IsRemote = false,
            ScheduleType = "one_time",
            StartDate = DateTime.UtcNow.AddDays(7).ToString("o"),
            EndDate = DateTime.UtcNow.AddDays(7).AddHours(4).ToString("o"),
            TimeCommitment = "4 hours",
        };

        var response = await client.PostAsJsonAsync("/api/v1/opportunities", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateOpportunity_ValidRecurring_Returns201()
    {
        var clerkId = "user_recurring_" + Guid.NewGuid().ToString()[..8];
        TestAuthHandler.ClerkUserId = clerkId;
        TestAuthHandler.UserType = "organization";
        await SeedVerifiedOrganizationAsync(clerkId);
        var client = CreateClient();

        var request = new
        {
            Title = "Weekly Tutoring",
            Description = "Help students with homework every week",
            IsRemote = true,
            ScheduleType = "recurring",
            RecurrenceDesc = "Every Saturday 9am-12pm",
            TimeCommitment = "3 hours per week",
        };

        var response = await client.PostAsJsonAsync("/api/v1/opportunities", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateOpportunity_WithAllOptionalFields_Returns201()
    {
        var clerkId = "user_allfields_" + Guid.NewGuid().ToString()[..8];
        TestAuthHandler.ClerkUserId = clerkId;
        TestAuthHandler.UserType = "organization";
        await SeedVerifiedOrganizationAsync(clerkId);
        var client = CreateClient();

        var request = new
        {
            Title = "Animal Shelter Helper",
            Description = "Assist with animal care at the local shelter",
            IsRemote = false,
            LocationAddress = "456 Shelter Lane",
            Latitude = 32.7765m,
            Longitude = -79.9311m,
            ScheduleType = "flexible",
            TimeCommitment = "4 hours per week",
            VolunteersNeeded = 10,
            SkillsRequired = "Must love animals",
            MinimumAge = 16,
        };

        var response = await client.PostAsJsonAsync("/api/v1/opportunities", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var body = await response.Content.ReadFromJsonAsync<CreateOpportunityResponseDto>();
        var opp = await db.Opportunities.FindAsync(body!.OpportunityId);
        Assert.NotNull(opp);
        Assert.Equal(10, opp!.VolunteersNeeded);
        Assert.Equal(16, opp.MinimumAge);
        Assert.Equal("Must love animals", opp.SkillsRequired);
    }

    [Fact]
    public async Task CreateOpportunity_MissingTitle_Returns400()
    {
        TestAuthHandler.UserType = "organization";
        await SeedVerifiedOrganizationAsync();
        var client = CreateClient();

        var request = new
        {
            Title = "",
            Description = "Some description",
            IsRemote = false,
            ScheduleType = "flexible",
            TimeCommitment = "2 hours",
        };

        var response = await client.PostAsJsonAsync("/api/v1/opportunities", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOpportunity_MissingDescription_Returns400()
    {
        var clerkId = "user_nodesc_" + Guid.NewGuid().ToString()[..8];
        TestAuthHandler.ClerkUserId = clerkId;
        TestAuthHandler.UserType = "organization";
        await SeedVerifiedOrganizationAsync(clerkId);
        var client = CreateClient();

        var request = new
        {
            Title = "Beach Cleanup",
            Description = "",
            IsRemote = false,
            ScheduleType = "flexible",
            TimeCommitment = "2 hours",
        };

        var response = await client.PostAsJsonAsync("/api/v1/opportunities", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOpportunity_InvalidScheduleType_Returns400()
    {
        var clerkId = "user_badsched_" + Guid.NewGuid().ToString()[..8];
        TestAuthHandler.ClerkUserId = clerkId;
        TestAuthHandler.UserType = "organization";
        await SeedVerifiedOrganizationAsync(clerkId);
        var client = CreateClient();

        var request = new
        {
            Title = "Beach Cleanup",
            Description = "Help clean the beach",
            IsRemote = false,
            ScheduleType = "invalid_type",
            TimeCommitment = "2 hours",
        };

        var response = await client.PostAsJsonAsync("/api/v1/opportunities", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOpportunity_OneTimeWithoutStartDate_Returns400()
    {
        var clerkId = "user_nostart_" + Guid.NewGuid().ToString()[..8];
        TestAuthHandler.ClerkUserId = clerkId;
        TestAuthHandler.UserType = "organization";
        await SeedVerifiedOrganizationAsync(clerkId);
        var client = CreateClient();

        var request = new
        {
            Title = "Park Day",
            Description = "One-time event",
            IsRemote = false,
            ScheduleType = "one_time",
            TimeCommitment = "4 hours",
        };

        var response = await client.PostAsJsonAsync("/api/v1/opportunities", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOpportunity_NoAuth_Returns401()
    {
        TestAuthHandler.ClerkUserId = null;
        var client = CreateClient();

        var request = new
        {
            Title = "Beach Cleanup",
            Description = "Help clean the beach",
            IsRemote = false,
            ScheduleType = "flexible",
            TimeCommitment = "2 hours",
        };

        var response = await client.PostAsJsonAsync("/api/v1/opportunities", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateOpportunity_VolunteerUserType_Returns403()
    {
        TestAuthHandler.UserType = "volunteer";
        var client = CreateClient();

        var request = new
        {
            Title = "Beach Cleanup",
            Description = "Help clean the beach",
            IsRemote = false,
            ScheduleType = "flexible",
            TimeCommitment = "2 hours",
        };

        var response = await client.PostAsJsonAsync("/api/v1/opportunities", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateOpportunity_UnverifiedOrg_Returns403()
    {
        var clerkId = "user_unverified_" + Guid.NewGuid().ToString()[..8];
        TestAuthHandler.ClerkUserId = clerkId;
        TestAuthHandler.UserType = "organization";
        await SeedPendingOrganizationAsync(clerkId);
        var client = CreateClient();

        var request = new
        {
            Title = "Beach Cleanup",
            Description = "Help clean the beach",
            IsRemote = false,
            ScheduleType = "flexible",
            TimeCommitment = "2 hours",
        };

        var response = await client.PostAsJsonAsync("/api/v1/opportunities", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateOpportunity_OrgNotFound_Returns404()
    {
        var clerkId = "user_noorg_" + Guid.NewGuid().ToString()[..8];
        TestAuthHandler.ClerkUserId = clerkId;
        TestAuthHandler.UserType = "organization";
        var client = CreateClient();

        var request = new
        {
            Title = "Beach Cleanup",
            Description = "Help clean the beach",
            IsRemote = false,
            ScheduleType = "flexible",
            TimeCommitment = "2 hours",
        };

        var response = await client.PostAsJsonAsync("/api/v1/opportunities", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateOpportunity_XssInTitle_SanitizesOutput()
    {
        var clerkId = "user_xss_" + Guid.NewGuid().ToString()[..8];
        TestAuthHandler.ClerkUserId = clerkId;
        TestAuthHandler.UserType = "organization";
        await SeedVerifiedOrganizationAsync(clerkId);
        var client = CreateClient();

        var request = new
        {
            Title = "<script>alert('xss')</script>Beach Cleanup",
            Description = "Help clean the beach",
            IsRemote = false,
            ScheduleType = "flexible",
            TimeCommitment = "2 hours",
        };

        var response = await client.PostAsJsonAsync("/api/v1/opportunities", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var body = await response.Content.ReadFromJsonAsync<CreateOpportunityResponseDto>();
        var opp = await db.Opportunities.FindAsync(body!.OpportunityId);
        Assert.DoesNotContain("<script>", opp!.Title);
        Assert.Contains("Beach Cleanup", opp.Title);
    }

    [Fact]
    public async Task CreateOpportunity_SavesCorrectOrgLinkage()
    {
        var clerkId = "user_linkage_" + Guid.NewGuid().ToString()[..8];
        TestAuthHandler.ClerkUserId = clerkId;
        TestAuthHandler.UserType = "organization";
        var orgId = await SeedVerifiedOrganizationAsync(clerkId);
        var client = CreateClient();

        var request = new
        {
            Title = "Beach Cleanup",
            Description = "Help clean the beach",
            IsRemote = false,
            ScheduleType = "flexible",
            TimeCommitment = "2 hours",
        };

        var response = await client.PostAsJsonAsync("/api/v1/opportunities", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var body = await response.Content.ReadFromJsonAsync<CreateOpportunityResponseDto>();
        var opp = await db.Opportunities.FindAsync(body!.OpportunityId);
        Assert.Equal(orgId, opp!.OrganizationId);
    }
}

internal record CreateOpportunityResponseDto(Guid OpportunityId, string Status);
