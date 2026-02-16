using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SwipeForCause.Api.Database;
using SwipeForCause.Api.Database.Entities;
using SwipeForCause.Api.Infrastructure.Email;

namespace SwipeForCause.Api.Tests;

[Collection("Sequential")]
public class VerifyOrganizationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _dbName;

    public VerifyOrganizationTests(WebApplicationFactory<Program> factory)
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

                // Replace email service with a no-op for tests
                var emailDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IEmailService));
                if (emailDescriptor != null)
                    services.Remove(emailDescriptor);
                services.AddSingleton<IEmailService, FakeEmailService>();
            });
        });

        TestAuthHandler.ClerkUserId = "admin_test123";
        TestAuthHandler.UserType = "admin";
    }

    public void Dispose()
    {
        TestAuthHandler.ClerkUserId = "user_test123";
        TestAuthHandler.UserType = null;
    }

    private HttpClient CreateClient() => _factory.CreateClient();

    private async Task<Organization> SeedOrganizationAsync(string status = "pending")
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            ClerkUserId = "org_clerk_" + Guid.NewGuid().ToString()[..8],
            Name = "Test Nonprofit",
            Ein = "12-3456789",
            Description = "A test nonprofit organization.",
            ContactName = "John Doe",
            ContactEmail = "org@example.com",
            WebsiteUrl = "https://example.com",
            City = "Portland",
            State = "OR",
            VerificationStatus = status,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();
        return org;
    }

    [Fact]
    public async Task ListOrganizations_Admin_ReturnsOk()
    {
        await SeedOrganizationAsync("pending");
        var client = CreateClient();

        var response = await client.GetAsync("/api/v1/admin/organizations?status=pending");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PagedResponseDto<AdminOrgDto>>();
        Assert.NotNull(body);
        Assert.NotEmpty(body!.Data);
        Assert.Equal("pending", body.Data[0].VerificationStatus);
    }

    [Fact]
    public async Task GetOrganizationDetail_Admin_ReturnsOk()
    {
        var org = await SeedOrganizationAsync();
        var client = CreateClient();

        var response = await client.GetAsync($"/api/v1/admin/organizations/{org.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AdminOrgDetailDto>();
        Assert.NotNull(body);
        Assert.Equal(org.Name, body!.Name);
        Assert.Equal(org.Ein, body.Ein);
        Assert.Equal(org.Description, body.Description);
        Assert.Equal(org.ContactName, body.ContactName);
    }

    [Fact]
    public async Task GetOrganizationDetail_NotFound_Returns404()
    {
        var client = CreateClient();

        var response = await client.GetAsync($"/api/v1/admin/organizations/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task VerifyOrganization_Approved_UpdatesStatus()
    {
        var org = await SeedOrganizationAsync("pending");
        var client = CreateClient();

        var request = new { Status = "verified", Reason = (string?)null };
        var response = await client.PutAsJsonAsync($"/api/v1/admin/organizations/{org.Id}/verify", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<VerifyOrgResponseDto>();
        Assert.NotNull(body);
        Assert.Equal("verified", body!.VerificationStatus);
        Assert.NotNull(body.VerifiedAt);

        // Verify in database
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var updated = await db.Organizations.FindAsync(org.Id);
        Assert.Equal("verified", updated!.VerificationStatus);
        Assert.NotNull(updated.VerifiedAt);
    }

    [Fact]
    public async Task VerifyOrganization_Rejected_WithReason_UpdatesStatus()
    {
        var org = await SeedOrganizationAsync("pending");
        var client = CreateClient();

        var request = new { Status = "rejected", Reason = "Invalid EIN number." };
        var response = await client.PutAsJsonAsync($"/api/v1/admin/organizations/{org.Id}/verify", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<VerifyOrgResponseDto>();
        Assert.NotNull(body);
        Assert.Equal("rejected", body!.VerificationStatus);
        Assert.Null(body.VerifiedAt);
    }

    [Fact]
    public async Task VerifyOrganization_Rejected_WithoutReason_Returns400()
    {
        var org = await SeedOrganizationAsync("pending");
        var client = CreateClient();

        var request = new { Status = "rejected", Reason = (string?)null };
        var response = await client.PutAsJsonAsync($"/api/v1/admin/organizations/{org.Id}/verify", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task VerifyOrganization_InvalidStatus_Returns400()
    {
        var org = await SeedOrganizationAsync("pending");
        var client = CreateClient();

        var request = new { Status = "invalid", Reason = (string?)null };
        var response = await client.PutAsJsonAsync($"/api/v1/admin/organizations/{org.Id}/verify", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task VerifyOrganization_NotFound_Returns404()
    {
        var client = CreateClient();

        var request = new { Status = "verified", Reason = (string?)null };
        var response = await client.PutAsJsonAsync($"/api/v1/admin/organizations/{Guid.NewGuid()}/verify", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ListOrganizations_NonAdmin_Returns403()
    {
        TestAuthHandler.UserType = "volunteer";
        var client = CreateClient();

        var response = await client.GetAsync("/api/v1/admin/organizations");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ListOrganizations_Unauthenticated_Returns401()
    {
        TestAuthHandler.ClerkUserId = null;
        TestAuthHandler.UserType = null;
        var client = CreateClient();

        var response = await client.GetAsync("/api/v1/admin/organizations");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

// DTOs for deserializing test responses
internal record AdminOrgDto(
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

internal record AdminOrgDetailDto(
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
    string? CoverImageUrl);

internal record VerifyOrgResponseDto(
    Guid Id,
    string Name,
    string VerificationStatus,
    DateTime? VerifiedAt,
    DateTime UpdatedAt);

internal class PagedResponseDto<T>
{
    public List<T> Data { get; set; } = [];
    public string? Cursor { get; set; }
    public bool HasMore { get; set; }
}

internal class FakeEmailService : IEmailService
{
    public Task SendVerificationApprovedAsync(string toEmail, string organizationName) => Task.CompletedTask;
    public Task SendVerificationRejectedAsync(string toEmail, string organizationName, string reason) => Task.CompletedTask;
}
