namespace ScrollForCause.Api.Database.Entities;

public class VolunteerCategory
{
    public Guid VolunteerId { get; set; }
    public Volunteer Volunteer { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
