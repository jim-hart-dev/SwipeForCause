using System.Security.Claims;
using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ScrollForCause.Api.Common;
using ScrollForCause.Api.Database;
using ScrollForCause.Api.Database.Entities;
using ScrollForCause.Api.Infrastructure.Auth;

namespace ScrollForCause.Api.Features.Volunteers;

public record RegisterVolunteerRequest(string DisplayName, string City, string State, List<Guid> CategoryIds);
public record RegisterVolunteerResponse(Guid VolunteerId, string DisplayName);

public class RegisterVolunteerValidator : AbstractValidator<RegisterVolunteerRequest>
{
    public RegisterVolunteerValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.State).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CategoryIds).Must(ids => ids.Count <= 10)
            .WithMessage("A maximum of 10 categories are allowed.");
    }
}

public static partial class RegisterVolunteer
{
    [GeneratedRegex("<[^>]*>")]
    private static partial Regex HtmlTagRegex();

    public static void MapRegisterVolunteer(this WebApplication app)
    {
        app.MapPost("/api/v1/volunteers", async (
            RegisterVolunteerRequest request,
            IValidator<RegisterVolunteerRequest> validator,
            ClaimsPrincipal user,
            AppDbContext db) =>
        {
            // Validate request
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "VALIDATION_ERROR",
                        Message = "Validation failed.",
                        Details = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }),
                    },
                });
            }

            // Extract Clerk user ID
            var currentUser = user.ToCurrentUser();

            // Check duplicate
            var exists = await db.Volunteers.AnyAsync(v => v.ClerkUserId == currentUser.ClerkUserId);
            if (exists)
            {
                return Results.Conflict(new ErrorResponse
                {
                    Error = new ErrorDetail
                    {
                        Code = "DUPLICATE_VOLUNTEER",
                        Message = "A volunteer profile already exists for this user.",
                    },
                });
            }

            // Validate category IDs
            var categoryIds = request.CategoryIds.Distinct().ToList();
            if (categoryIds.Count > 0)
            {
                var validCategoryIds = await db.Categories
                    .Where(c => categoryIds.Contains(c.Id) && c.IsActive)
                    .Select(c => c.Id)
                    .ToListAsync();

                if (validCategoryIds.Count != categoryIds.Count)
                {
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = new ErrorDetail
                        {
                            Code = "INVALID_CATEGORY",
                            Message = "One or more category IDs are invalid.",
                        },
                    });
                }
            }

            // Sanitize user input
            static string Sanitize(string input) =>
                HtmlTagRegex().Replace(input, string.Empty).Trim();

            // Create volunteer
            var volunteer = new Volunteer
            {
                Id = Guid.NewGuid(),
                ClerkUserId = currentUser.ClerkUserId,
                Email = user.FindFirstValue("email") ?? string.Empty,
                DisplayName = Sanitize(request.DisplayName),
                City = Sanitize(request.City),
                State = Sanitize(request.State),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            db.Volunteers.Add(volunteer);

            // Create volunteer category records
            foreach (var categoryId in categoryIds)
            {
                db.VolunteerCategories.Add(new VolunteerCategory
                {
                    VolunteerId = volunteer.Id,
                    CategoryId = categoryId,
                });
            }

            await db.SaveChangesAsync();

            var response = new RegisterVolunteerResponse(volunteer.Id, volunteer.DisplayName);
            return Results.Created($"/api/v1/volunteers/{volunteer.Id}", response);
        })
        .RequireAuthorization()
        .WithTags("Volunteers")
        .WithName("RegisterVolunteer");
    }
}
