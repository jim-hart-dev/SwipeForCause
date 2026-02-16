namespace ScrollForCause.Api.Database.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public List<OrganizationCategory> OrganizationCategories { get; set; } = [];
    public List<VolunteerCategory> VolunteerCategories { get; set; } = [];
}
