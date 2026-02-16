using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SwipeForCause.Api.Database;
using SwipeForCause.Api.Database.Entities;

namespace SwipeForCause.Api.Tests;

public class GetCategoriesTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public GetCategoriesTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Remove existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add in-memory database with a fixed name so all scopes share the same store
                var dbName = Guid.NewGuid().ToString();
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));

                // Replace auth with test handler
                services.AddAuthentication("TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });
            });
        });

        // Seed the 10 categories into the in-memory database
        SeedCategories();
    }

    private void SeedCategories()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (db.Categories.Any())
            return;

        db.Categories.AddRange(
            new Category { Id = new Guid("a1b2c3d4-0001-4000-8000-000000000001"), Name = "Environment", Slug = "environment", Icon = "leaf", DisplayOrder = 1, IsActive = true },
            new Category { Id = new Guid("a1b2c3d4-0002-4000-8000-000000000002"), Name = "Education", Slug = "education", Icon = "book", DisplayOrder = 2, IsActive = true },
            new Category { Id = new Guid("a1b2c3d4-0003-4000-8000-000000000003"), Name = "Health", Slug = "health", Icon = "heart", DisplayOrder = 3, IsActive = true },
            new Category { Id = new Guid("a1b2c3d4-0004-4000-8000-000000000004"), Name = "Animals", Slug = "animals", Icon = "paw", DisplayOrder = 4, IsActive = true },
            new Category { Id = new Guid("a1b2c3d4-0005-4000-8000-000000000005"), Name = "Seniors", Slug = "seniors", Icon = "users", DisplayOrder = 5, IsActive = true },
            new Category { Id = new Guid("a1b2c3d4-0006-4000-8000-000000000006"), Name = "Youth", Slug = "youth", Icon = "star", DisplayOrder = 6, IsActive = true },
            new Category { Id = new Guid("a1b2c3d4-0007-4000-8000-000000000007"), Name = "Disaster Relief", Slug = "disaster-relief", Icon = "shield", DisplayOrder = 7, IsActive = true },
            new Category { Id = new Guid("a1b2c3d4-0008-4000-8000-000000000008"), Name = "Arts & Culture", Slug = "arts-culture", Icon = "palette", DisplayOrder = 8, IsActive = true },
            new Category { Id = new Guid("a1b2c3d4-0009-4000-8000-000000000009"), Name = "Food Security", Slug = "food-security", Icon = "utensils", DisplayOrder = 9, IsActive = true },
            new Category { Id = new Guid("a1b2c3d4-0010-4000-8000-000000000010"), Name = "Housing", Slug = "housing", Icon = "home", DisplayOrder = 10, IsActive = true }
        );
        db.SaveChanges();
    }

    private HttpClient CreateClient() => _factory.CreateClient();

    [Fact]
    public async Task GetCategories_ReturnsSeededCategories()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/v1/categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var categories = await response.Content.ReadFromJsonAsync<List<CategoryResponse>>();
        Assert.NotNull(categories);
        Assert.Equal(10, categories!.Count);
        Assert.Contains(categories, c => c.Name == "Environment");
        Assert.Contains(categories, c => c.Name == "Housing");
    }

    [Fact]
    public async Task GetCategories_ReturnsCategoriesOrderedByDisplayOrder()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/v1/categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var categories = await response.Content.ReadFromJsonAsync<List<CategoryResponse>>();
        Assert.NotNull(categories);
        Assert.Equal("Environment", categories!.First().Name);
        Assert.Equal("Housing", categories!.Last().Name);
    }
}

// DTO for deserializing responses in tests
internal record CategoryResponse(Guid CategoryId, string Name, string Slug, string? Icon);
