# SwipeForCause — MVP Product Requirements Document

## Document Purpose

This PRD defines the complete feature set for the SwipeForCause MVP. It serves as the source of truth for what gets built in the first release. Features are organized by priority and grouped into epics.

---

## User Types

SwipeForCause has two distinct user types with separate registration flows, permissions, and experiences.

### Volunteer

A person looking to discover and sign up for volunteer opportunities. Volunteers consume content, express interest in opportunities, save posts, and follow organizations.

**Registration requires:** Name, email, location (city/state), optional profile photo, optional bio, optional interests/causes selection.

### Nonprofit Organization

A verified nonprofit that creates content, posts volunteer opportunities, and manages incoming volunteer interest.

**Registration requires:** Organization name, EIN (Employer Identification Number), contact person name and email, organization description, website URL, logo upload, location (city/state), cause categories (select up to 5).

---

## Epic 1: Authentication & User Management

### 1.1 Volunteer Registration

**User Story:** As a potential volunteer, I want to create an account quickly so I can start browsing opportunities.

**Acceptance Criteria:**
- User can register with email/password or social login (Google, Apple) via Clerk
- Minimal required fields: name, email, password, city/state
- Optional onboarding step to select interest categories (environment, education, health, animals, seniors, youth, disaster relief, arts, food security, housing)
- After registration, user lands directly on the feed
- Profile is created in the database with Clerk user ID linkage

### 1.2 Nonprofit Registration

**User Story:** As a nonprofit administrator, I want to register my organization so I can start posting volunteer opportunities.

**Acceptance Criteria:**
- Separate registration flow from volunteer sign-up
- Required fields: org name, EIN, contact person details, org description, location, at least one cause category
- Logo upload (supports JPG, PNG, max 5MB)
- Organization enters a "pending verification" state after registration
- Admin receives email confirmation
- Manual verification process (admin reviews EIN, checks legitimacy) before org can post content
- After verification, org lands on their dashboard

### 1.3 Login & Session Management

**Acceptance Criteria:**
- Clerk handles all session management
- Users are routed to the correct experience based on user type (volunteer → feed, nonprofit → dashboard)
- Session persistence across browser sessions
- Logout available from profile/settings

### 1.4 Profile Management

**Acceptance Criteria:**
- Volunteers can edit: name, bio, profile photo, location, interest categories
- Nonprofits can edit: org description, logo, contact info, cause categories, website URL
- Changes save immediately and reflect across the platform

---

## Epic 2: Content & Feed System

### 2.1 Nonprofit Content Creation

**User Story:** As a nonprofit, I want to create compelling posts with video or images so I can attract volunteers.

**Acceptance Criteria:**
- Upload short-form video (MP4, max 60 seconds, max 100MB)
- Upload images (JPG, PNG, max 10MB, up to 5 images per post in carousel)
- Required fields per post: title (max 100 chars), description (max 500 chars), associated opportunity (select from org's active opportunities)
- Optional fields: tags (max 5), location override
- Video transcoding to web-friendly format (H.264, multiple resolutions)
- Image optimization and resizing for feed display
- Preview before publishing
- Post enters "active" state immediately (unless moderation flag is triggered)
- Posts can be edited or deleted after publishing

### 2.2 Volunteer Opportunity Listing

**User Story:** As a nonprofit, I want to create volunteer opportunity listings that my content links to.

**Acceptance Criteria:**
- Create opportunity with: title, description, location (address or "remote"), date/time (one-time, recurring, or flexible), number of volunteers needed, skills/requirements, minimum age (if applicable), time commitment estimate
- Opportunities have statuses: draft, active, filled, expired, cancelled
- Multiple posts can link to the same opportunity
- Opportunity detail page shows all linked content posts
- Auto-expire opportunities past their date (for one-time events)

### 2.3 The Feed

**User Story:** As a volunteer, I want to scroll through a compelling feed of short videos and images so I can discover causes that resonate with me.

**Acceptance Criteria:**
- Full-screen vertical scroll feed (TikTok-style)
- Each feed item shows: video/image content (full screen), org name and logo (tappable, navigates to org profile), post title and truncated description, "Volunteer Now" CTA button, save/bookmark icon, follow org icon
- Videos autoplay on scroll-into-view, pause when scrolled past
- Videos are muted by default, tap to unmute
- Image carousels swipe horizontally within a feed item
- Smooth scroll snapping between feed items
- Infinite scroll with lazy loading
- Pull-to-refresh at top of feed

**Feed Algorithm (MVP — keep it simple):**
- Default sort: chronological (newest first)
- If volunteer has selected interest categories, weight those categories higher
- If volunteer follows orgs, mix followed org content with discovery content
- Deprioritize content the user has already seen
- Location proximity as a secondary signal (show local opportunities slightly higher)

### 2.4 Save Posts

**User Story:** As a volunteer, I want to save posts I'm interested in so I can come back to them later.

**Acceptance Criteria:**
- Tap bookmark icon on any feed item to save
- Saved posts accessible from profile/saved tab
- Saved posts display in reverse chronological order (most recently saved first)
- Unsave by tapping bookmark again (on feed or saved list)
- Saved count visible on post (only to the posting org, not publicly)

### 2.5 Follow Organizations

**User Story:** As a volunteer, I want to follow organizations whose mission I care about so I can see their future posts.

**Acceptance Criteria:**
- Follow button on feed items and org profile pages
- "Following" tab in volunteer profile shows list of followed orgs
- Followed org content appears with higher priority in feed
- Unfollow from feed, org profile, or following list
- Follower count visible on org profile
- Org receives notification when they gain a new follower (batched, not per-follow)

---

## Epic 3: Connection Flow

### 3.1 "Volunteer Now" Expression of Interest

**User Story:** As a volunteer, I want to quickly express interest in an opportunity so the nonprofit knows I want to help.

**Acceptance Criteria:**
- Tapping "Volunteer Now" on a feed item opens a lightweight bottom sheet/modal
- Bottom sheet shows: opportunity summary (title, date, location, time commitment), org name, simple confirmation message ("Let [Org Name] know you're interested?")
- Optional: short message field (max 200 chars) for the volunteer to include a note
- Single tap to confirm interest
- After confirmation: visual feedback (animation/checkmark), button state changes to "Interest Sent"
- Volunteer cannot express interest twice for the same opportunity
- Interest is recorded in the database with timestamp

### 3.2 Nonprofit Volunteer Interest Dashboard

**User Story:** As a nonprofit, I want to see which volunteers are interested in my opportunities so I can follow up with them.

**Acceptance Criteria:**
- Dashboard tab showing all incoming volunteer interest
- List view with: volunteer name and photo, opportunity they're interested in, date of interest expression, optional message from volunteer, status (new, reviewed, accepted, declined)
- Org can update status of each interest
- Org can view volunteer's profile (name, bio, location, interests)
- Bulk actions: mark multiple as reviewed

### 3.3 Email Notifications

**Acceptance Criteria:**
- Nonprofit receives email when new volunteer interest comes in (configurable: immediate, daily digest, weekly digest)
- Volunteer receives email when their interest status changes (accepted/declined)
- Volunteer receives email when a followed org posts new content (weekly digest only)
- All notifications can be toggled on/off in settings
- Emails are transactional, not marketing (use SendGrid or similar)

---

## Epic 4: Organization Profile & Discovery

### 4.1 Organization Profile Page

**User Story:** As a volunteer, I want to view an organization's full profile so I can decide if I want to volunteer with them.

**Acceptance Criteria:**
- Accessible by tapping org name/logo from feed or search
- Profile displays: org logo and cover image, org name and description, cause categories (as tags), location, website link, follower count, verification badge
- Tabs on profile: "Posts" (grid view of all content), "Opportunities" (list of active volunteer opportunities)
- Follow/unfollow button
- "Volunteer Now" accessible from individual opportunity cards on the profile

### 4.2 Explore / Search

**User Story:** As a volunteer, I want to search for specific types of opportunities or organizations.

**Acceptance Criteria:**
- Explore page accessible from bottom navigation
- Category browsing: tiles for each cause category showing post count
- Search bar: searches org names, opportunity titles, descriptions, locations
- Search results show mix of orgs and opportunities
- Filter options: cause category, location/distance, date range, remote-friendly

---

## Epic 5: Volunteer Profile

### 5.1 Volunteer Profile Page

**User Story:** As a volunteer, I want a profile that shows my activity and saved content.

**Acceptance Criteria:**
- Profile displays: name, photo, bio, location, interest categories
- Tabs: "Saved" (saved posts), "Following" (followed orgs), "Activity" (history of interest expressions with statuses)
- Edit profile accessible from profile page
- Settings accessible from profile page (notifications, account, logout)

---

## Epic 6: Moderation & Trust

### 6.1 Nonprofit Verification

**Acceptance Criteria:**
- New nonprofits enter "pending" state
- Admin panel to review pending orgs (EIN lookup, website check, basic legitimacy)
- Approved orgs get verification badge on profile
- Rejected orgs receive email with reason
- Only verified orgs can publish content to the feed

### 6.2 Content Moderation

**Acceptance Criteria:**
- Automated screening on upload (file type validation, basic content checks)
- Report button on all content and profiles (volunteers can flag inappropriate content)
- Admin moderation queue for reported content
- Content removal with notification to org
- Repeat offenders: warning → suspension → ban escalation

---

## Navigation Structure (Mobile-First)

Bottom navigation bar with 4 tabs:

| Tab | Icon | Destination |
|-----|------|-------------|
| Home | 🏠 | The Feed |
| Explore | 🔍 | Search & Categories |
| Saved | 🔖 | Saved Posts |
| Profile | 👤 | Volunteer Profile / Org Dashboard |

Nonprofits see a different navigation:

| Tab | Icon | Destination |
|-----|------|-------------|
| Dashboard | 📊 | Volunteer Interest & Stats |
| Create | ➕ | New Post / Opportunity |
| Content | 📱 | Manage Posts & Opportunities |
| Profile | 👤 | Org Profile & Settings |

---

## Non-Functional Requirements

### Performance
- Feed loads first item within 1.5 seconds on 4G
- Video begins playback within 500ms of scroll-into-view
- API response times under 200ms for feed requests
- Support 10,000 concurrent users at launch

### Accessibility
- WCAG 2.1 AA compliance
- Screen reader support for all interactive elements
- Captions/subtitles support for videos (org-provided)
- Sufficient color contrast ratios

### Security
- All API endpoints require authentication (except public org profiles and feed preview)
- Rate limiting on all endpoints
- File upload scanning for malware
- Input sanitization on all user-generated content
- HTTPS everywhere

### Responsive Design
- Primary: mobile (375px-428px)
- Secondary: tablet (768px-1024px)
- Tertiary: desktop (1024px+)
- Feed experience optimized for mobile viewport
