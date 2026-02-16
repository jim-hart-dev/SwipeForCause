namespace ScrollForCause.Api.Database.Entities;

public class OrganizationCategory
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
