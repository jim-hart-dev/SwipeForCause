using System.Security.Claims;
using ScrollForCause.Api.Infrastructure.Auth;

namespace ScrollForCause.Api.Tests;

public class CurrentUserTests
{
    [Fact]
    public void ToCurrentUser_WithAllClaims_ReturnsPopulatedCurrentUser()
    {
        var profileId = Guid.NewGuid();
        var principal = CreatePrincipal(
            ("sub", "user_abc123"),
            ("user_type", "volunteer"),
            ("profile_id", profileId.ToString()));

        var result = principal.ToCurrentUser();

        Assert.Equal("user_abc123", result.ClerkUserId);
        Assert.Equal("volunteer", result.UserType);
        Assert.Equal(profileId, result.ProfileId);
    }

    [Fact]
    public void ToCurrentUser_WithOnlySub_ReturnsNullOptionalFields()
    {
        var principal = CreatePrincipal(("sub", "user_abc123"));

        var result = principal.ToCurrentUser();

        Assert.Equal("user_abc123", result.ClerkUserId);
        Assert.Null(result.UserType);
        Assert.Null(result.ProfileId);
    }

    [Fact]
    public void ToCurrentUser_WithInvalidProfileId_ReturnsNullProfileId()
    {
        var principal = CreatePrincipal(
            ("sub", "user_abc123"),
            ("profile_id", "not-a-guid"));

        var result = principal.ToCurrentUser();

        Assert.Equal("user_abc123", result.ClerkUserId);
        Assert.Null(result.ProfileId);
    }

    [Fact]
    public void ToCurrentUser_WithoutSub_Throws()
    {
        var principal = CreatePrincipal(("user_type", "volunteer"));

        Assert.Throws<InvalidOperationException>(() => principal.ToCurrentUser());
    }

    private static ClaimsPrincipal CreatePrincipal(params (string Type, string Value)[] claims)
    {
        var identity = new ClaimsIdentity(
            claims.Select(c => new Claim(c.Type, c.Value)),
            "Test");
        return new ClaimsPrincipal(identity);
    }
}
