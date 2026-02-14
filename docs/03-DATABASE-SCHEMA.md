# SwipeForCause — Database Schema Design

## Overview

PostgreSQL database using EF Core with code-first migrations. All tables use GUIDs as primary keys and include standard audit columns (CreatedAt, UpdatedAt).

---

## Entity Relationship Diagram (Text)

```
Volunteer ──────< VolunteerInterest >────── Opportunity
    │                                           │
    ├──< SavedPost >──── Post ──────────────────┤
    │                      │                    │
    ├──< Follow >─── Organization ──────< Post  │
    │                      │                    │
    │                      ├──────< Opportunity ─┘
    │                      │
    │                      ├──────< OrganizationCategory
    │                      │              │
    VolunteerCategory ─────┼──── Category ─┘
                           │
                     ContentReport
```

---

## Tables

### volunteers

Core volunteer user profile linked to Clerk.

```sql
CREATE TABLE volunteers (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    clerk_user_id       VARCHAR(255) NOT NULL UNIQUE,
    email               VARCHAR(255) NOT NULL,
    display_name        VARCHAR(100) NOT NULL,
    bio                 TEXT,
    avatar_url          VARCHAR(500),
    city                VARCHAR(100),
    state               VARCHAR(50),
    latitude            DECIMAL(10, 7),
    longitude           DECIMAL(10, 7),
    is_active           BOOLEAN NOT NULL DEFAULT true,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_volunteers_clerk_user_id ON volunteers(clerk_user_id);
CREATE INDEX idx_volunteers_location ON volunteers(state, city);
```

### organizations

Nonprofit organization profile linked to Clerk.

```sql
CREATE TABLE organizations (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    clerk_user_id       VARCHAR(255) NOT NULL UNIQUE,
    name                VARCHAR(200) NOT NULL,
    ein                 VARCHAR(20) NOT NULL,
    description         TEXT NOT NULL,
    contact_name        VARCHAR(100) NOT NULL,
    contact_email       VARCHAR(255) NOT NULL,
    website_url         VARCHAR(500),
    logo_url            VARCHAR(500),
    cover_image_url     VARCHAR(500),
    city                VARCHAR(100),
    state               VARCHAR(50),
    latitude            DECIMAL(10, 7),
    longitude           DECIMAL(10, 7),
    verification_status VARCHAR(20) NOT NULL DEFAULT 'pending',  -- pending, verified, rejected
    verified_at         TIMESTAMPTZ,
    follower_count      INTEGER NOT NULL DEFAULT 0,
    is_active           BOOLEAN NOT NULL DEFAULT true,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_organizations_clerk_user_id ON organizations(clerk_user_id);
CREATE INDEX idx_organizations_verification ON organizations(verification_status);
CREATE INDEX idx_organizations_location ON organizations(state, city);
CREATE INDEX idx_organizations_name ON organizations(name);
```

### categories

Predefined cause categories.

```sql
CREATE TABLE categories (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name                VARCHAR(50) NOT NULL UNIQUE,
    slug                VARCHAR(50) NOT NULL UNIQUE,
    icon                VARCHAR(50),               -- emoji or icon identifier
    display_order       INTEGER NOT NULL DEFAULT 0,
    is_active           BOOLEAN NOT NULL DEFAULT true
);

-- Seed data
INSERT INTO categories (id, name, slug, icon, display_order) VALUES
    (gen_random_uuid(), 'Environment', 'environment', '🌍', 1),
    (gen_random_uuid(), 'Education', 'education', '📚', 2),
    (gen_random_uuid(), 'Health', 'health', '🏥', 3),
    (gen_random_uuid(), 'Animals', 'animals', '🐾', 4),
    (gen_random_uuid(), 'Seniors', 'seniors', '👴', 5),
    (gen_random_uuid(), 'Youth', 'youth', '👦', 6),
    (gen_random_uuid(), 'Disaster Relief', 'disaster-relief', '🆘', 7),
    (gen_random_uuid(), 'Arts & Culture', 'arts-culture', '🎨', 8),
    (gen_random_uuid(), 'Food Security', 'food-security', '🍽️', 9),
    (gen_random_uuid(), 'Housing', 'housing', '🏠', 10);
```

### organization_categories

Many-to-many: organizations ↔ categories.

```sql
CREATE TABLE organization_categories (
    organization_id     UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    category_id         UUID NOT NULL REFERENCES categories(id) ON DELETE CASCADE,
    PRIMARY KEY (organization_id, category_id)
);

CREATE INDEX idx_org_categories_category ON organization_categories(category_id);
```

### volunteer_categories

Many-to-many: volunteers ↔ interest categories.

```sql
CREATE TABLE volunteer_categories (
    volunteer_id        UUID NOT NULL REFERENCES volunteers(id) ON DELETE CASCADE,
    category_id         UUID NOT NULL REFERENCES categories(id) ON DELETE CASCADE,
    PRIMARY KEY (volunteer_id, category_id)
);

CREATE INDEX idx_vol_categories_category ON volunteer_categories(category_id);
```

### opportunities

Volunteer opportunity listings created by organizations.

```sql
CREATE TABLE opportunities (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    organization_id     UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    title               VARCHAR(200) NOT NULL,
    description         TEXT NOT NULL,
    location_address    VARCHAR(500),
    is_remote           BOOLEAN NOT NULL DEFAULT false,
    latitude            DECIMAL(10, 7),
    longitude           DECIMAL(10, 7),
    schedule_type       VARCHAR(20) NOT NULL,       -- one_time, recurring, flexible
    start_date          TIMESTAMPTZ,
    end_date            TIMESTAMPTZ,
    recurrence_desc     VARCHAR(200),               -- e.g., "Every Saturday 9am-12pm"
    volunteers_needed   INTEGER,
    time_commitment     VARCHAR(100),               -- e.g., "3 hours", "4 hours/week"
    skills_required     TEXT,
    minimum_age         INTEGER,
    status              VARCHAR(20) NOT NULL DEFAULT 'active',  -- draft, active, filled, expired, cancelled
    interest_count      INTEGER NOT NULL DEFAULT 0,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_opportunities_org ON opportunities(organization_id);
CREATE INDEX idx_opportunities_status ON opportunities(status);
CREATE INDEX idx_opportunities_dates ON opportunities(start_date, end_date);
CREATE INDEX idx_opportunities_location ON opportunities(latitude, longitude) WHERE latitude IS NOT NULL;
```

### posts

Content posts (video or image) created by organizations.

```sql
CREATE TABLE posts (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    organization_id     UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    opportunity_id      UUID REFERENCES opportunities(id) ON DELETE SET NULL,
    title               VARCHAR(100) NOT NULL,
    description         VARCHAR(500),
    media_type          VARCHAR(10) NOT NULL,        -- video, image
    status              VARCHAR(20) NOT NULL DEFAULT 'active',  -- active, removed, archived
    view_count          INTEGER NOT NULL DEFAULT 0,
    save_count          INTEGER NOT NULL DEFAULT 0,
    interest_count      INTEGER NOT NULL DEFAULT 0,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_posts_org ON posts(organization_id);
CREATE INDEX idx_posts_opportunity ON posts(opportunity_id);
CREATE INDEX idx_posts_status_created ON posts(status, created_at DESC);
CREATE INDEX idx_posts_feed ON posts(status, created_at DESC) WHERE status = 'active';
```

### post_media

Media files associated with a post (supports carousel images).

```sql
CREATE TABLE post_media (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    post_id             UUID NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    media_url           VARCHAR(500) NOT NULL,       -- Primary URL (720p for video, feed-size for image)
    thumbnail_url       VARCHAR(500),
    original_url        VARCHAR(500),
    low_res_url         VARCHAR(500),                -- 360p for video, thumbnail for image
    media_type          VARCHAR(10) NOT NULL,         -- video, image
    duration_seconds    INTEGER,                      -- Video only
    width               INTEGER,
    height              INTEGER,
    file_size_bytes     BIGINT,
    display_order       INTEGER NOT NULL DEFAULT 0,
    processing_status   VARCHAR(20) NOT NULL DEFAULT 'pending',  -- pending, processing, complete, failed
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_post_media_post ON post_media(post_id);
```

### post_tags

Tags on posts for discoverability.

```sql
CREATE TABLE post_tags (
    post_id             UUID NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    tag                 VARCHAR(50) NOT NULL,
    PRIMARY KEY (post_id, tag)
);

CREATE INDEX idx_post_tags_tag ON post_tags(tag);
```

### volunteer_interests

Volunteer expressing interest in an opportunity.

```sql
CREATE TABLE volunteer_interests (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    volunteer_id        UUID NOT NULL REFERENCES volunteers(id) ON DELETE CASCADE,
    opportunity_id      UUID NOT NULL REFERENCES opportunities(id) ON DELETE CASCADE,
    post_id             UUID REFERENCES posts(id) ON DELETE SET NULL,  -- Which post they came from
    message             VARCHAR(200),
    status              VARCHAR(20) NOT NULL DEFAULT 'pending',  -- pending, reviewed, accepted, declined
    status_updated_at   TIMESTAMPTZ,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    UNIQUE (volunteer_id, opportunity_id)
);

CREATE INDEX idx_interests_volunteer ON volunteer_interests(volunteer_id);
CREATE INDEX idx_interests_opportunity ON volunteer_interests(opportunity_id);
CREATE INDEX idx_interests_status ON volunteer_interests(status);
```

### saved_posts

Volunteer bookmarked/saved posts.

```sql
CREATE TABLE saved_posts (
    volunteer_id        UUID NOT NULL REFERENCES volunteers(id) ON DELETE CASCADE,
    post_id             UUID NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (volunteer_id, post_id)
);

CREATE INDEX idx_saved_posts_volunteer ON saved_posts(volunteer_id, created_at DESC);
```

### follows

Volunteer following an organization.

```sql
CREATE TABLE follows (
    volunteer_id        UUID NOT NULL REFERENCES volunteers(id) ON DELETE CASCADE,
    organization_id     UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (volunteer_id, organization_id)
);

CREATE INDEX idx_follows_volunteer ON follows(volunteer_id);
CREATE INDEX idx_follows_organization ON follows(organization_id);
```

### content_reports

Flagged/reported content from users.

```sql
CREATE TABLE content_reports (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    reporter_id         UUID NOT NULL,                -- Could be volunteer or org user
    reporter_type       VARCHAR(20) NOT NULL,          -- volunteer, organization
    content_type        VARCHAR(20) NOT NULL,          -- post, organization, volunteer
    content_id          UUID NOT NULL,
    reason              VARCHAR(50) NOT NULL,           -- spam, inappropriate, misleading, fraud, other
    description         VARCHAR(500),
    status              VARCHAR(20) NOT NULL DEFAULT 'pending',  -- pending, reviewed, actioned, dismissed
    reviewed_by         VARCHAR(255),                   -- Admin who reviewed
    reviewed_at         TIMESTAMPTZ,
    action_taken        VARCHAR(100),
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_reports_status ON content_reports(status);
CREATE INDEX idx_reports_content ON content_reports(content_type, content_id);
```

### notification_settings

User notification preferences.

```sql
CREATE TABLE notification_settings (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id             UUID NOT NULL,
    user_type           VARCHAR(20) NOT NULL,          -- volunteer, organization
    new_interest_email  VARCHAR(20) DEFAULT 'immediate',  -- immediate, daily, weekly, off (org only)
    interest_update     BOOLEAN DEFAULT true,           -- volunteer: notify on status change
    new_content_digest  VARCHAR(20) DEFAULT 'weekly',   -- off, weekly (volunteer only)
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    UNIQUE (user_id, user_type)
);
```

### feed_views

Track which posts a volunteer has seen (for feed deduplication).

```sql
CREATE TABLE feed_views (
    volunteer_id        UUID NOT NULL REFERENCES volunteers(id) ON DELETE CASCADE,
    post_id             UUID NOT NULL REFERENCES posts(id) ON DELETE CASCADE,
    viewed_at           TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (volunteer_id, post_id)
);

-- Partition or TTL strategy: purge views older than 30 days
CREATE INDEX idx_feed_views_volunteer ON feed_views(volunteer_id, viewed_at DESC);
```

---

## Denormalized Counters

Several tables include denormalized count fields (`follower_count`, `save_count`, `interest_count`, `view_count`) for performance. These are updated via triggers or application-level increments to avoid expensive COUNT queries on the feed.

```sql
-- Example trigger for follower_count
CREATE OR REPLACE FUNCTION update_follower_count()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        UPDATE organizations SET follower_count = follower_count + 1 WHERE id = NEW.organization_id;
    ELSIF TG_OP = 'DELETE' THEN
        UPDATE organizations SET follower_count = follower_count - 1 WHERE id = OLD.organization_id;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_follower_count
AFTER INSERT OR DELETE ON follows
FOR EACH ROW EXECUTE FUNCTION update_follower_count();
```

---

## Migration Strategy

Use EF Core migrations managed through the CLI:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Seed data for categories is applied in the initial migration. Test/demo data is managed through a separate seeding mechanism (not migrations).
