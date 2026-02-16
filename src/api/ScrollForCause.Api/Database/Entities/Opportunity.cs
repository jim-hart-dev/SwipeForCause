namespace ScrollForCause.Api.Database.Entities;

public class Opportunity
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? LocationAddress { get; set; }
    public bool IsRemote { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string ScheduleType { get; set; } = "flexible";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? RecurrenceDesc { get; set; }
    public int? VolunteersNeeded { get; set; }
    public string? TimeCommitment { get; set; }
    public string? SkillsRequired { get; set; }
    public int? MinimumAge { get; set; }
    public string Status { get; set; } = "active";
    public int InterestCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<Post> Posts { get; set; } = [];
    public List<VolunteerInterest> VolunteerInterests { get; set; } = [];
}
