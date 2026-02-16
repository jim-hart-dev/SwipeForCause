namespace ScrollForCause.Api.Database.Entities;

public class Volunteer
{
    public Guid Id { get; set; }
    public string ClerkUserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public List<VolunteerCategory> VolunteerCategories { get; set; } = [];
    public List<VolunteerInterest> VolunteerInterests { get; set; } = [];
    public List<SavedPost> SavedPosts { get; set; } = [];
    public List<Follow> Follows { get; set; } = [];
    public List<FeedView> FeedViews { get; set; } = [];
}
