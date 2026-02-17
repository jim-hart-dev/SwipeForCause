# Feed Thin Slice Design

Thin slice of issue #13 — a simple chronological feed endpoint to enable frontend development and end-to-end testing.

## Endpoint

`GET /api/v1/feed` — public, no authentication required.

### Query Parameters

| Parameter | Type | Default | Constraint |
|-----------|------|---------|------------|
| cursor | string? | null | Base64-encoded (CreatedAt, PostId) |
| limit | int | 10 | Max 20 |

### Query Logic

1. Filter: `Post.Status == "active"` AND `Organization.VerificationStatus == "verified"` AND `Organization.IsActive`
2. Include: Organization, Opportunity, Media (ordered by DisplayOrder)
3. Order: `CreatedAt DESC`, `Id DESC` (tiebreaker)
4. Cursor: `WHERE (CreatedAt, Id) < (cursorCreatedAt, cursorId)`
5. Fetch `limit + 1` rows to determine `hasMore`, return `limit`

## Response Shape

```
GetFeedResponse (PagedResponse<FeedItem>):
  data: FeedItem[]
  cursor: string?
  hasMore: bool

FeedItem:
  postId: Guid
  title: string
  description: string?
  mediaType: string
  createdAt: DateTime

  media: MediaInfo[]
    - id: Guid
    - url: string
    - thumbnailUrl: string?
    - duration: decimal?
    - width: int?
    - height: int?

  organization:
    - id: Guid
    - name: string
    - logoUrl: string?
    - isVerified: bool

  opportunity: (null if post not linked)
    - id: Guid
    - title: string
    - scheduleType: string
    - startDate: DateTime?
    - location: string?
    - isRemote: bool
    - timeCommitment: string?
```

No user-state fields (isSaved, isFollowing, hasExpressedInterest) — deferred to follow-up issue.

## Implementation

**File:** `Features/Feed/GetFeed.cs` — single vertical slice with Request, Response, FeedItem (nested records), Validator, and handler.

**Approach:** Single EF Core query with eager loading (Include). Cursor encoded as base64 `(CreatedAt, PostId)` — forward-compatible with the personalized feed.

## Testing

**File:** `ScrollForCause.Api.Tests/GetFeedTests.cs`

Test cases:
1. Returns 200 with empty feed when no posts exist
2. Returns posts ordered by newest first
3. Only returns active posts from verified orgs
4. Cursor pagination returns non-overlapping pages
5. Respects limit parameter (default 10, max 20)
6. Returns hasMore: false on last page
7. Includes media, organization, and opportunity data
8. Invalid cursor returns 400

## Scope Boundary

This issue covers ONLY the chronological feed. The following remain in issue #13:
- Authentication-dependent behavior (public vs authenticated)
- User state fields (isSaved, isFollowing, hasExpressedInterest)
- Interest category boosting
- Followed org boosting
- View deduplication (feed_views)
- POST /api/v1/feed/view endpoint
- Rate limiting
