# SwipeForCause — Nonprofit Portal PRD

## Overview

The Nonprofit Portal is the org-facing side of SwipeForCause. It's where nonprofits register, create content, manage volunteer opportunities, and handle incoming volunteer interest. The portal must be simple enough that a non-technical program manager can use it without training.

---

## User Persona

**Maria, Program Manager** at a mid-size animal rescue nonprofit. She manages volunteer scheduling, oversees social media, and handles community outreach. She's comfortable with basic tech (Instagram, Canva, Google Sheets) but has no coding or design background. She has 30 minutes a week to dedicate to volunteer recruitment platforms.

---

## Registration & Onboarding

### Registration Flow

```
1. Landing page → "I'm a Nonprofit" button
2. Create account (email/password or Google SSO via Clerk)
3. Organization Details form:
   - Organization name *
   - EIN (Employer Identification Number) *
   - Organization description * (with character count, max 2000)
   - Contact person name *
   - Contact person email *
   - Website URL
   - City/State *
   - Select cause categories (2-5 required) *
4. Upload logo (drag-and-drop or file picker, crop tool)
5. Confirmation page: "Your organization is under review. We'll email you within 48 hours."
```

### Verification Process

Verification happens behind the scenes after registration:

1. Admin receives notification of new org registration
2. Admin checks: EIN validity (IRS database), website exists and matches org, description is legitimate
3. Admin marks as **Verified** or **Rejected** (with reason)
4. Org receives email notification with result
5. Verified orgs can immediately start creating content
6. Rejected orgs can reapply after addressing issues

### First-Time Setup Guide

After verification, the org sees a guided setup checklist:

- ✅ Account created
- ✅ Organization verified
- ☐ Upload a cover image for your profile
- ☐ Create your first volunteer opportunity
- ☐ Create your first post (video or image)

Each item links directly to the relevant action. Checklist dismisses after all items are complete.

---

## Dashboard

The dashboard is the org's home screen after login. It provides an at-a-glance view of their SwipeForCause presence.

### Dashboard Layout

```
┌─────────────────────────────────────────────────┐
│  Welcome back, Charleston Waterkeeper            │
│                                                   │
│  ┌───────────┐ ┌───────────┐ ┌───────────┐      │
│  │ New        │ │ Active    │ │ Total     │      │
│  │ Interests  │ │ Opps      │ │ Followers │      │
│  │    12      │ │    3      │ │    234    │      │
│  └───────────┘ └───────────┘ └───────────┘      │
│                                                   │
│  Recent Volunteer Interest          [View All →]  │
│  ┌─────────────────────────────────────────────┐ │
│  │ Jim D. → Beach Cleanup Day    2 hours ago   │ │
│  │ Sarah K. → Food Drive         yesterday     │ │
│  │ Mike R. → Beach Cleanup Day   2 days ago    │ │
│  └─────────────────────────────────────────────┘ │
│                                                   │
│  Your Posts (Last 7 Days)         [View All →]   │
│  ┌─────────────────────────────────────────────┐ │
│  │ 🎬 "Help us clean the waterfront!" 1.2K views│ │
│  │ 🖼️ "Meet the turtles we saved"    800 views │ │
│  └─────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────┘
```

### Stats Cards

- **New Interests:** Unreviewed volunteer interest expressions (links to interest management)
- **Active Opportunities:** Number of currently active volunteer opportunities
- **Total Followers:** Organization follower count

---

## Content Creation

### Creating a Post

The post creation flow is designed to be as quick as posting to Instagram.

**Step 1: Upload Media**
- Drag-and-drop zone or "Choose File" button
- Supports video (MP4, MOV, max 60s, max 100MB) or images (JPG, PNG, max 10MB)
- For images: upload up to 5 for a carousel
- Upload progress bar with estimated time
- Video preview plays inline after upload
- Image preview with reorder drag handles for carousel

**Step 2: Add Details**
- Title (required, max 100 chars) with character counter
- Description (optional, max 500 chars) with character counter
- Link to Opportunity: dropdown of org's active opportunities (optional but encouraged)
- Tags: free-text input, max 5 tags, autocomplete from popular tags

**Step 3: Preview**
- Shows exactly how the post will appear in the feed
- Mobile viewport preview (even on desktop)
- "Publish" button and "Save as Draft" option

**Post States:**
- `draft` — saved but not published
- `processing` — media is being transcoded (video only, usually 1-3 minutes)
- `active` — live in the feed
- `archived` — removed from feed by org, still visible in org's content management
- `removed` — removed by moderation

### Content Guidelines (Shown During Creation)

A collapsible "Tips for Great Posts" section:

- Keep videos under 30 seconds for best engagement
- Show real people and real impact — authenticity wins
- State clearly what help you need and when
- Film vertically (9:16 ratio) for best feed display
- Use good lighting and clear audio
- Include a call to action in your video

---

## Opportunity Management

### Creating an Opportunity

**Required fields:**
- Title (max 200 chars)
- Description (rich text, max 5000 chars)
- Location: address input with geocoding OR "This is a remote opportunity" toggle
- Schedule Type: One-time / Recurring / Flexible
  - One-time: date picker + start/end time
  - Recurring: description field ("Every Saturday 9am-12pm")
  - Flexible: no date fields, just time commitment
- Time Commitment: free text ("3 hours", "4 hours/week")

**Optional fields:**
- Number of volunteers needed
- Skills or requirements
- Minimum age
- What to bring / what to wear

### Opportunity List View

Table showing all opportunities with columns: Title, Status, Schedule, Volunteers Interested, Created Date.

Sortable by any column. Filterable by status.

Bulk actions: Archive, Reactivate.

### Opportunity Status Management

- **Draft** → **Active**: Publish button
- **Active** → **Filled**: When enough volunteers confirmed, or manually
- **Active** → **Expired**: Auto-triggered when one-time event date passes
- **Active** → **Cancelled**: Manual cancellation (sends notification to interested volunteers)
- Any → **Archived**: Soft delete, can be reactivated

---

## Volunteer Interest Management

### Interest List

This is the most operationally critical screen for nonprofits — it's where they connect with potential volunteers.

**List view with columns:**
- Volunteer name + avatar (clickable → volunteer profile)
- Opportunity title
- Message (if provided)
- Date submitted
- Status badge (pending / reviewed / accepted / declined)

**Filters:** Status, Opportunity, Date range

**Actions per interest:**
- **Review** — mark as seen (changes from pending → reviewed)
- **Accept** — confirm the volunteer (triggers email to volunteer)
- **Decline** — decline with optional reason (triggers email to volunteer)

**Bulk actions:** Select multiple → Mark as Reviewed, Accept All, Decline All

### Volunteer Profile View (From Org Perspective)

When an org clicks on a volunteer's name, they see:
- Display name, avatar, city/state
- Bio
- Interest categories
- Number of interest expressions made on SwipeForCause (social proof)
- Note: orgs do NOT see the volunteer's email until they accept the interest (privacy protection)

### After Accepting

When an org accepts a volunteer's interest:
- Volunteer receives email with: org name, opportunity details, contact email of the org
- Org receives the volunteer's email address
- From here, coordination happens via email (no in-app messaging for MVP)

---

## Content Management

### Posts List

Grid or list view of all published and draft posts.

Each post card shows: thumbnail, title, status badge, view count, save count, interest conversions, date.

Actions: Edit, Archive, Delete (with confirmation).

### Post Analytics (Per Post)

Simple metrics for each post:
- Views
- Saves
- "Volunteer Now" taps from this post
- Posted date

No complex analytics for MVP — just these core numbers.

---

## Organization Profile Management

### Editable Fields

- Organization name
- Description
- Logo (re-upload with crop)
- Cover image (recommended 1200x400, with crop)
- Website URL
- City/State
- Cause categories (2-5)
- Contact person name and email

### Profile Preview

"Preview Profile" button shows how the public-facing org profile page looks to volunteers.

---

## Settings

- **Notification Preferences:** New interest alerts (immediate/daily/weekly/off), new follower digest (weekly/off)
- **Account:** Change password, update contact email
- **Team Access (future):** Invite team members to manage the org account
- **Danger Zone:** Deactivate organization (removes all content from feed, preserves data for 90 days)
