using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace ScrollForCause.Api.Infrastructure.Auth;

public static class ClerkAuthExtensions
{
    public static IServiceCollection AddClerkAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var authority = configuration["Clerk:Authority"]
            ?? throw new InvalidOperationException("Clerk:Authority is not configured.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                // Clerk session tokens don't include an aud claim by default,
                // so audience validation would reject all tokens.
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    NameClaimType = "sub",
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("Volunteer", policy =>
                policy.RequireClaim("user_type", "volunteer"))
            .AddPolicy("Organization", policy =>
                policy.RequireClaim("user_type", "organization"))
            .AddPolicy("Admin", policy =>
                policy.RequireClaim("user_type", "admin"));

        return services;
    }
}
