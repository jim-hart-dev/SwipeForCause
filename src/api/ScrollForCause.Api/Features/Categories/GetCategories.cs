using Microsoft.EntityFrameworkCore;
using ScrollForCause.Api.Database;

namespace ScrollForCause.Api.Features.Categories;

public record GetCategoriesResponse(Guid CategoryId, string Name, string Slug, string? Icon);

public static class GetCategories
{
    public static void MapGetCategories(this WebApplication app)
    {
        app.MapGet("/api/v1/categories", async (AppDbContext db) =>
        {
            var categories = await db.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new GetCategoriesResponse(c.Id, c.Name, c.Slug, c.Icon))
                .ToListAsync();

            return Results.Ok(categories);
        })
        .AllowAnonymous()
        .WithTags("Categories")
        .WithName("GetCategories");
    }
}
