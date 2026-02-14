# SwipeForCause — Technical Architecture

## Architecture Overview

SwipeForCause follows Jim's standard architecture patterns: C# Web API with vertical slice architecture, React TypeScript frontend, PostgreSQL database, Clerk authentication, and Azure hosting.

```
┌──────────────────────────────────────────────────┐
│                   Client Layer                    │
│        React (TypeScript) — Mobile-First SPA      │
│          Hosted on Azure Static Web Apps          │
└──────────────────┬───────────────────────────────┘
                   │ HTTPS
                   ▼
┌──────────────────────────────────────────────────┐
│                   API Layer                       │
│         C# Web API (.NET 8) — Vertical Slices     │
│            Hosted on Azure App Service            │
│                                                   │
│  ┌─────────────┐ ┌──────────┐ ┌───────────────┐  │
│  │   Clerk     │ │ Azure    │ │   SendGrid    │  │
│  │   Auth      │ │ Blob     │ │   Email       │  │
│  │   Middleware│ │ Storage  │ │   Service     │  │
│  └─────────────┘ └──────────┘ └───────────────┘  │
└──────────────────┬───────────────────────────────┘
                   │
                   ▼
┌──────────────────────────────────────────────────┐
│                 Data Layer                        │
│         PostgreSQL on Azure Database              │
│                                                   │
│         Azure Blob Storage (media files)          │
│         Azure CDN (media delivery)                │
└──────────────────────────────────────────────────┘
```

## Backend Architecture

### Vertical Slice Organization

Each feature is a self-contained slice with its own request, response, handler, and validation. No shared service layers or generic repository patterns.

```
src/
├── SwipeForCause.Api/
│   ├── Program.cs
│   ├── appsettings.json
│   │
│   ├── Features/
│   │   ├── Feed/
│   │   │   ├── GetFeed.cs              // GetFeedRequest, GetFeedResponse, GetFeedHandler
│   │   │   └── GetFeedItem.cs          // GetFeedItemRequest, GetFeedItemResponse, handler
│   │   │
│   │   ├── Organizations/
│   │   │   ├── RegisterOrganization.cs
│   │   │   ├── GetOrganization.cs
│   │   │   ├── UpdateOrganization.cs
│   │   │   └── VerifyOrganization.cs
│   │   │
│   │   ├── Posts/
│   │   │   ├── CreatePost.cs
│   │   │   ├── GetPost.cs
│   │   │   ├── UpdatePost.cs
│   │   │   ├── DeletePost.cs
│   │   │   └── GetOrganizationPosts.cs
│   │   │
│   │   ├── Opportunities/
│   │   │   ├── CreateOpportunity.cs
│   │   │   ├── GetOpportunity.cs
│   │   │   ├── UpdateOpportunity.cs
│   │   │   └── GetOrganizationOpportunities.cs
│   │   │
│   │   ├── Volunteers/
│   │   │   ├── RegisterVolunteer.cs
│   │   │   ├── GetVolunteerProfile.cs
│   │   │   ├── UpdateVolunteerProfile.cs
│   │   │   └── GetVolunteerActivity.cs
│   │   │
│   │   ├── Interests/
│   │   │   ├── ExpressInterest.cs
│   │   │   ├── GetInterestsForOrg.cs
│   │   │   ├── UpdateInterestStatus.cs
│   │   │   └── GetVolunteerInterests.cs
│   │   │
│   │   ├── Saves/
│   │   │   ├── SavePost.cs
│   │   │   ├── UnsavePost.cs
│   │   │   └── GetSavedPosts.cs
│   │   │
│   │   ├── Follows/
│   │   │   ├── FollowOrganization.cs
│   │   │   ├── UnfollowOrganization.cs
│   │   │   └── GetFollowedOrganizations.cs
│   │   │
│   │   ├── Search/
│   │   │   ├── SearchOrganizations.cs
│   │   │   ├── SearchOpportunities.cs
│   │   │   └── BrowseCategories.cs
│   │   │
│   │   ├── Media/
│   │   │   ├── UploadMedia.cs
│   │   │   └── DeleteMedia.cs
│   │   │
│   │   ├── Moderation/
│   │   │   ├── ReportContent.cs
│   │   │   ├── GetModerationQueue.cs
│   │   │   └── ModerateContent.cs
│   │   │
│   │   └── Notifications/
│   │       ├── GetNotificationSettings.cs
│   │       └── UpdateNotificationSettings.cs
│   │
│   ├── Database/
│   │   ├── AppDbContext.cs
│   │   ├── Entities/                   // EF Core entity classes
│   │   └── Migrations/
│   │
│   ├── Infrastructure/
│   │   ├── Auth/
│   │   │   └── ClerkAuthMiddleware.cs
│   │   ├── Storage/
│   │   │   └── AzureBlobService.cs
│   │   ├── Email/
│   │   │   └── SendGridEmailService.cs
│   │   └── Media/
│   │       └── MediaProcessingService.cs
│   │
│   └── Common/
│       ├── PagedResponse.cs
│       └── ErrorResponse.cs
```

### Key Architecture Decisions

**Concrete classes, not interfaces.** Services like `AzureBlobService` and `SendGridEmailService` are registered as concrete classes. No `IStorageService` or `IEmailService` abstractions until there's a real need to swap implementations.

**\*Request/\*Response naming.** Every slice uses `{Action}Request` and `{Action}Response`. No generic DTOs, no `{Entity}Dto` patterns.

**Minimal folder nesting.** Features are one level deep. No sub-folders within a feature unless complexity demands it. Each slice is a single file containing request, response, handler, and optionally a validator.

**Slice file pattern:**

```csharp
// Features/Interests/ExpressInterest.cs

public class ExpressInterestRequest
{
    public Guid OpportunityId { get; set; }
    public string? Message { get; set; }
}

public class ExpressInterestResponse
{
    public Guid InterestId { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }
}

public static class ExpressInterestEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/interests", Handle)
            .RequireAuthorization("Volunteer");
    }

    private static async Task<IResult> Handle(
        ExpressInterestRequest request,
        AppDbContext db,
        ClaimsPrincipal user)
    {
        var volunteerId = user.GetVolunteerId();

        // Check for duplicate interest
        var existing = await db.VolunteerInterests
            .AnyAsync(i => i.VolunteerId == volunteerId
                && i.OpportunityId == request.OpportunityId);

        if (existing)
            return Results.Conflict("Already expressed interest in this opportunity");

        var interest = new VolunteerInterest
        {
            Id = Guid.NewGuid(),
            VolunteerId = volunteerId,
            OpportunityId = request.OpportunityId,
            Message = request.Message,
            Status = InterestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        db.VolunteerInterests.Add(interest);
        await db.SaveChangesAsync();

        // TODO: Trigger notification to org

        return Results.Ok(new ExpressInterestResponse
        {
            InterestId = interest.Id,
            Status = interest.Status.ToString(),
            CreatedAt = interest.CreatedAt
        });
    }
}
```

### Authentication

Clerk handles all authentication. The backend validates Clerk JWTs and extracts user identity.

**Two authorization policies:**
- `"Volunteer"` — requires Clerk user with volunteer role
- `"Organization"` — requires Clerk user with organization role and verified status
- `"Admin"` — requires Clerk user with admin role (for moderation)

**Clerk webhook integration:** Clerk fires webhooks on user creation. Backend creates corresponding Volunteer or Organization records in PostgreSQL linked to the Clerk user ID.

### Media Pipeline

```
Upload Flow:
1. Frontend requests pre-signed upload URL from API
2. Frontend uploads directly to Azure Blob Storage (avoids API bottleneck)
3. Frontend notifies API that upload is complete with blob reference
4. API triggers processing:
   - Videos: Transcode to H.264 MP4, generate thumbnail, create multiple resolutions (360p, 720p)
   - Images: Resize/optimize, generate thumbnail, create feed-size and full-size versions
5. API stores processed media URLs in database
6. Content served via Azure CDN
```

**Azure Blob Storage structure:**
```
swipeforcause-media/
├── uploads/          # Raw uploads (temporary)
├── videos/
│   ├── {id}/
│   │   ├── original.mp4
│   │   ├── 720p.mp4
│   │   ├── 360p.mp4
│   │   └── thumbnail.jpg
├── images/
│   ├── {id}/
│   │   ├── original.jpg
│   │   ├── feed.jpg      # Optimized for feed display
│   │   └── thumbnail.jpg
├── avatars/
│   └── {userId}.jpg
└── logos/
    └── {orgId}.jpg
```

### Video Processing (MVP Approach)

For MVP, use **Azure Media Services** or a simpler approach with **FFmpeg running in an Azure Function**:

1. Blob trigger fires when video lands in `uploads/`
2. Azure Function pulls the video, runs FFmpeg transcoding
3. Outputs placed in `videos/{id}/` at target resolutions
4. Database record updated with processed media URLs
5. Original upload deleted after processing

This keeps costs low for MVP. Can migrate to a dedicated media pipeline later.

---

## Frontend Architecture

### Tech Stack

- **React 18** with TypeScript
- **Vite** for build tooling
- **React Router** for navigation
- **TanStack Query** for server state management
- **Clerk React SDK** for authentication
- **Tailwind CSS** for styling (mobile-first utilities)
- **Framer Motion** for scroll/swipe animations

### Project Structure

```
src/
├── main.tsx
├── App.tsx
├── routes/
│   ├── index.tsx                # Route definitions
│   ├── Feed.tsx
│   ├── Explore.tsx
│   ├── SavedPosts.tsx
│   ├── VolunteerProfile.tsx
│   ├── OrgDashboard.tsx
│   ├── OrgCreatePost.tsx
│   ├── OrgManageContent.tsx
│   ├── OrgProfile.tsx           # Public org profile page
│   ├── OpportunityDetail.tsx
│   ├── Login.tsx
│   ├── RegisterVolunteer.tsx
│   └── RegisterOrganization.tsx
│
├── components/
│   ├── feed/
│   │   ├── FeedContainer.tsx    # Scroll management, virtualization
│   │   ├── FeedItem.tsx         # Single feed card (video or image)
│   │   ├── VideoPlayer.tsx      # Autoplay, mute/unmute, progress
│   │   ├── ImageCarousel.tsx    # Horizontal swipe for multi-image
│   │   └── FeedItemOverlay.tsx  # CTA, org info, save/follow buttons
│   │
│   ├── shared/
│   │   ├── BottomNav.tsx
│   │   ├── VolunteerNowSheet.tsx  # Bottom sheet for interest expression
│   │   ├── OrgCard.tsx
│   │   ├── OpportunityCard.tsx
│   │   └── CategoryTag.tsx
│   │
│   └── org/
│       ├── PostEditor.tsx
│       ├── OpportunityForm.tsx
│       ├── InterestList.tsx
│       └── MediaUploader.tsx
│
├── api/
│   ├── client.ts               # Axios/fetch wrapper with Clerk token
│   ├── feed.ts
│   ├── organizations.ts
│   ├── posts.ts
│   ├── opportunities.ts
│   ├── interests.ts
│   ├── saves.ts
│   ├── follows.ts
│   └── media.ts
│
├── hooks/
│   ├── useFeed.ts
│   ├── useIntersectionObserver.ts  # For video autoplay
│   ├── useMediaUpload.ts
│   └── useVolunteerNow.ts
│
└── types/
    └── index.ts                # Shared TypeScript types
```

### Feed Implementation Strategy

The feed is the most critical UI component. Key technical considerations:

**Scroll behavior:** Use CSS `scroll-snap-type: y mandatory` with `scroll-snap-align: start` on each feed item for TikTok-style snap scrolling.

**Video autoplay:** Use Intersection Observer API to detect when a video enters the viewport (>50% visible) and trigger play. Pause when it leaves.

**Virtualization:** For long feeds, virtualize off-screen items to keep DOM size manageable. Only render ~5 items at a time (2 above, current, 2 below). Use a library like `react-virtuoso` or custom implementation.

**Infinite scroll:** TanStack Query's `useInfiniteQuery` with cursor-based pagination. Load next page when user is 2 items from the bottom.

**Preloading:** Preload the next video while current one plays to ensure seamless transitions.

---

## Infrastructure & Deployment

### Azure Resources

| Resource | Service | Purpose |
|----------|---------|---------|
| API Server | Azure App Service (B1 tier for MVP) | Host C# Web API |
| Frontend | Azure Static Web Apps | Host React SPA |
| Database | Azure Database for PostgreSQL (Flexible Server, Burstable B1ms) | Primary data store |
| Media Storage | Azure Blob Storage (Hot tier) | Video/image storage |
| CDN | Azure CDN | Media delivery with edge caching |
| Media Processing | Azure Functions (Consumption plan) | Video transcoding |
| DNS | Azure DNS or Cloudflare | Domain management |

### CI/CD Pipeline

GitHub Actions for both frontend and backend:

```
main branch push → Build → Test → Deploy to Azure

Backend:
1. dotnet build
2. dotnet test
3. dotnet publish
4. Deploy to Azure App Service

Frontend:
1. npm ci
2. npm run build
3. Deploy to Azure Static Web Apps
```

### Environment Configuration

```
Development:  localhost API + local PostgreSQL + Azure Blob (dev container)
Staging:      Azure App Service (staging slot) + Azure PostgreSQL (dev tier)
Production:   Azure App Service + Azure PostgreSQL (production tier) + CDN
```

### Estimated Monthly Costs (MVP Scale)

| Service | Tier | Est. Cost |
|---------|------|-----------|
| App Service | B1 | $13/mo |
| PostgreSQL Flexible | B1ms | $15/mo |
| Blob Storage | Hot, ~50GB | $1/mo |
| CDN | Standard | $5-10/mo |
| Azure Functions | Consumption | $0-5/mo |
| Clerk | Free tier (up to 10k MAU) | $0 |
| SendGrid | Free tier (100 emails/day) | $0 |
| **Total** | | **~$35-45/mo** |

---

## API Design Principles

- RESTful endpoints with consistent naming
- Cursor-based pagination for feed and lists (no offset pagination)
- Consistent error response format across all endpoints
- Rate limiting: 100 requests/minute for authenticated users, 20/minute for unauthenticated
- API versioning via URL prefix: `/api/v1/`
- All timestamps in UTC, ISO 8601 format
- All IDs are GUIDs
