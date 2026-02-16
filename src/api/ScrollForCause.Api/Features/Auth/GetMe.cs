using System.Security.Claims;
using ScrollForCause.Api.Infrastructure.Auth;

namespace ScrollForCause.Api.Features.Auth;

public record GetMeResponse(string ClerkUserId, string? UserType, Guid? ProfileId);

public static class GetMe
{
    public static void MapGetMe(this WebApplication app)
    {
        app.MapGet("/api/v1/auth/me", (ClaimsPrincipal user) =>
        {
            var currentUser = user.ToCurrentUser();
            return Results.Ok(new GetMeResponse(
                currentUser.ClerkUserId,
                currentUser.UserType,
                currentUser.ProfileId));
        })
        .RequireAuthorization()
        .WithTags("Auth")
        .WithName("GetMe");
    }
}
