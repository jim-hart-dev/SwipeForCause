namespace ScrollForCause.Api.Database.Entities;

public class Organization
{
    public Guid Id { get; set; }
    public string ClerkUserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Ein { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? WebsiteUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string VerificationStatus { get; set; } = "pending";
    public DateTime? VerifiedAt { get; set; }
    public int FollowerCount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public List<OrganizationCategory> OrganizationCategories { get; set; } = [];
    public List<Post> Posts { get; set; } = [];
    public List<Opportunity> Opportunities { get; set; } = [];
    public List<Follow> Followers { get; set; } = [];
}
