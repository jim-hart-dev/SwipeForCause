using System.Security.Claims;

namespace ScrollForCause.Api.Infrastructure.Auth;

public record CurrentUser(string ClerkUserId, string? UserType, Guid? ProfileId);

public static class ClaimsPrincipalExtensions
{
    public static CurrentUser ToCurrentUser(this ClaimsPrincipal principal)
    {
        var clerkUserId = principal.FindFirstValue("sub")
            ?? throw new InvalidOperationException("Missing sub claim.");

        var userType = principal.FindFirstValue("user_type");

        var profileIdClaim = principal.FindFirstValue("profile_id");
        var profileId = Guid.TryParse(profileIdClaim, out var parsed) ? parsed : (Guid?)null;

        return new CurrentUser(clerkUserId, userType, profileId);
    }
}
