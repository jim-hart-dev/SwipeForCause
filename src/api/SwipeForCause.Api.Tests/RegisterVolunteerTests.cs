using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwipeForCause.Api.Database;
using SwipeForCause.Api.Database.Entities;

namespace SwipeForCause.Api.Tests;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public static string? ClerkUserId { get; set; } = "user_test123";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (ClerkUserId is null)
            return Task.FromResult(AuthenticateResult.Fail("No user"));

        var claims = new[]
        {
            new Claim("sub", ClerkUserId),
            new Claim("email", "test@example.com"),
        };

        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class RegisterVolunteerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _dbName;

    public RegisterVolunteerTests(WebApplicationFactory<Program> factory)
    {
        _dbName = Guid.NewGuid().ToString();
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Remove existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add in-memory database
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(_dbName));

                // Replace auth with test handler
                services.AddAuthentication("TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });
            });
        });

        // Reset auth state for each test
        TestAuthHandler.ClerkUserId = "user_test123";
    }

    public void Dispose()
    {
        // Reset static state
        TestAuthHandler.ClerkUserId = "user_test123";
    }

    private HttpClient CreateClient() => _factory.CreateClient();

    private async Task SeedCategoryAsync(Guid categoryId, string name, string slug)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Categories.Add(new Category
        {
            Id = categoryId,
            Name = name,
            Slug = slug,
            Icon = "test",
            DisplayOrder = 1,
            IsActive = true,
        });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task RegisterVolunteer_ValidRequest_Returns201()
    {
        var client = CreateClient();
        var request = new
        {
            DisplayName = "Jane Doe",
            City = "Portland",
            State = "OR",
            CategoryIds = Array.Empty<Guid>(),
        };

        var response = await client.PostAsJsonAsync("/api/v1/volunteers", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<RegisterVolunteerResponseDto>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body!.VolunteerId);
        Assert.Equal("Jane Doe", body.DisplayName);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task RegisterVolunteer_WithCategories_CreatesCategoryRecords()
    {
        var categoryId = Guid.NewGuid();
        await SeedCategoryAsync(categoryId, "TestCat-" + categoryId.ToString()[..8], "test-cat-" + categoryId.ToString()[..8]);

        TestAuthHandler.ClerkUserId = "user_withcats_" + Guid.NewGuid().ToString()[..8];
        var client = CreateClient();
        var request = new
        {
            DisplayName = "Cat Lover",
            City = "Seattle",
            State = "WA",
            CategoryIds = new[] { categoryId },
        };

        var response = await client.PostAsJsonAsync("/api/v1/volunteers", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var body = await response.Content.ReadFromJsonAsync<RegisterVolunteerResponseDto>();
        var volunteerCategories = await db.VolunteerCategories
            .Where(vc => vc.VolunteerId == body!.VolunteerId)
            .ToListAsync();

        Assert.Single(volunteerCategories);
        Assert.Equal(categoryId, volunteerCategories[0].CategoryId);
    }

    [Fact]
    public async Task RegisterVolunteer_MissingDisplayName_Returns400()
    {
        var client = CreateClient();
        var request = new
        {
            DisplayName = "",
            City = "Portland",
            State = "OR",
            CategoryIds = Array.Empty<Guid>(),
        };

        var response = await client.PostAsJsonAsync("/api/v1/volunteers", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterVolunteer_MissingCity_Returns400()
    {
        var client = CreateClient();
        var request = new
        {
            DisplayName = "Jane Doe",
            City = "",
            State = "OR",
            CategoryIds = Array.Empty<Guid>(),
        };

        var response = await client.PostAsJsonAsync("/api/v1/volunteers", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterVolunteer_MissingState_Returns400()
    {
        var client = CreateClient();
        var request = new
        {
            DisplayName = "Jane Doe",
            City = "Portland",
            State = "",
            CategoryIds = Array.Empty<Guid>(),
        };

        var response = await client.PostAsJsonAsync("/api/v1/volunteers", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterVolunteer_DuplicateClerkId_Returns409()
    {
        TestAuthHandler.ClerkUserId = "user_duplicate_" + Guid.NewGuid().ToString()[..8];
        var client = CreateClient();
        var request = new
        {
            DisplayName = "Jane Doe",
            City = "Portland",
            State = "OR",
            CategoryIds = Array.Empty<Guid>(),
        };

        var first = await client.PostAsJsonAsync("/api/v1/volunteers", request);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await client.PostAsJsonAsync("/api/v1/volunteers", request);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task RegisterVolunteer_InvalidCategoryId_Returns400()
    {
        TestAuthHandler.ClerkUserId = "user_badcat_" + Guid.NewGuid().ToString()[..8];
        var client = CreateClient();
        var request = new
        {
            DisplayName = "Jane Doe",
            City = "Portland",
            State = "OR",
            CategoryIds = new[] { Guid.NewGuid() },
        };

        var response = await client.PostAsJsonAsync("/api/v1/volunteers", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterVolunteer_NoAuth_Returns401()
    {
        TestAuthHandler.ClerkUserId = null;
        var client = CreateClient();
        var request = new
        {
            DisplayName = "Jane Doe",
            City = "Portland",
            State = "OR",
            CategoryIds = Array.Empty<Guid>(),
        };

        var response = await client.PostAsJsonAsync("/api/v1/volunteers", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegisterVolunteer_XssInDisplayName_SanitizesOutput()
    {
        TestAuthHandler.ClerkUserId = "user_xss_" + Guid.NewGuid().ToString()[..8];
        var client = CreateClient();
        var request = new
        {
            DisplayName = "<script>alert('xss')</script>Jane",
            City = "Portland",
            State = "OR",
            CategoryIds = Array.Empty<Guid>(),
        };

        var response = await client.PostAsJsonAsync("/api/v1/volunteers", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<RegisterVolunteerResponseDto>();
        Assert.DoesNotContain("<script>", body!.DisplayName);
        Assert.Contains("Jane", body.DisplayName);
    }
}

// DTO for deserializing responses in tests
internal record RegisterVolunteerResponseDto(Guid VolunteerId, string DisplayName);
