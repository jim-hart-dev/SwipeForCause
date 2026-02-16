namespace ScrollForCause.Api.Database.Entities;

public class Post
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public Guid? OpportunityId { get; set; }
    public Opportunity? Opportunity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string MediaType { get; set; } = "image";
    public string Status { get; set; } = "active";
    public int ViewCount { get; set; }
    public int SaveCount { get; set; }
    public int InterestCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<PostMedia> Media { get; set; } = [];
    public List<PostTag> Tags { get; set; } = [];
    public List<SavedPost> SavedByVolunteers { get; set; } = [];
    public List<FeedView> FeedViews { get; set; } = [];
}
