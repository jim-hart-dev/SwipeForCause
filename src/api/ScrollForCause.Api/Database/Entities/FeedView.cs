namespace ScrollForCause.Api.Database.Entities;

public class FeedView
{
    public Guid VolunteerId { get; set; }
    public Volunteer Volunteer { get; set; } = null!;
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
}
