# SwipeForCause — Volunteer Experience PRD

## Overview

The volunteer experience is the consumer side of SwipeForCause. It needs to feel as effortless and engaging as scrolling TikTok — with the added satisfaction of discovering meaningful ways to give back. Every design decision should minimize friction between "that looks interesting" and "I'm signed up."

---

## User Persona

**Alex, 27, Software Developer.** Wants to volunteer but never gets around to it. Browsing VolunteerMatch feels like job hunting. Alex spends 2+ hours/day on TikTok and Instagram. Would volunteer more if finding opportunities was as easy as scrolling a feed. Lives in a mid-size city and would prefer local, in-person opportunities but is open to remote.

---

## Registration & Onboarding

### Registration Flow

```
1. Landing page → "Find Volunteer Opportunities" button
2. Create account (email/password, Google, or Apple sign-in via Clerk)
3. Minimal profile setup:
   - Display name *
   - City/State * (auto-detect with browser geolocation, editable)
4. Interest selection (optional but encouraged):
   - "What causes matter to you?" grid of category tiles
   - Tap to select 1-5 categories
   - "Skip for now" option (can set later in profile)
5. Land directly on the feed
```

Total time: under 60 seconds. The goal is to get people scrolling immediately.

### No-Account Browsing

For maximum top-of-funnel:
- The feed is viewable without an account (limited to 10 items, no personalization)
- "Volunteer Now" CTA prompts sign-up
- Save and follow prompt sign-up
- Soft gate: see the value before committing to registration

---

## The Feed — Core Experience

### Layout

Full-screen vertical feed. Each item occupies the entire viewport height on mobile. Content snaps to center when scrolling.

```
┌─────────────────────────────────┐
│                                 │
│                                 │
│     [VIDEO / IMAGE CONTENT]     │
│     (Full screen background)    │
│                                 │
│                                 │
│  ┌─────┐                  ┌──┐  │
│  │ 🏢  │ Org Name          │♡│  │  ← Org logo + name (tappable)
│  │ logo│ @handle           │🔖│  │  ← Right side: save, follow
│  └─────┘                  └──┘  │
│                                 │
│  Post title goes here...        │
│  Brief description truncated... │
│  #tag1 #tag2                    │
│                                 │
│  ┌─────────────────────────────┐│
│  │     🤝 Volunteer Now        ││  ← Primary CTA button
│  └─────────────────────────────┘│
│                                 │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ │  ← Scroll progress indicator
│  🏠   🔍    🔖    👤            │  ← Bottom navigation
└─────────────────────────────────┘
```

### Feed Item Components

**Background:** Video or image fills the screen. For images, use a subtle Ken Burns effect (slow zoom/pan) to add motion. For carousel images, horizontal swipe indicators (dots) at bottom of media area.

**Organization Info (bottom-left overlay):**
- Org logo (circular, 36px) — tap navigates to org profile
- Org name — tap navigates to org profile
- Verification badge (if verified)

**Action Buttons (right side, stacked vertically):**
- Save/Bookmark icon — tap to toggle save state, subtle animation on save
- Follow icon — tap to follow org, shows "Following" state if already following

**Text Overlay (bottom, over gradient):**
- Post title (bold, max 2 lines)
- Description (regular weight, max 2 lines, "...more" to expand)
- Tags (tappable, navigate to category/tag view)

**CTA Button (bottom, above nav):**
- "Volunteer Now" — full-width button, always visible
- Shows opportunity info on hover/long-press (date, location, time)
- If user already expressed interest: shows "Interest Sent ✓" (disabled state)

### Video Behavior

- Autoplay when item is >50% visible in viewport
- Pause when scrolled away
- Muted by default — tap anywhere on video to toggle audio
- Mute/unmute icon indicator appears briefly on tap
- Loop playback
- Progress bar at bottom of video area (thin, non-intrusive)
- If video fails to load: show thumbnail with play button overlay

### Image Carousel Behavior

- Horizontal swipe to navigate between images
- Dot indicators show current position
- Swipe up/down still scrolls the feed (gesture disambiguation)

### Feed Loading

- Skeleton loading state: gray placeholder rectangles matching feed layout
- Pull-to-refresh: pull down at top of feed refreshes with latest content
- Infinite scroll: next page fetched when 2 items from bottom
- Preload: start loading next video while current one plays
- Empty state (new user, no content matching interests): show broad content with message "Follow organizations and select interests to personalize your feed"

---

## "Volunteer Now" Flow

This is the primary conversion action. It must feel effortless.

### Step 1: Tap "Volunteer Now"

Bottom sheet slides up over the feed (feed is still visible but dimmed behind).

### Step 2: Confirm Interest

```
┌─────────────────────────────────┐
│                                 │
│  🤝 Volunteer at                │
│  Charleston Waterkeeper         │
│                                 │
│  ┌─────────────────────────────┐│
│  │ Beach Cleanup Day            ││
│  │ 📅 Sat, Mar 15 · 9am-12pm   ││
│  │ 📍 Folly Beach, SC           ││
│  │ ⏱️ 3 hours                   ││
│  └─────────────────────────────┘│
│                                 │
│  Add a message (optional)       │
│  ┌─────────────────────────────┐│
│  │ I'd love to help!           ││
│  └─────────────────────────────┘│
│                                 │
│  ┌─────────────────────────────┐│
│  │    ✅ Yes, I'm Interested    ││
│  └─────────────────────────────┘│
│                                 │
│  By confirming, your name and   │
│  profile will be shared with    │
│  this organization.             │
└─────────────────────────────────┘
```

### Step 3: Confirmation

Bottom sheet transitions to success state:
- Animated checkmark
- "You're in! Charleston Waterkeeper will be in touch."
- "View Opportunity Details" link
- Auto-dismisses after 3 seconds, or tap to dismiss
- Feed CTA button updates to "Interest Sent ✓"

### Edge Cases

- Post has no linked opportunity: CTA navigates to org profile instead (with prompt to browse their opportunities)
- Opportunity is filled/expired: CTA disabled with "This opportunity is no longer available"
- User already expressed interest: CTA shows "Interest Sent ✓" (no action)
- User not logged in: CTA triggers sign-up flow, then returns to this post

---

## Organization Profile Page

Accessible by tapping org name/logo from any feed item.

### Layout

```
┌─────────────────────────────────┐
│  [← Back]            [Share]    │
│                                 │
│  ┌─────────────────────────────┐│
│  │    [COVER IMAGE]            ││
│  │                             ││
│  │         ┌────┐              ││
│  │         │logo│              ││
│  │         └────┘              ││
│  └─────────────────────────────┘│
│                                 │
│  Charleston Waterkeeper ✓       │
│  🌍 Environment · 📍 Charleston │
│  234 followers                  │
│                                 │
│  [Follow] [Visit Website]       │
│                                 │
│  "Protecting Charleston's       │
│  waterways through education,   │
│  advocacy, and action."         │
│                                 │
│  ┌──────────┬──────────────────┐│
│  │  Posts   │  Opportunities   ││
│  └──────────┴──────────────────┘│
│                                 │
│  Posts Tab: 3-column grid of    │
│  thumbnails (tappable → feed    │
│  view starting at that post)    │
│                                 │
│  Opportunities Tab: List of     │
│  active opportunities with      │
│  "Volunteer Now" on each        │
└─────────────────────────────────┘
```

---

## Explore / Search

### Explore Page

Primary discovery interface beyond the feed.

**Top section:** Search bar with placeholder "Search organizations, opportunities..."

**Category Grid:** Tiles for each cause category with icons and post counts. Tapping a category shows a feed filtered to that category.

**Trending/Featured:** Optional section for highlighted organizations or seasonal campaigns (manually curated for MVP).

### Search Results

Combined results showing:
- **Organizations** — logo, name, categories, follower count, "Follow" button
- **Opportunities** — title, org name, date, location, "Volunteer Now" button

Filter chips: Category, Remote-friendly, This week, This month, Near me

---

## Saved Posts

Accessed from "Saved" tab in bottom navigation.

- Grid view of saved post thumbnails (matching org profile post grid style)
- Tap a saved post to view it full-screen (same as feed item)
- Empty state: "Save posts you're interested in and they'll show up here. Tap the bookmark icon on any post to save it."
- Can unsave from this view (long-press → "Remove from saved")

---

## Volunteer Profile

### Profile Page

```
┌─────────────────────────────────┐
│  My Profile          [Settings] │
│                                 │
│       ┌────────┐                │
│       │ Avatar │                │
│       └────────┘                │
│    Alex Thompson                │
│    📍 Charleston, SC            │
│    "Passionate about making a   │
│    difference in my community"  │
│                                 │
│  Interests:                     │
│  [🌍 Environment] [🐾 Animals]  │
│  [🍽️ Food Security]             │
│                                 │
│  [Edit Profile]                 │
│                                 │
│  ┌────────┬──────────┬────────┐ │
│  │ Saved  │Following │Activity│ │
│  └────────┴──────────┴────────┘ │
│                                 │
│  Activity Tab:                  │
│  - Interest expressions with    │
│    status (pending/accepted/    │
│    declined)                    │
│  - Shows org name, opportunity, │
│    date, current status         │
└─────────────────────────────────┘
```

### Activity Tab

Shows the volunteer's history of expressed interest:

Each item shows: Org logo + name, Opportunity title, Date expressed, Status badge (pending = yellow, accepted = green, declined = gray).

Tapping an item expands to show: the volunteer's message (if any), opportunity details, link to org profile.

---

## Settings

Accessible from profile page.

- **Notification Preferences:**
  - Interest status updates (on/off)
  - Weekly digest from followed orgs (on/off)
- **Account:**
  - Edit email
  - Change password
  - Delete account (with confirmation and data deletion notice)
- **About:** Links to Terms of Service, Privacy Policy, Help/FAQ

---

## Push Notification Strategy (Future, Post-MVP)

Not in MVP (web only, no service workers initially), but designed for:

- "You've been accepted!" — when an org accepts their interest
- "New from [Org Name]" — when a followed org posts new content
- "[Opportunity] is filling up" — urgency trigger when spots are limited
- Weekly digest: "3 new opportunities near you this week"

---

## Accessibility Requirements

- All images require alt text (org-provided or auto-generated)
- Video captions support (org-uploaded SRT/VTT files)
- Screen reader announces: org name, post title, and CTA on each feed item
- Keyboard navigation support for all interactive elements
- Focus management when bottom sheets open/close
- Reduced motion preference: disable autoplay, Ken Burns effects, animations
- Minimum 4.5:1 contrast ratio on all text overlays (ensure gradient opacity is sufficient)
