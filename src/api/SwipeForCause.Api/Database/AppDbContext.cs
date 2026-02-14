using Microsoft.EntityFrameworkCore;
using SwipeForCause.Api.Database.Entities;

namespace SwipeForCause.Api.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Volunteer> Volunteers => Set<Volunteer>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<OrganizationCategory> OrganizationCategories => Set<OrganizationCategory>();
    public DbSet<VolunteerCategory> VolunteerCategories => Set<VolunteerCategory>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostMedia> PostMedia => Set<PostMedia>();
    public DbSet<PostTag> PostTags => Set<PostTag>();
    public DbSet<VolunteerInterest> VolunteerInterests => Set<VolunteerInterest>();
    public DbSet<SavedPost> SavedPosts => Set<SavedPost>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<ContentReport> ContentReports => Set<ContentReport>();
    public DbSet<NotificationSetting> NotificationSettings => Set<NotificationSetting>();
    public DbSet<FeedView> FeedViews => Set<FeedView>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureVolunteer(modelBuilder);
        ConfigureOrganization(modelBuilder);
        ConfigureCategory(modelBuilder);
        ConfigureOrganizationCategory(modelBuilder);
        ConfigureVolunteerCategory(modelBuilder);
        ConfigureOpportunity(modelBuilder);
        ConfigurePost(modelBuilder);
        ConfigurePostMedia(modelBuilder);
        ConfigurePostTag(modelBuilder);
        ConfigureVolunteerInterest(modelBuilder);
        ConfigureSavedPost(modelBuilder);
        ConfigureFollow(modelBuilder);
        ConfigureContentReport(modelBuilder);
        ConfigureNotificationSetting(modelBuilder);
        ConfigureFeedView(modelBuilder);
        SeedCategories(modelBuilder);
    }

    private static void ConfigureVolunteer(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Volunteer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClerkUserId).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Bio).HasMaxLength(500);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.Latitude).HasPrecision(10, 7);
            entity.Property(e => e.Longitude).HasPrecision(10, 7);

            entity.HasIndex(e => e.ClerkUserId).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => new { e.City, e.State });
        });
    }

    private static void ConfigureOrganization(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClerkUserId).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Ein).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.ContactName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ContactEmail).HasMaxLength(255).IsRequired();
            entity.Property(e => e.WebsiteUrl).HasMaxLength(500);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.CoverImageUrl).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.Latitude).HasPrecision(10, 7);
            entity.Property(e => e.Longitude).HasPrecision(10, 7);
            entity.Property(e => e.VerificationStatus).HasMaxLength(20).IsRequired();

            entity.HasIndex(e => e.ClerkUserId).IsUnique();
            entity.HasIndex(e => e.Ein);
            entity.HasIndex(e => e.VerificationStatus);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => new { e.City, e.State });
        });
    }

    private static void ConfigureCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Icon).HasMaxLength(50);

            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.DisplayOrder);
        });
    }

    private static void ConfigureOrganizationCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrganizationCategory>(entity =>
        {
            entity.HasKey(e => new { e.OrganizationId, e.CategoryId });

            entity.HasOne(e => e.Organization)
                .WithMany(o => o.OrganizationCategories)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.OrganizationCategories)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureVolunteerCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VolunteerCategory>(entity =>
        {
            entity.HasKey(e => new { e.VolunteerId, e.CategoryId });

            entity.HasOne(e => e.Volunteer)
                .WithMany(v => v.VolunteerCategories)
                .HasForeignKey(e => e.VolunteerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.VolunteerCategories)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureOpportunity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Opportunity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(5000).IsRequired();
            entity.Property(e => e.LocationAddress).HasMaxLength(500);
            entity.Property(e => e.Latitude).HasPrecision(10, 7);
            entity.Property(e => e.Longitude).HasPrecision(10, 7);
            entity.Property(e => e.ScheduleType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.RecurrenceDesc).HasMaxLength(500);
            entity.Property(e => e.TimeCommitment).HasMaxLength(100);
            entity.Property(e => e.SkillsRequired).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();

            entity.HasOne(e => e.Organization)
                .WithMany(o => o.Opportunities)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.OrganizationId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsRemote);
            entity.HasIndex(e => e.StartDate);
            entity.HasIndex(e => new { e.Status, e.CreatedAt });
        });
    }

    private static void ConfigurePost(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.MediaType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();

            entity.HasOne(e => e.Organization)
                .WithMany(o => o.Posts)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Opportunity)
                .WithMany(o => o.Posts)
                .HasForeignKey(e => e.OpportunityId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.OrganizationId);
            entity.HasIndex(e => e.OpportunityId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.Status, e.CreatedAt });
        });
    }

    private static void ConfigurePostMedia(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PostMedia>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MediaUrl).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);
            entity.Property(e => e.OriginalUrl).HasMaxLength(500);
            entity.Property(e => e.LowResUrl).HasMaxLength(500);
            entity.Property(e => e.MediaType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.ProcessingStatus).HasMaxLength(20).IsRequired();

            entity.HasOne(e => e.Post)
                .WithMany(p => p.Media)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.PostId);
            entity.HasIndex(e => e.ProcessingStatus);
        });
    }

    private static void ConfigurePostTag(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PostTag>(entity =>
        {
            entity.HasKey(e => new { e.PostId, e.Tag });
            entity.Property(e => e.Tag).HasMaxLength(100).IsRequired();

            entity.HasOne(e => e.Post)
                .WithMany(p => p.Tags)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.Tag);
        });
    }

    private static void ConfigureVolunteerInterest(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VolunteerInterest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Message).HasMaxLength(1000);
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();

            entity.HasOne(e => e.Volunteer)
                .WithMany(v => v.VolunteerInterests)
                .HasForeignKey(e => e.VolunteerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Opportunity)
                .WithMany(o => o.VolunteerInterests)
                .HasForeignKey(e => e.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Post)
                .WithMany()
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.VolunteerId, e.OpportunityId }).IsUnique();
            entity.HasIndex(e => e.OpportunityId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    private static void ConfigureSavedPost(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SavedPost>(entity =>
        {
            entity.HasKey(e => new { e.VolunteerId, e.PostId });

            entity.HasOne(e => e.Volunteer)
                .WithMany(v => v.SavedPosts)
                .HasForeignKey(e => e.VolunteerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Post)
                .WithMany(p => p.SavedByVolunteers)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.PostId);
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    private static void ConfigureFollow(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Follow>(entity =>
        {
            entity.HasKey(e => new { e.VolunteerId, e.OrganizationId });

            entity.HasOne(e => e.Volunteer)
                .WithMany(v => v.Follows)
                .HasForeignKey(e => e.VolunteerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Organization)
                .WithMany(o => o.Followers)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.OrganizationId);
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    private static void ConfigureContentReport(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContentReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReporterType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.ContentType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Reason).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.ReviewedBy).HasMaxLength(255);
            entity.Property(e => e.ActionTaken).HasMaxLength(500);

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.ContentType, e.ContentId });
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    private static void ConfigureNotificationSetting(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationSetting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.NewInterestEmail).HasMaxLength(20).IsRequired();
            entity.Property(e => e.NewContentDigest).HasMaxLength(20).IsRequired();

            entity.HasIndex(e => new { e.UserId, e.UserType }).IsUnique();
        });
    }

    private static void ConfigureFeedView(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FeedView>(entity =>
        {
            entity.HasKey(e => new { e.VolunteerId, e.PostId });

            entity.HasOne(e => e.Volunteer)
                .WithMany(v => v.FeedViews)
                .HasForeignKey(e => e.VolunteerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Post)
                .WithMany(p => p.FeedViews)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.PostId);
            entity.HasIndex(e => e.ViewedAt);
        });
    }

    private static void SeedCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = new Guid("a1b2c3d4-0001-4000-8000-000000000001"), Name = "Environment", Slug = "environment", Icon = "leaf", DisplayOrder = 1, IsActive = true },
            new Category { Id = new Guid("a1b2c3d4-0002-4000-8000-000000000002"), Name = "Education", Slug = "education", Icon = "book", DisplayOrder = 2, IsActive = true },
            new Category { Id = new Guid("a1b2c3d4-0003-4000-8000-000000000003"), Name = "Health", Slug = "health", Icon = "heart", DisplayOrder = 3, IsActive = true },
            new Category { Id = new Guid("a1b2c3d4-0004-4000-8000-000000000004"), Name = "Animals", Slug = "animals", Icon = "paw", DisplayOrder = 4, IsActive = true },
            new Category { Id = new Guid("a1b2c3d4-0005-4000-8000-000000000005"), Name = "Seniors", Slug = "seniors", Icon = "users", DisplayOrder = 5, IsActive = true },
            new Category { Id = new Guid("a1b2c3d4-0006-4000-8000-000000000006"), Name = "Youth", Slug = "youth", Icon = "star", DisplayOrder = 6, IsActive = true },
            new Category { Id = new Guid("a1b2c3d4-0007-4000-8000-000000000007"), Name = "Disaster Relief", Slug = "disaster-relief", Icon = "shield", DisplayOrder = 7, IsActive = true },
            new Category { Id = new Guid("a1b2c3d4-0008-4000-8000-000000000008"), Name = "Arts & Culture", Slug = "arts-culture", Icon = "palette", DisplayOrder = 8, IsActive = true },
            new Category { Id = new Guid("a1b2c3d4-0009-4000-8000-000000000009"), Name = "Food Security", Slug = "food-security", Icon = "utensils", DisplayOrder = 9, IsActive = true },
            new Category { Id = new Guid("a1b2c3d4-0010-4000-8000-000000000010"), Name = "Housing", Slug = "housing", Icon = "home", DisplayOrder = 10, IsActive = true }
        );
    }
}
