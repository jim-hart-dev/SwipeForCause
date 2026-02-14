# SwipeForCause — API Endpoints

## Base URL

```
Production:  https://api.swipeforcause.com/api/v1
Development: https://localhost:5001/api/v1
```

## Authentication

All endpoints require a valid Clerk JWT in the Authorization header unless marked as `[Public]`.

```
Authorization: Bearer <clerk_jwt_token>
```

## Standard Response Formats

### Success (single item)
```json
{
    "data": { ... }
}
```

### Success (list with pagination)
```json
{
    "data": [ ... ],
    "cursor": "eyJjcmVhdGVkQXQiOiIyMDI2LTAxLTAxVDAwOjAwOjAwWiJ9",
    "hasMore": true
}
```

### Error
```json
{
    "error": {
        "code": "VALIDATION_ERROR",
        "message": "Description is required",
        "details": { ... }
    }
}
```

---

## Feed

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/feed` | Volunteer | Get personalized feed |
| POST | `/feed/view` | Volunteer | Record that a post was viewed |

### GET /feed

Returns paginated feed items for the authenticated volunteer.

**Query Parameters:**
- `cursor` (string, optional) — pagination cursor from previous response
- `limit` (int, optional, default 10, max 20) — items per page

**Response:**
```json
{
    "data": [
        {
            "postId": "uuid",
            "title": "Help us clean the Charleston waterfront!",
            "description": "Join us this Saturday for...",
            "mediaType": "video",
            "media": [
                {
                    "mediaId": "uuid",
                    "url": "https://cdn.swipeforcause.com/videos/abc/720p.mp4",
                    "thumbnailUrl": "https://cdn.swipeforcause.com/videos/abc/thumbnail.jpg",
                    "lowResUrl": "https://cdn.swipeforcause.com/videos/abc/360p.mp4",
                    "type": "video",
                    "durationSeconds": 45,
                    "width": 1080,
                    "height": 1920
                }
            ],
            "organization": {
                "organizationId": "uuid",
                "name": "Charleston Waterkeeper",
                "logoUrl": "https://cdn.swipeforcause.com/logos/xyz.jpg",
                "isVerified": true
            },
            "opportunity": {
                "opportunityId": "uuid",
                "title": "Beach Cleanup Day",
                "scheduleType": "one_time",
                "startDate": "2026-03-15T09:00:00Z",
                "location": "Folly Beach, SC",
                "isRemote": false,
                "timeCommitment": "3 hours"
            },
            "isSaved": false,
            "isFollowing": true,
            "hasExpressedInterest": false,
            "createdAt": "2026-02-10T14:30:00Z"
        }
    ],
    "cursor": "eyJjcmVhdGVkQXQiOi...",
    "hasMore": true
}
```

### POST /feed/view

Records a post view for feed deduplication.

**Request:**
```json
{ "postId": "uuid" }
```

---

## Organizations

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/organizations` | Clerk User | Register new organization |
| GET | `/organizations/{id}` | Public | Get organization profile |
| PUT | `/organizations/{id}` | Organization (owner) | Update organization profile |
| GET | `/organizations/{id}/posts` | Public | Get organization's posts |
| GET | `/organizations/{id}/opportunities` | Public | Get organization's opportunities |
| GET | `/organizations/{id}/stats` | Organization (owner) | Get dashboard stats |

### POST /organizations

**Request:**
```json
{
    "name": "Charleston Waterkeeper",
    "ein": "12-3456789",
    "description": "Protecting Charleston's waterways...",
    "contactName": "Jane Smith",
    "contactEmail": "jane@charlestonwaterkeeper.org",
    "websiteUrl": "https://charlestonwaterkeeper.org",
    "city": "Charleston",
    "state": "SC",
    "categoryIds": ["uuid", "uuid"]
}
```

**Response:** `201 Created`
```json
{
    "data": {
        "organizationId": "uuid",
        "verificationStatus": "pending"
    }
}
```

### GET /organizations/{id} `[Public]`

**Response:**
```json
{
    "data": {
        "organizationId": "uuid",
        "name": "Charleston Waterkeeper",
        "description": "...",
        "logoUrl": "...",
        "coverImageUrl": "...",
        "websiteUrl": "...",
        "city": "Charleston",
        "state": "SC",
        "categories": [
            { "categoryId": "uuid", "name": "Environment", "slug": "environment" }
        ],
        "followerCount": 234,
        "isVerified": true,
        "isFollowing": false,
        "postCount": 15,
        "activeOpportunityCount": 3
    }
}
```

---

## Posts

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/posts` | Organization | Create new post |
| GET | `/posts/{id}` | Public | Get single post |
| PUT | `/posts/{id}` | Organization (owner) | Update post |
| DELETE | `/posts/{id}` | Organization (owner) | Delete post |

### POST /posts

**Request:**
```json
{
    "title": "Help us clean the waterfront!",
    "description": "Join us this Saturday for our monthly cleanup...",
    "opportunityId": "uuid",
    "mediaIds": ["uuid"],
    "tags": ["cleanup", "environment", "charleston"]
}
```

Note: Media is uploaded separately first via the media upload flow. `mediaIds` reference already-uploaded and processed media.

---

## Opportunities

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/opportunities` | Organization | Create opportunity |
| GET | `/opportunities/{id}` | Public | Get opportunity detail |
| PUT | `/opportunities/{id}` | Organization (owner) | Update opportunity |
| DELETE | `/opportunities/{id}` | Organization (owner) | Delete opportunity |

### POST /opportunities

**Request:**
```json
{
    "title": "Beach Cleanup Day",
    "description": "Help us remove trash and debris from Folly Beach...",
    "locationAddress": "Folly Beach, SC",
    "isRemote": false,
    "scheduleType": "one_time",
    "startDate": "2026-03-15T09:00:00Z",
    "endDate": "2026-03-15T12:00:00Z",
    "volunteersNeeded": 25,
    "timeCommitment": "3 hours",
    "skillsRequired": "None — all skill levels welcome!",
    "minimumAge": 16
}
```

---

## Volunteer Interests

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/interests` | Volunteer | Express interest in opportunity |
| GET | `/interests/mine` | Volunteer | Get my interest history |
| GET | `/interests/org` | Organization | Get interests for my org |
| PUT | `/interests/{id}/status` | Organization | Update interest status |

### POST /interests

**Request:**
```json
{
    "opportunityId": "uuid",
    "postId": "uuid",
    "message": "I'd love to help with the cleanup!"
}
```

**Response:** `200 OK`
```json
{
    "data": {
        "interestId": "uuid",
        "status": "pending",
        "createdAt": "2026-02-13T10:00:00Z"
    }
}
```

### GET /interests/org

**Query Parameters:**
- `status` (string, optional) — filter by status (pending, reviewed, accepted, declined)
- `opportunityId` (uuid, optional) — filter by opportunity
- `cursor`, `limit` — pagination

**Response:**
```json
{
    "data": [
        {
            "interestId": "uuid",
            "volunteer": {
                "volunteerId": "uuid",
                "displayName": "Jim D.",
                "avatarUrl": "...",
                "city": "Charleston",
                "state": "SC"
            },
            "opportunity": {
                "opportunityId": "uuid",
                "title": "Beach Cleanup Day"
            },
            "message": "I'd love to help!",
            "status": "pending",
            "createdAt": "2026-02-13T10:00:00Z"
        }
    ],
    "cursor": "...",
    "hasMore": false
}
```

### PUT /interests/{id}/status

**Request:**
```json
{ "status": "accepted" }
```

---

## Saves

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/saves/{postId}` | Volunteer | Save a post |
| DELETE | `/saves/{postId}` | Volunteer | Unsave a post |
| GET | `/saves` | Volunteer | Get saved posts |

---

## Follows

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/follows/{organizationId}` | Volunteer | Follow an org |
| DELETE | `/follows/{organizationId}` | Volunteer | Unfollow an org |
| GET | `/follows` | Volunteer | Get followed orgs |

---

## Media

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/media/upload-url` | Organization | Get pre-signed upload URL |
| POST | `/media/confirm` | Organization | Confirm upload complete |
| DELETE | `/media/{id}` | Organization (owner) | Delete media |

### POST /media/upload-url

**Request:**
```json
{
    "fileName": "cleanup-video.mp4",
    "contentType": "video/mp4",
    "fileSizeBytes": 15000000
}
```

**Response:**
```json
{
    "data": {
        "uploadUrl": "https://swipeforcause.blob.core.windows.net/uploads/...",
        "mediaId": "uuid",
        "expiresAt": "2026-02-13T11:00:00Z"
    }
}
```

### POST /media/confirm

**Request:**
```json
{ "mediaId": "uuid" }
```

Triggers async media processing (transcoding, thumbnailing).

---

## Search

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/search` | Volunteer | Search orgs and opportunities |
| GET | `/categories` | Public | Get all categories |
| GET | `/categories/{slug}/posts` | Volunteer | Get posts by category |

### GET /search

**Query Parameters:**
- `q` (string) — search query
- `type` (string, optional) — `organizations`, `opportunities`, or both (default)
- `categoryId` (uuid, optional) — filter by category
- `isRemote` (bool, optional) — remote-friendly only
- `state` (string, optional) — filter by state
- `cursor`, `limit` — pagination

---

## Volunteers

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/volunteers` | Clerk User | Register as volunteer |
| GET | `/volunteers/me` | Volunteer | Get my profile |
| PUT | `/volunteers/me` | Volunteer | Update my profile |
| GET | `/volunteers/me/activity` | Volunteer | Get my activity feed |

---

## Reports (Moderation)

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/reports` | Any authenticated | Report content |
| GET | `/admin/reports` | Admin | Get moderation queue |
| PUT | `/admin/reports/{id}` | Admin | Action on report |
| PUT | `/admin/organizations/{id}/verify` | Admin | Verify/reject org |

---

## Notifications

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/notifications/settings` | Any authenticated | Get notification prefs |
| PUT | `/notifications/settings` | Any authenticated | Update notification prefs |

---

## Webhook Endpoints (Internal)

| Method | Path | Source | Description |
|--------|------|--------|-------------|
| POST | `/webhooks/clerk` | Clerk | User created/updated/deleted events |
| POST | `/webhooks/media` | Azure Functions | Media processing complete callback |
