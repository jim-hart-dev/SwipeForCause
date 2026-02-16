namespace ScrollForCause.Api.Database.Entities;

public class VolunteerInterest
{
    public Guid Id { get; set; }
    public Guid VolunteerId { get; set; }
    public Volunteer Volunteer { get; set; } = null!;
    public Guid OpportunityId { get; set; }
    public Opportunity Opportunity { get; set; } = null!;
    public Guid? PostId { get; set; }
    public Post? Post { get; set; }
    public string? Message { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime? StatusUpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
