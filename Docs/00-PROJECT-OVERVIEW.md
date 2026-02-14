# SwipeForCause — Project Overview

## Vision Statement

SwipeForCause is a volunteer marketplace that connects people who want to give their time with nonprofits that need help — through the language of short-form video. Think TikTok, but every scroll brings you face-to-face with a cause worth your time.

## The Problem

Volunteering is broken from both sides:

**For volunteers:** Finding volunteer opportunities is tedious. Current platforms (VolunteerMatch, Idealist, United Way) present dry, text-heavy listings that feel like job boards. There's no emotional connection, no sense of urgency, and no way to quickly browse and discover causes that resonate. The friction from "I want to help" to "I'm signed up" is enormous.

**For nonprofits:** Recruiting volunteers is expensive and time-consuming. Nonprofits compete for attention on platforms that don't let them showcase their mission compellingly. They can't show the human side of their work — the faces, the impact, the urgency — in a way that cuts through the noise.

**The gap:** No platform combines the addictive, low-friction browsing experience of modern social media with the specific goal of connecting volunteers to causes.

## The Solution

SwipeForCause gives nonprofits the tools to create compelling short-form video and image content that showcases their volunteer needs, and gives potential volunteers a fast, engaging way to discover and commit to opportunities.

### Core Experience

1. **Volunteers** open the app and immediately see a full-screen, vertically-scrolling feed of short videos and images from nonprofits
2. Each post highlights a specific volunteer need — what the org does, what help they need, when and where
3. A prominent **"Volunteer Now"** CTA button lets users express interest with a single tap
4. Tapping the org's profile navigates to a detail page with more information, other opportunities, and org background
5. Users can **save posts** for later and **follow organizations** to see their future content

### What Makes It Different

- **Content-first discovery:** No search boxes or category trees to start. The feed does the work.
- **Emotional connection:** Video lets nonprofits show impact, not just describe it.
- **Low friction:** One tap to express interest. No lengthy applications for initial contact.
- **Mobile-first design:** Built for how people actually browse in 2026 — on their phones, in short sessions.

## Target Users

### Primary: Volunteers (Consumers)

- **Demographics:** 18-40, digitally native, socially conscious
- **Psychographics:** Want to help but don't know where to start. Feel overwhelmed by traditional volunteer platforms. Respond to visual, emotional content. Prefer quick decisions over lengthy research.
- **Behavior:** Already spend significant time on TikTok, Instagram Reels, YouTube Shorts. Comfortable with vertical scroll feeds. Make snap decisions about content worth their attention.

### Secondary: Nonprofit Organizations (Creators)

- **Size:** Small to mid-size nonprofits (5-200 staff) who struggle with volunteer recruitment
- **Pain point:** Limited marketing budget, difficulty reaching younger demographics, no platform that lets them "show" rather than "tell"
- **Need:** Simple content creation and posting tools, volunteer management, exposure to motivated potential volunteers

## Product Principles

1. **Feed first, everything else second.** The scroll experience IS the product. Everything should serve making the feed more engaging and useful.
2. **One-tap commitment.** The distance from "this looks interesting" to "I'm in" should be exactly one tap.
3. **Nonprofits are creators, volunteers are consumers.** Clear content model — orgs post, people discover. No muddying the feed with non-org content in MVP.
4. **Show, don't tell.** Video and images are first-class citizens. Text is supporting context, not the main event.
5. **Respect attention, reward curiosity.** The feed should feel worthwhile — not manipulative. Quality content from real organizations doing real work.

## Success Metrics (MVP)

| Metric | Target | Why It Matters |
|--------|--------|----------------|
| Volunteer sign-ups | 1,000 in first 3 months | Platform has demand |
| Nonprofit orgs onboarded | 50 in first 3 months | Supply side is viable |
| Feed engagement rate | >60% scroll past 5th post | Content is compelling |
| "Volunteer Now" tap rate | >5% of post views | CTA is working |
| Volunteer→Org connection rate | >30% of taps lead to confirmed contact | Funnel converts |

## MVP Scope Summary

### In Scope

- Mobile-first responsive web application
- Vertically-scrolling short-form video/image feed
- Nonprofit registration, profile creation, and content posting
- Volunteer registration and profile creation
- "Volunteer Now" CTA with interest expression flow
- Save posts and follow organizations
- Org detail pages with opportunity listings
- Basic content moderation
- Email notifications for new volunteer interest

### Out of Scope (Post-MVP)

- Native mobile apps (iOS/Android)
- In-app messaging between volunteers and orgs
- AI-powered matching and recommendations
- Volunteer hour tracking and verification
- Monetization features (paid tiers, promoted posts)
- Social features (comments, sharing, reactions)
- Volunteer-generated content
- Advanced analytics dashboard for orgs
- Background check integration
- Team/group volunteering coordination

## Technical Summary

| Layer | Technology |
|-------|-----------|
| Frontend | React (TypeScript), mobile-first responsive |
| Backend API | C# Web API (.NET 8), vertical slice architecture |
| Database | PostgreSQL |
| Authentication | Clerk (dual user types: volunteer + nonprofit) |
| File Storage | Azure Blob Storage (video/images) |
| Hosting | Azure App Service |
| Payments (future) | Stripe |

## Timeline Estimate

| Phase | Duration | Deliverables |
|-------|----------|-------------|
| Phase 1: Foundation | 3-4 weeks | Auth, database, basic API, project scaffolding |
| Phase 2: Nonprofit Portal | 3-4 weeks | Org profiles, content upload, opportunity management |
| Phase 3: Volunteer Feed | 3-4 weeks | Feed UI, scroll experience, video playback, save/follow |
| Phase 4: Connection Flow | 2-3 weeks | "Volunteer Now" flow, notifications, org dashboard |
| Phase 5: Polish & Launch | 2-3 weeks | Moderation, performance, responsive testing, soft launch |
| **Total MVP** | **~14-18 weeks** | **Full MVP ready for soft launch** |

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|-----------|
| Cold start — no content at launch | Volunteers see empty feed, leave | Pre-seed with 20-30 nonprofits before public launch. Create sample content. |
| Video hosting costs | Storage and bandwidth expenses grow fast | Use Azure Blob with CDN. Set max video length (60s). Compress aggressively. |
| Content quality | Low-quality posts make feed feel amateur | Provide creation guides for orgs. Basic quality checks on upload. |
| Nonprofit adoption | Orgs may not see value in yet-another-platform | Make posting dead simple. Highlight the unique video-first approach. Free tier. |
| Moderation at scale | Inappropriate or fraudulent content | Automated screening + manual review queue. Require org verification. |
