using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ScrollForCause.Api.Common;
using ScrollForCause.Api.Database;
using ScrollForCause.Api.Features.Auth;
using ScrollForCause.Api.Features.Categories;
using ScrollForCause.Api.Features.Moderation;
using ScrollForCause.Api.Features.Opportunities;
using ScrollForCause.Api.Features.Organizations;
using ScrollForCause.Api.Features.Volunteers;
using ScrollForCause.Api.Infrastructure.Auth;
using ScrollForCause.Api.Infrastructure.Email;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Authentication & Authorization
builder.Services.AddClerkAuth(builder.Configuration);

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Enter your Clerk JWT token",
    });
    options.AddSecurityRequirement(document =>
        new Microsoft.OpenApi.OpenApiSecurityRequirement
        {
            [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});

// CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Email
builder.Services.AddSingleton<IEmailService, SendGridEmailService>();

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Global exception handling
app.UseMiddleware<GlobalExceptionHandler>();

// Swagger (development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapHealthChecks("/health");
app.MapGetMe();
app.MapRegisterVolunteer();
app.MapGetCategories();
app.MapListOrganizations();
app.MapGetOrganizationDetail();
app.MapVerifyOrganization();
app.MapGetOrganizationDashboard();
app.MapCreateOpportunity();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
