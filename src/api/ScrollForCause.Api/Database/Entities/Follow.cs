namespace ScrollForCause.Api.Database.Entities;

public class Follow
{
    public Guid VolunteerId { get; set; }
    public Volunteer Volunteer { get; set; } = null!;
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
