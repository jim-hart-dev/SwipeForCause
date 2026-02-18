using Microsoft.EntityFrameworkCore;
using ScrollForCause.Api.Database;
using ScrollForCause.Api.Database.Entities;

namespace ScrollForCause.Api.Features.Seed;

public static class SeedData
{
    // Category GUIDs (must match AppDbContext.SeedCategories)
    private static readonly Guid EnvironmentCatId = new("a1b2c3d4-0001-4000-8000-000000000001");
    private static readonly Guid EducationCatId = new("a1b2c3d4-0002-4000-8000-000000000002");
    private static readonly Guid HealthCatId = new("a1b2c3d4-0003-4000-8000-000000000003");
    private static readonly Guid AnimalsCatId = new("a1b2c3d4-0004-4000-8000-000000000004");
    private static readonly Guid SeniorsCatId = new("a1b2c3d4-0005-4000-8000-000000000005");
    private static readonly Guid YouthCatId = new("a1b2c3d4-0006-4000-8000-000000000006");
    private static readonly Guid DisasterReliefCatId = new("a1b2c3d4-0007-4000-8000-000000000007");
    private static readonly Guid ArtsCultureCatId = new("a1b2c3d4-0008-4000-8000-000000000008");
    private static readonly Guid FoodSecurityCatId = new("a1b2c3d4-0009-4000-8000-000000000009");
    private static readonly Guid HousingCatId = new("a1b2c3d4-0010-4000-8000-000000000010");

    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Skip seeding if migrations haven't been applied yet
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
            return;

        if (await context.Organizations.AnyAsync(o => o.ClerkUserId.StartsWith("seed_")))
            return;

        var now = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        var organizations = CreateOrganizations(now);
        var orgCategories = CreateOrganizationCategories();
        var opportunities = CreateOpportunities(organizations, now);
        var posts = CreatePosts(organizations, opportunities, now);
        var postMedia = CreatePostMedia(posts, now);
        var postTags = CreatePostTags(posts);

        context.Organizations.AddRange(organizations);
        context.OrganizationCategories.AddRange(orgCategories);
        context.Opportunities.AddRange(opportunities);
        context.Posts.AddRange(posts);
        context.PostMedia.AddRange(postMedia);
        context.PostTags.AddRange(postTags);

        await context.SaveChangesAsync();
    }

    private static Guid OrgId(int n) => new($"00000054-0001-4000-8000-{n:D12}");
    private static Guid OppId(int orgN, int oppN) => new($"00000054-0002-4000-8000-00000{orgN:D4}{oppN:D3}");
    private static Guid PostId(int orgN, int postN) => new($"00000054-0003-4000-8000-00000{orgN:D4}{postN:D3}");
    private static Guid MediaId(int orgN, int postN) => new($"00000054-0004-4000-8000-00000{orgN:D4}{postN:D3}");

    private static List<Organization> CreateOrganizations(DateTime now)
    {
        return
        [
            new Organization
            {
                Id = OrgId(1), ClerkUserId = "seed_org_ocean_guardians", Name = "Ocean Guardians",
                Ein = "12-3456781", Description = "Ocean Guardians mobilizes coastal communities to protect marine ecosystems through beach cleanups, coral reef restoration, and ocean education programs. Our volunteers have removed over 50,000 pounds of debris from shorelines.",
                ContactName = "Maria Santos", ContactEmail = "maria@oceanguardians.org", WebsiteUrl = "https://oceanguardians.org",
                City = "San Diego", State = "CA", Latitude = 32.7157m, Longitude = -117.1611m,
                VerificationStatus = "verified", VerifiedAt = now.AddDays(-30), IsActive = true, CreatedAt = now.AddDays(-90), UpdatedAt = now.AddDays(-30)
            },
            new Organization
            {
                Id = OrgId(2), ClerkUserId = "seed_org_code_for_tomorrow", Name = "Code for Tomorrow",
                Ein = "12-3456782", Description = "Code for Tomorrow bridges the digital divide by providing free coding bootcamps and mentorship to underserved youth. We believe every young person deserves access to tech education regardless of their zip code.",
                ContactName = "James Chen", ContactEmail = "james@codefortomorrow.org", WebsiteUrl = "https://codefortomorrow.org",
                City = "Austin", State = "TX", Latitude = 30.2672m, Longitude = -97.7431m,
                VerificationStatus = "verified", VerifiedAt = now.AddDays(-25), IsActive = true, CreatedAt = now.AddDays(-85), UpdatedAt = now.AddDays(-25)
            },
            new Organization
            {
                Id = OrgId(3), ClerkUserId = "seed_org_healthy_hearts", Name = "Healthy Hearts Initiative",
                Ein = "12-3456783", Description = "Healthy Hearts Initiative provides free health screenings, nutrition workshops, and fitness programs in communities with limited healthcare access. Our mobile health clinics reach thousands of residents each month.",
                ContactName = "Dr. Aisha Johnson", ContactEmail = "aisha@healthyhearts.org", WebsiteUrl = "https://healthyhearts.org",
                City = "Chicago", State = "IL", Latitude = 41.8781m, Longitude = -87.6298m,
                VerificationStatus = "verified", VerifiedAt = now.AddDays(-28), IsActive = true, CreatedAt = now.AddDays(-80), UpdatedAt = now.AddDays(-28)
            },
            new Organization
            {
                Id = OrgId(4), ClerkUserId = "seed_org_paws_claws", Name = "Paws & Claws Rescue",
                Ein = "12-3456784", Description = "Paws & Claws Rescue saves animals from high-kill shelters and provides foster care, veterinary services, and adoption support. We've placed over 3,000 animals in loving homes since our founding.",
                ContactName = "Emily Park", ContactEmail = "emily@pawsandclaws.org", WebsiteUrl = "https://pawsandclaws.org",
                City = "Portland", State = "OR", Latitude = 45.5152m, Longitude = -122.6784m,
                VerificationStatus = "verified", VerifiedAt = now.AddDays(-20), IsActive = true, CreatedAt = now.AddDays(-75), UpdatedAt = now.AddDays(-20)
            },
            new Organization
            {
                Id = OrgId(5), ClerkUserId = "seed_org_silver_companions", Name = "Silver Companions",
                Ein = "12-3456785", Description = "Silver Companions combats senior isolation by pairing volunteers with elderly residents for regular visits, errand assistance, and companionship. Our programs operate in assisted living facilities and private homes alike.",
                ContactName = "Robert Williams", ContactEmail = "robert@silvercompanions.org", WebsiteUrl = "https://silvercompanions.org",
                City = "Phoenix", State = "AZ", Latitude = 33.4484m, Longitude = -112.0740m,
                VerificationStatus = "verified", VerifiedAt = now.AddDays(-22), IsActive = true, CreatedAt = now.AddDays(-70), UpdatedAt = now.AddDays(-22)
            },
            new Organization
            {
                Id = OrgId(6), ClerkUserId = "seed_org_future_leaders", Name = "Future Leaders Academy",
                Ein = "12-3456786", Description = "Future Leaders Academy empowers at-risk youth through after-school programs, leadership workshops, and summer camps. We focus on building confidence, critical thinking, and community engagement skills.",
                ContactName = "Terrence Davis", ContactEmail = "terrence@futureleaders.org", WebsiteUrl = "https://futureleaders.org",
                City = "Atlanta", State = "GA", Latitude = 33.7490m, Longitude = -84.3880m,
                VerificationStatus = "verified", VerifiedAt = now.AddDays(-18), IsActive = true, CreatedAt = now.AddDays(-65), UpdatedAt = now.AddDays(-18)
            },
            new Organization
            {
                Id = OrgId(7), ClerkUserId = "seed_org_rapid_response", Name = "Rapid Response Network",
                Ein = "12-3456787", Description = "Rapid Response Network deploys trained volunteers to disaster-stricken communities within 48 hours. We provide emergency shelter coordination, supply distribution, and long-term recovery assistance.",
                ContactName = "Captain Lisa Torres", ContactEmail = "lisa@rapidresponse.org", WebsiteUrl = "https://rapidresponse.org",
                City = "Houston", State = "TX", Latitude = 29.7604m, Longitude = -95.3698m,
                VerificationStatus = "verified", VerifiedAt = now.AddDays(-15), IsActive = true, CreatedAt = now.AddDays(-60), UpdatedAt = now.AddDays(-15)
            },
            new Organization
            {
                Id = OrgId(8), ClerkUserId = "seed_org_community_canvas", Name = "Community Canvas",
                Ein = "12-3456788", Description = "Community Canvas transforms neglected urban spaces through collaborative public art projects. Our volunteer artists create murals, sculptures, and installations that celebrate local culture and build neighborhood pride.",
                ContactName = "Sofia Rivera", ContactEmail = "sofia@communitycanvas.org", WebsiteUrl = "https://communitycanvas.org",
                City = "Denver", State = "CO", Latitude = 39.7392m, Longitude = -104.9903m,
                VerificationStatus = "verified", VerifiedAt = now.AddDays(-12), IsActive = true, CreatedAt = now.AddDays(-55), UpdatedAt = now.AddDays(-12)
            },
            new Organization
            {
                Id = OrgId(9), ClerkUserId = "seed_org_no_hunger", Name = "No Hunger Project",
                Ein = "12-3456789", Description = "No Hunger Project fights food insecurity through community gardens, mobile food pantries, and nutrition education. We rescue 10,000 pounds of surplus food weekly and redistribute it to families in need.",
                ContactName = "Marcus Brown", ContactEmail = "marcus@nohunger.org", WebsiteUrl = "https://nohunger.org",
                City = "Philadelphia", State = "PA", Latitude = 39.9526m, Longitude = -75.1652m,
                VerificationStatus = "verified", VerifiedAt = now.AddDays(-10), IsActive = true, CreatedAt = now.AddDays(-50), UpdatedAt = now.AddDays(-10)
            },
            new Organization
            {
                Id = OrgId(0), ClerkUserId = "seed_org_habitat_helpers", Name = "Habitat Helpers",
                Ein = "12-3456780", Description = "Habitat Helpers builds and repairs homes for low-income families, veterans, and disaster survivors. Our skilled volunteers tackle everything from foundation work to finishing touches, creating safe and stable housing.",
                ContactName = "David Kim", ContactEmail = "david@habitathelpers.org", WebsiteUrl = "https://habitathelpers.org",
                City = "Nashville", State = "TN", Latitude = 36.1627m, Longitude = -86.7816m,
                VerificationStatus = "verified", VerifiedAt = now.AddDays(-8), IsActive = true, CreatedAt = now.AddDays(-45), UpdatedAt = now.AddDays(-8)
            }
        ];
    }

    private static List<OrganizationCategory> CreateOrganizationCategories()
    {
        return
        [
            new OrganizationCategory { OrganizationId = OrgId(1), CategoryId = EnvironmentCatId },
            new OrganizationCategory { OrganizationId = OrgId(2), CategoryId = EducationCatId },
            new OrganizationCategory { OrganizationId = OrgId(3), CategoryId = HealthCatId },
            new OrganizationCategory { OrganizationId = OrgId(4), CategoryId = AnimalsCatId },
            new OrganizationCategory { OrganizationId = OrgId(5), CategoryId = SeniorsCatId },
            new OrganizationCategory { OrganizationId = OrgId(6), CategoryId = YouthCatId },
            new OrganizationCategory { OrganizationId = OrgId(7), CategoryId = DisasterReliefCatId },
            new OrganizationCategory { OrganizationId = OrgId(8), CategoryId = ArtsCultureCatId },
            new OrganizationCategory { OrganizationId = OrgId(9), CategoryId = FoodSecurityCatId },
            new OrganizationCategory { OrganizationId = OrgId(0), CategoryId = HousingCatId }
        ];
    }

    private static List<Opportunity> CreateOpportunities(List<Organization> orgs, DateTime now)
    {
        return
        [
            // Org 1 - Ocean Guardians
            new Opportunity
            {
                Id = OppId(1, 1), OrganizationId = OrgId(1), Title = "Saturday Beach Cleanup", Description = "Join us every Saturday morning to clean up Mission Beach. We provide all supplies — just bring sunscreen and a water bottle. All ages welcome!",
                ScheduleType = "recurring", RecurrenceDesc = "Every Saturday, 8:00 AM - 11:00 AM", TimeCommitment = "3 hours/week",
                LocationAddress = "Mission Beach, San Diego, CA", IsRemote = false, VolunteersNeeded = 30, Status = "active",
                CreatedAt = now.AddDays(-80), UpdatedAt = now.AddDays(-5)
            },
            new Opportunity
            {
                Id = OppId(1, 2), OrganizationId = OrgId(1), Title = "Marine Education Workshop Leader", Description = "Lead fun, interactive ocean science workshops for elementary school students. Training provided. Perfect for marine biology students or ocean enthusiasts.",
                ScheduleType = "flexible", TimeCommitment = "4-6 hours/month", SkillsRequired = "Teaching, public speaking",
                LocationAddress = "Birch Aquarium, La Jolla, CA", IsRemote = false, VolunteersNeeded = 10, Status = "active",
                CreatedAt = now.AddDays(-70), UpdatedAt = now.AddDays(-3)
            },
            // Org 2 - Code for Tomorrow
            new Opportunity
            {
                Id = OppId(2, 1), OrganizationId = OrgId(2), Title = "Python Bootcamp Mentor", Description = "Mentor a cohort of 10 teens through our 8-week Python fundamentals course. You'll guide exercises, review projects, and provide encouragement. No teaching degree needed — just Python experience and patience.",
                ScheduleType = "recurring", RecurrenceDesc = "Tuesdays & Thursdays, 4:00 PM - 6:00 PM", TimeCommitment = "4 hours/week",
                IsRemote = true, VolunteersNeeded = 8, SkillsRequired = "Python, mentoring", Status = "active",
                CreatedAt = now.AddDays(-75), UpdatedAt = now.AddDays(-4)
            },
            new Opportunity
            {
                Id = OppId(2, 2), OrganizationId = OrgId(2), Title = "Laptop Refurbishment Day", Description = "Help us wipe, reinstall, and configure donated laptops for students in our program. Basic hardware skills helpful but not required.",
                ScheduleType = "one_time", StartDate = now.AddDays(14), TimeCommitment = "6 hours",
                LocationAddress = "Austin Community Center, Austin, TX", IsRemote = false, VolunteersNeeded = 15, Status = "active",
                CreatedAt = now.AddDays(-60), UpdatedAt = now.AddDays(-2)
            },
            // Org 3 - Healthy Hearts Initiative
            new Opportunity
            {
                Id = OppId(3, 1), OrganizationId = OrgId(3), Title = "Mobile Clinic Assistant", Description = "Assist our medical team during free health screening events. Duties include patient check-in, blood pressure readings, and distributing educational materials.",
                ScheduleType = "recurring", RecurrenceDesc = "First and third Saturdays, 9:00 AM - 2:00 PM", TimeCommitment = "10 hours/month",
                LocationAddress = "Various locations, Chicago, IL", IsRemote = false, VolunteersNeeded = 20, Status = "active",
                CreatedAt = now.AddDays(-70), UpdatedAt = now.AddDays(-6)
            },
            new Opportunity
            {
                Id = OppId(3, 2), OrganizationId = OrgId(3), Title = "Nutrition Education Content Creator", Description = "Create engaging social media content about heart-healthy eating on a budget. Work remotely on your own schedule.",
                ScheduleType = "flexible", TimeCommitment = "5 hours/week",
                IsRemote = true, VolunteersNeeded = 5, SkillsRequired = "Graphic design, nutrition knowledge", Status = "active",
                CreatedAt = now.AddDays(-55), UpdatedAt = now.AddDays(-1)
            },
            // Org 4 - Paws & Claws Rescue
            new Opportunity
            {
                Id = OppId(4, 1), OrganizationId = OrgId(4), Title = "Foster Home Provider", Description = "Open your home to a rescue animal while we find their forever family. We cover all vet bills and food costs. Average foster period is 2-4 weeks.",
                ScheduleType = "flexible", TimeCommitment = "Ongoing (2-4 week commitments)",
                LocationAddress = "Portland Metro Area, OR", IsRemote = false, VolunteersNeeded = 50, Status = "active",
                CreatedAt = now.AddDays(-65), UpdatedAt = now.AddDays(-7)
            },
            new Opportunity
            {
                Id = OppId(4, 2), OrganizationId = OrgId(4), Title = "Adoption Event Coordinator", Description = "Help organize and run our monthly adoption fairs at local pet stores. Greet potential adopters, handle paperwork, and match families with their new best friends.",
                ScheduleType = "recurring", RecurrenceDesc = "Last Saturday of each month, 10:00 AM - 4:00 PM", TimeCommitment = "6 hours/month",
                LocationAddress = "Various pet stores, Portland, OR", IsRemote = false, VolunteersNeeded = 12, Status = "active",
                CreatedAt = now.AddDays(-50), UpdatedAt = now.AddDays(-3)
            },
            // Org 5 - Silver Companions
            new Opportunity
            {
                Id = OppId(5, 1), OrganizationId = OrgId(5), Title = "Weekly Companion Visitor", Description = "Visit a senior resident once a week for conversation, games, or a short walk. Consistent commitment helps build meaningful relationships. Background check required.",
                ScheduleType = "recurring", RecurrenceDesc = "Weekly, flexible day/time", TimeCommitment = "2 hours/week",
                LocationAddress = "Various facilities, Phoenix, AZ", IsRemote = false, VolunteersNeeded = 40, MinimumAge = 18, Status = "active",
                CreatedAt = now.AddDays(-60), UpdatedAt = now.AddDays(-5)
            },
            new Opportunity
            {
                Id = OppId(5, 2), OrganizationId = OrgId(5), Title = "Tech Help Desk for Seniors", Description = "Help seniors learn to use smartphones, tablets, and video calling so they can stay connected with family. Patience and clear communication are key!",
                ScheduleType = "recurring", RecurrenceDesc = "Wednesdays, 1:00 PM - 3:00 PM", TimeCommitment = "2 hours/week",
                LocationAddress = "Phoenix Senior Center, Phoenix, AZ", IsRemote = false, VolunteersNeeded = 8, Status = "active",
                CreatedAt = now.AddDays(-45), UpdatedAt = now.AddDays(-2)
            },
            // Org 6 - Future Leaders Academy
            new Opportunity
            {
                Id = OppId(6, 1), OrganizationId = OrgId(6), Title = "After-School Homework Tutor", Description = "Help middle schoolers with homework and study skills in our after-school program. Subjects include math, English, and science. Training provided.",
                ScheduleType = "recurring", RecurrenceDesc = "Mon-Thu, 3:30 PM - 5:30 PM (pick your days)", TimeCommitment = "2-8 hours/week",
                LocationAddress = "Future Leaders Center, Atlanta, GA", IsRemote = false, VolunteersNeeded = 25, Status = "active",
                CreatedAt = now.AddDays(-55), UpdatedAt = now.AddDays(-4)
            },
            new Opportunity
            {
                Id = OppId(6, 2), OrganizationId = OrgId(6), Title = "Summer Camp Counselor", Description = "Lead activities and mentor a group of 8-10 teens during our two-week summer leadership camp. Includes outdoor adventure, public speaking, and community service projects.",
                ScheduleType = "one_time", StartDate = now.AddDays(45), EndDate = now.AddDays(59), TimeCommitment = "Full-time, 2 weeks",
                LocationAddress = "Camp Horizon, Blue Ridge, GA", IsRemote = false, VolunteersNeeded = 15, MinimumAge = 21, Status = "active",
                CreatedAt = now.AddDays(-40), UpdatedAt = now.AddDays(-1)
            },
            // Org 7 - Rapid Response Network
            new Opportunity
            {
                Id = OppId(7, 1), OrganizationId = OrgId(7), Title = "Disaster Response Volunteer", Description = "Join our trained rapid-response team. After a free 16-hour certification course, you'll be on call to deploy within 48 hours to disaster areas for shelter setup and supply distribution.",
                ScheduleType = "flexible", TimeCommitment = "On-call + 1 weekend training/quarter",
                LocationAddress = "Houston HQ + deployment sites", IsRemote = false, VolunteersNeeded = 100, MinimumAge = 18, Status = "active",
                CreatedAt = now.AddDays(-50), UpdatedAt = now.AddDays(-8)
            },
            new Opportunity
            {
                Id = OppId(7, 2), OrganizationId = OrgId(7), Title = "Remote Donation Coordinator", Description = "Manage incoming donations and coordinate shipping logistics from your home. Track inventory in our system and communicate with donors via email.",
                ScheduleType = "flexible", TimeCommitment = "8-10 hours/week",
                IsRemote = true, VolunteersNeeded = 6, SkillsRequired = "Organization, spreadsheets, communication", Status = "active",
                CreatedAt = now.AddDays(-35), UpdatedAt = now.AddDays(-2)
            },
            // Org 8 - Community Canvas
            new Opportunity
            {
                Id = OppId(8, 1), OrganizationId = OrgId(8), Title = "Mural Painting Volunteer", Description = "Help paint large-scale murals in underserved neighborhoods. No art experience needed — we have roles for painters, prep work, and community outreach. All supplies provided.",
                ScheduleType = "one_time", StartDate = now.AddDays(7), TimeCommitment = "Full weekend (Sat-Sun)",
                LocationAddress = "Five Points, Denver, CO", IsRemote = false, VolunteersNeeded = 30, Status = "active",
                CreatedAt = now.AddDays(-30), UpdatedAt = now.AddDays(-5)
            },
            new Opportunity
            {
                Id = OppId(8, 2), OrganizationId = OrgId(8), Title = "Art Workshop Instructor", Description = "Teach a free art class (painting, pottery, or mixed media) to community members. Classes are held at our studio space on weekday evenings.",
                ScheduleType = "recurring", RecurrenceDesc = "Weekday evenings, 6:00 PM - 8:00 PM", TimeCommitment = "2-4 hours/week",
                LocationAddress = "Community Canvas Studio, Denver, CO", IsRemote = false, VolunteersNeeded = 10, SkillsRequired = "Art skills, teaching", Status = "active",
                CreatedAt = now.AddDays(-25), UpdatedAt = now.AddDays(-3)
            },
            // Org 9 - No Hunger Project
            new Opportunity
            {
                Id = OppId(9, 1), OrganizationId = OrgId(9), Title = "Food Rescue Driver", Description = "Pick up surplus food from restaurants and grocery stores and deliver it to our distribution centers. Must have a valid driver's license and reliable vehicle.",
                ScheduleType = "recurring", RecurrenceDesc = "Daily routes available, flexible scheduling", TimeCommitment = "2-3 hours/shift",
                LocationAddress = "Various pickup points, Philadelphia, PA", IsRemote = false, VolunteersNeeded = 20, MinimumAge = 21, Status = "active",
                CreatedAt = now.AddDays(-40), UpdatedAt = now.AddDays(-4)
            },
            new Opportunity
            {
                Id = OppId(9, 2), OrganizationId = OrgId(9), Title = "Community Garden Volunteer", Description = "Help maintain our urban community gardens that provide fresh produce to local food pantries. Tasks include planting, weeding, watering, and harvesting.",
                ScheduleType = "flexible", TimeCommitment = "3-5 hours/week",
                LocationAddress = "Kensington Community Garden, Philadelphia, PA", IsRemote = false, VolunteersNeeded = 15, Status = "active",
                CreatedAt = now.AddDays(-30), UpdatedAt = now.AddDays(-2)
            },
            // Org 10 (index 0) - Habitat Helpers
            new Opportunity
            {
                Id = OppId(0, 1), OrganizationId = OrgId(0), Title = "Home Build Day Volunteer", Description = "Swing a hammer for a good cause! Join our weekend build days where teams frame walls, install drywall, and paint interiors. No construction experience required — we train on site.",
                ScheduleType = "recurring", RecurrenceDesc = "Saturdays, 7:30 AM - 3:30 PM", TimeCommitment = "8 hours/day",
                LocationAddress = "Various build sites, Nashville, TN", IsRemote = false, VolunteersNeeded = 40, MinimumAge = 16, Status = "active",
                CreatedAt = now.AddDays(-35), UpdatedAt = now.AddDays(-6)
            },
            new Opportunity
            {
                Id = OppId(0, 2), OrganizationId = OrgId(0), Title = "Grant Writing Assistant", Description = "Help research and draft grant applications to fund our building projects. Great opportunity for writers, researchers, or anyone interested in nonprofit fundraising.",
                ScheduleType = "flexible", TimeCommitment = "5-8 hours/week",
                IsRemote = true, VolunteersNeeded = 4, SkillsRequired = "Writing, research", Status = "active",
                CreatedAt = now.AddDays(-20), UpdatedAt = now.AddDays(-1)
            }
        ];
    }

    private static List<Post> CreatePosts(List<Organization> orgs, List<Opportunity> opps, DateTime now)
    {
        return
        [
            // Org 1 - Ocean Guardians (posts 1-3)
            new Post
            {
                Id = PostId(1, 1), OrganizationId = OrgId(1), OpportunityId = OppId(1, 1),
                Title = "200 volunteers removed 2,000 lbs of trash from Mission Beach!", Description = "Our biggest cleanup yet! Check out the incredible before-and-after of Mission Beach. Every piece of trash picked up is one less threat to marine life.",
                MediaType = "video", Status = "active", ViewCount = 1250, SaveCount = 89, InterestCount = 34,
                CreatedAt = now.AddDays(-10), UpdatedAt = now.AddDays(-10)
            },
            new Post
            {
                Id = PostId(1, 2), OrganizationId = OrgId(1), OpportunityId = OppId(1, 2),
                Title = "Kids learning about tide pools at our marine workshop", Description = "These future marine biologists spent the morning exploring tide pools and learning about ocean ecosystems. The excitement on their faces says it all!",
                MediaType = "video", Status = "active", ViewCount = 890, SaveCount = 62, InterestCount = 18,
                CreatedAt = now.AddDays(-7), UpdatedAt = now.AddDays(-7)
            },
            new Post
            {
                Id = PostId(1, 3), OrganizationId = OrgId(1),
                Title = "Sea turtle release day — meet Coral!", Description = "After 3 months of rehabilitation, Coral the sea turtle is heading home. Watch her first swim back in the Pacific. This is why we do what we do.",
                MediaType = "video", Status = "active", ViewCount = 3200, SaveCount = 245, InterestCount = 67,
                CreatedAt = now.AddDays(-3), UpdatedAt = now.AddDays(-3)
            },
            // Org 2 - Code for Tomorrow (posts 1-3)
            new Post
            {
                Id = PostId(2, 1), OrganizationId = OrgId(2), OpportunityId = OppId(2, 1),
                Title = "Teen coders ship their first Python app!", Description = "After 8 weeks of hard work, our bootcamp students presented their final projects. From weather apps to quiz games — these kids are the real deal.",
                MediaType = "video", Status = "active", ViewCount = 1100, SaveCount = 78, InterestCount = 29,
                CreatedAt = now.AddDays(-12), UpdatedAt = now.AddDays(-12)
            },
            new Post
            {
                Id = PostId(2, 2), OrganizationId = OrgId(2), OpportunityId = OppId(2, 2),
                Title = "50 laptops refurbished and ready for students", Description = "Our volunteer team worked all weekend to get these laptops ready. Each one will go to a student who didn't have a computer at home. Tech equity matters.",
                MediaType = "video", Status = "active", ViewCount = 760, SaveCount = 55, InterestCount = 12,
                CreatedAt = now.AddDays(-8), UpdatedAt = now.AddDays(-8)
            },
            new Post
            {
                Id = PostId(2, 3), OrganizationId = OrgId(2),
                Title = "Meet Jaylen — from student to mentor", Description = "Jaylen joined our bootcamp two years ago with zero coding experience. Now he's a mentor helping the next generation. His story will inspire you.",
                MediaType = "video", Status = "active", ViewCount = 2100, SaveCount = 190, InterestCount = 45,
                CreatedAt = now.AddDays(-2), UpdatedAt = now.AddDays(-2)
            },
            // Org 3 - Healthy Hearts Initiative (posts 1-3)
            new Post
            {
                Id = PostId(3, 1), OrganizationId = OrgId(3), OpportunityId = OppId(3, 1),
                Title = "Free health screenings reached 300 residents this month", Description = "Our mobile clinic just wrapped its busiest month ever. Early detection saves lives — and our volunteers made it happen.",
                MediaType = "video", Status = "active", ViewCount = 680, SaveCount = 42, InterestCount = 15,
                CreatedAt = now.AddDays(-14), UpdatedAt = now.AddDays(-14)
            },
            new Post
            {
                Id = PostId(3, 2), OrganizationId = OrgId(3), OpportunityId = OppId(3, 2),
                Title = "Heart-healthy cooking on a $5 budget", Description = "Chef Maria shows how to make a delicious, heart-healthy dinner for a family of four for under $5. Nutrition education that meets people where they are.",
                MediaType = "video", Status = "active", ViewCount = 1500, SaveCount = 130, InterestCount = 22,
                CreatedAt = now.AddDays(-9), UpdatedAt = now.AddDays(-9)
            },
            new Post
            {
                Id = PostId(3, 3), OrganizationId = OrgId(3),
                Title = "Community yoga in the park — free and open to all", Description = "Sunshine, fresh air, and 50 neighbors doing yoga together. Wellness isn't a luxury — it's a right. Join us next Saturday!",
                MediaType = "video", Status = "active", ViewCount = 920, SaveCount = 71, InterestCount = 28,
                CreatedAt = now.AddDays(-4), UpdatedAt = now.AddDays(-4)
            },
            // Org 4 - Paws & Claws Rescue (posts 1-3)
            new Post
            {
                Id = PostId(4, 1), OrganizationId = OrgId(4), OpportunityId = OppId(4, 1),
                Title = "Biscuit found his forever home!", Description = "After 6 months in foster care, Biscuit the golden retriever just got adopted by the Johnson family. Happy tears all around.",
                MediaType = "video", Status = "active", ViewCount = 4500, SaveCount = 380, InterestCount = 92,
                CreatedAt = now.AddDays(-11), UpdatedAt = now.AddDays(-11)
            },
            new Post
            {
                Id = PostId(4, 2), OrganizationId = OrgId(4), OpportunityId = OppId(4, 2),
                Title = "Adoption fair success — 15 animals found homes!", Description = "Our best adoption event yet! Fifteen dogs, cats, and rabbits went home with loving families today. Thank you to everyone who came out.",
                MediaType = "video", Status = "active", ViewCount = 2800, SaveCount = 210, InterestCount = 55,
                CreatedAt = now.AddDays(-6), UpdatedAt = now.AddDays(-6)
            },
            new Post
            {
                Id = PostId(4, 3), OrganizationId = OrgId(4),
                Title = "Meet our newest rescues — 8 kittens need foster homes", Description = "These tiny kittens were found under a porch during a rainstorm. They're healthy and playful but need warm foster homes ASAP. Can you help?",
                MediaType = "video", Status = "active", ViewCount = 5100, SaveCount = 420, InterestCount = 110,
                CreatedAt = now.AddDays(-1), UpdatedAt = now.AddDays(-1)
            },
            // Org 5 - Silver Companions (posts 1-3)
            new Post
            {
                Id = PostId(5, 1), OrganizationId = OrgId(5), OpportunityId = OppId(5, 1),
                Title = "Harold and his companion Jake — 1 year of friendship", Description = "Harold, 89, and Jake, 24, have been meeting every Tuesday for a year. From chess games to life stories, their bond is unbreakable.",
                MediaType = "video", Status = "active", ViewCount = 1800, SaveCount = 155, InterestCount = 38,
                CreatedAt = now.AddDays(-13), UpdatedAt = now.AddDays(-13)
            },
            new Post
            {
                Id = PostId(5, 2), OrganizationId = OrgId(5), OpportunityId = OppId(5, 2),
                Title = "Grandma Rose's first video call with her grandkids", Description = "After our tech workshop, Rose learned to use FaceTime. Seeing her face light up when her grandchildren appeared on screen — priceless.",
                MediaType = "video", Status = "active", ViewCount = 2400, SaveCount = 195, InterestCount = 42,
                CreatedAt = now.AddDays(-8), UpdatedAt = now.AddDays(-8)
            },
            new Post
            {
                Id = PostId(5, 3), OrganizationId = OrgId(5),
                Title = "Senior dance party at Sunrise Living — pure joy", Description = "Who says you can't dance at 90? Our weekly music sessions bring so much energy and laughter. This is what community looks like.",
                MediaType = "video", Status = "active", ViewCount = 3100, SaveCount = 260, InterestCount = 58,
                CreatedAt = now.AddDays(-2), UpdatedAt = now.AddDays(-2)
            },
            // Org 6 - Future Leaders Academy (posts 1-3)
            new Post
            {
                Id = PostId(6, 1), OrganizationId = OrgId(6), OpportunityId = OppId(6, 1),
                Title = "Straight A's! Tutoring program shows real results", Description = "Our after-school students improved their GPAs by an average of 0.8 points this semester. Dedicated tutors plus determined kids equals magic.",
                MediaType = "video", Status = "active", ViewCount = 940, SaveCount = 68, InterestCount = 21,
                CreatedAt = now.AddDays(-15), UpdatedAt = now.AddDays(-15)
            },
            new Post
            {
                Id = PostId(6, 2), OrganizationId = OrgId(6), OpportunityId = OppId(6, 2),
                Title = "Summer camp registration is OPEN!", Description = "Two weeks of leadership, adventure, and growth for teens ages 14-18. Limited spots available — sign up your teen or volunteer as a counselor!",
                MediaType = "video", Status = "active", ViewCount = 620, SaveCount = 45, InterestCount = 33,
                CreatedAt = now.AddDays(-5), UpdatedAt = now.AddDays(-5)
            },
            new Post
            {
                Id = PostId(6, 3), OrganizationId = OrgId(6),
                Title = "Student-led community cleanup — our kids giving back", Description = "Our teens organized their own neighborhood cleanup without any adult prompting. 40 bags of trash collected. These are the future leaders we need.",
                MediaType = "video", Status = "active", ViewCount = 1300, SaveCount = 95, InterestCount = 27,
                CreatedAt = now.AddDays(-1), UpdatedAt = now.AddDays(-1)
            },
            // Org 7 - Rapid Response Network (posts 1-3)
            new Post
            {
                Id = PostId(7, 1), OrganizationId = OrgId(7), OpportunityId = OppId(7, 1),
                Title = "Deployed to Louisiana — 72 hours after the storm", Description = "Our team arrived in Lake Charles within 72 hours of the hurricane. We set up emergency shelters for 200 displaced families. This is rapid response in action.",
                MediaType = "video", Status = "active", ViewCount = 2700, SaveCount = 220, InterestCount = 75,
                CreatedAt = now.AddDays(-16), UpdatedAt = now.AddDays(-16)
            },
            new Post
            {
                Id = PostId(7, 2), OrganizationId = OrgId(7), OpportunityId = OppId(7, 2),
                Title = "Supply drive filled 3 trucks in one weekend", Description = "The community came together in an incredible way. Three trucks full of water, blankets, and hygiene kits heading to tornado-affected areas.",
                MediaType = "video", Status = "active", ViewCount = 1600, SaveCount = 125, InterestCount = 40,
                CreatedAt = now.AddDays(-9), UpdatedAt = now.AddDays(-9)
            },
            new Post
            {
                Id = PostId(7, 3), OrganizationId = OrgId(7),
                Title = "Volunteer training weekend — 50 new responders certified", Description = "50 new volunteers completed our disaster response certification. They learned shelter management, first aid, and crisis communication. Ready to serve.",
                MediaType = "video", Status = "active", ViewCount = 870, SaveCount = 60, InterestCount = 48,
                CreatedAt = now.AddDays(-3), UpdatedAt = now.AddDays(-3)
            },
            // Org 8 - Community Canvas (posts 1-3)
            new Post
            {
                Id = PostId(8, 1), OrganizationId = OrgId(8), OpportunityId = OppId(8, 1),
                Title = "Before & after — the Five Points mural transformation", Description = "What was once a blank, graffiti-tagged wall is now a vibrant 60-foot mural celebrating Denver's history. 40 volunteers, 3 days, 1 masterpiece.",
                MediaType = "video", Status = "active", ViewCount = 2200, SaveCount = 185, InterestCount = 35,
                CreatedAt = now.AddDays(-14), UpdatedAt = now.AddDays(-14)
            },
            new Post
            {
                Id = PostId(8, 2), OrganizationId = OrgId(8), OpportunityId = OppId(8, 2),
                Title = "Free pottery class — look what our students made!", Description = "First-time potters showing off their creations. Art belongs to everyone, not just galleries. Our free classes prove that every week.",
                MediaType = "video", Status = "active", ViewCount = 1100, SaveCount = 88, InterestCount = 20,
                CreatedAt = now.AddDays(-7), UpdatedAt = now.AddDays(-7)
            },
            new Post
            {
                Id = PostId(8, 3), OrganizationId = OrgId(8),
                Title = "Neighborhood kids design their own park bench", Description = "We asked local kids to design a bench for their park. The winning design is being built by our volunteer woodworkers. Art + community = magic.",
                MediaType = "video", Status = "active", ViewCount = 780, SaveCount = 52, InterestCount = 15,
                CreatedAt = now.AddDays(-2), UpdatedAt = now.AddDays(-2)
            },
            // Org 9 - No Hunger Project (posts 1-3)
            new Post
            {
                Id = PostId(9, 1), OrganizationId = OrgId(9), OpportunityId = OppId(9, 1),
                Title = "10,000 lbs of food rescued this week alone", Description = "Our food rescue drivers saved ten thousand pounds of perfectly good food from going to landfills. Instead, it's feeding families across Philly.",
                MediaType = "video", Status = "active", ViewCount = 1400, SaveCount = 105, InterestCount = 30,
                CreatedAt = now.AddDays(-12), UpdatedAt = now.AddDays(-12)
            },
            new Post
            {
                Id = PostId(9, 2), OrganizationId = OrgId(9), OpportunityId = OppId(9, 2),
                Title = "Community garden harvest — tomatoes for everyone!", Description = "Our Kensington garden produced 500 lbs of tomatoes this month. Volunteers grew it, neighbors eat it. This is food sovereignty in action.",
                MediaType = "video", Status = "active", ViewCount = 960, SaveCount = 72, InterestCount = 19,
                CreatedAt = now.AddDays(-6), UpdatedAt = now.AddDays(-6)
            },
            new Post
            {
                Id = PostId(9, 3), OrganizationId = OrgId(9),
                Title = "Kids cooking class — making healthy meals fun", Description = "Teaching kids to cook with fresh, local ingredients. Today's menu: veggie stir-fry and fruit smoothies. Healthy eating habits start young!",
                MediaType = "video", Status = "active", ViewCount = 1150, SaveCount = 90, InterestCount = 25,
                CreatedAt = now.AddDays(-1), UpdatedAt = now.AddDays(-1)
            },
            // Org 10 (index 0) - Habitat Helpers (posts 1-3)
            new Post
            {
                Id = PostId(0, 1), OrganizationId = OrgId(0), OpportunityId = OppId(0, 1),
                Title = "The Martinez family gets their keys!", Description = "After 6 months of building, the Martinez family moved into their new home today. 200 volunteers made this possible. Welcome home!",
                MediaType = "video", Status = "active", ViewCount = 3800, SaveCount = 310, InterestCount = 85,
                CreatedAt = now.AddDays(-10), UpdatedAt = now.AddDays(-10)
            },
            new Post
            {
                Id = PostId(0, 2), OrganizationId = OrgId(0), OpportunityId = OppId(0, 2),
                Title = "We just secured a $50K grant!", Description = "Thanks to our volunteer grant writers, we landed a major grant to fund our next three homes. Proof that every skill can make a difference.",
                MediaType = "video", Status = "active", ViewCount = 1050, SaveCount = 80, InterestCount = 16,
                CreatedAt = now.AddDays(-5), UpdatedAt = now.AddDays(-5)
            },
            new Post
            {
                Id = PostId(0, 3), OrganizationId = OrgId(0),
                Title = "Veterans home repair blitz — 5 houses in 2 days", Description = "Our volunteer crews fixed roofs, replaced plumbing, and painted five veterans' homes in a single weekend. These heroes deserve safe homes.",
                MediaType = "video", Status = "active", ViewCount = 2500, SaveCount = 200, InterestCount = 60,
                CreatedAt = now.AddDays(-1), UpdatedAt = now.AddDays(-1)
            }
        ];
    }

    private static List<PostMedia> CreatePostMedia(List<Post> posts, DateTime now)
    {
        // Posts are ordered: org1-p1, org1-p2, org1-p3, org2-p1, ... so we can derive indices
        var orgIndices = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
        var media = new List<PostMedia>();

        for (var i = 0; i < posts.Count; i++)
        {
            var orgN = orgIndices[i / 3];
            var postN = (i % 3) + 1;
            var mediaId = MediaId(orgN, postN);

            media.Add(new PostMedia
            {
                Id = mediaId,
                PostId = posts[i].Id,
                MediaUrl = $"https://placeholder.scrollforcause.dev/videos/{mediaId}/720p.mp4",
                ThumbnailUrl = $"https://placeholder.scrollforcause.dev/videos/{mediaId}/thumb.jpg",
                MediaType = "video",
                DurationSeconds = 10,
                Width = 1080,
                Height = 1920,
                FileSizeBytes = 5_000_000,
                DisplayOrder = 0,
                ProcessingStatus = "complete",
                CreatedAt = posts[i].CreatedAt
            });
        }
        return media;
    }

    private static List<PostTag> CreatePostTags(List<Post> posts)
    {
        var orgIndices = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };

        var tagsByOrg = new Dictionary<int, string[]>
        {
            [1] = ["ocean", "beach-cleanup", "marine-life", "environment", "conservation", "volunteer"],
            [2] = ["coding", "education", "tech", "youth", "mentorship", "digital-equity"],
            [3] = ["health", "wellness", "nutrition", "community-health", "free-screenings", "heart-health"],
            [4] = ["animals", "adoption", "rescue", "foster", "pets", "dogs", "cats"],
            [5] = ["seniors", "companionship", "elderly-care", "tech-help", "community", "aging"],
            [6] = ["youth", "leadership", "tutoring", "education", "summer-camp", "mentorship"],
            [7] = ["disaster-relief", "emergency", "volunteers", "hurricane", "community-response", "supplies"],
            [8] = ["art", "murals", "community-art", "culture", "public-art", "pottery"],
            [9] = ["food", "hunger", "food-rescue", "community-garden", "nutrition", "food-security"],
            [0] = ["housing", "home-building", "veterans", "grants", "construction", "community"]
        };

        var tags = new List<PostTag>();
        for (var i = 0; i < posts.Count; i++)
        {
            var orgN = orgIndices[i / 3];
            var postN = (i % 3) + 1;
            var orgTags = tagsByOrg[orgN];

            // Each post gets 2-3 tags based on its position
            var startIdx = (postN - 1) * 2;
            var count = postN == 2 ? 3 : 2; // middle post gets 3 tags

            for (var j = 0; j < count && startIdx + j < orgTags.Length; j++)
            {
                tags.Add(new PostTag
                {
                    PostId = posts[i].Id,
                    Tag = orgTags[startIdx + j]
                });
            }
        }
        return tags;
    }
}
