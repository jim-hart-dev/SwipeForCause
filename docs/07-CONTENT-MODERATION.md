# SwipeForCause — Content & Moderation Strategy

## Overview

SwipeForCause has a unique advantage over general social media platforms: the content model is constrained. Only verified nonprofits can post content, and all content is related to volunteer recruitment. This dramatically reduces the surface area for abuse, but moderation is still essential for trust and safety.

---

## Content Model

### Who Can Post

Only verified nonprofit organizations. This is the single most important moderation decision in the product — the verification gate filters out the majority of potential bad actors.

### What Gets Posted

- Short-form videos (max 60 seconds) showcasing volunteer opportunities, org missions, or impact stories
- Images (single or carousel up to 5) related to volunteer opportunities
- Each post has text fields: title (100 chars), description (500 chars), tags (5 max)

### What Doesn't Belong

- Content unrelated to volunteering, nonprofits, or community service
- Political campaigning or partisan content (nonprofits doing nonpartisan civic engagement is fine)
- Commercial advertising or fundraising (this is a volunteer platform, not a donation platform)
- Content featuring minors without appropriate context and consent indicators
- Anything violent, hateful, sexually explicit, or illegal

---

## Three Lines of Defense

### Line 1: Prevention (Before Content Enters the System)

**Organization Verification**
- Every org must be verified before posting
- EIN validation against IRS database
- Website cross-reference
- Manual review by admin
- Estimated rejection rate: 5-10% (mostly incomplete applications, occasional fraud)

**Upload Restrictions**
- File type validation (only MP4, MOV, JPG, PNG accepted)
- File size limits (100MB video, 10MB image)
- Video duration limit (60 seconds)
- Rate limiting on uploads (max 5 posts/day per org)

**Text Validation**
- Profanity filter on titles, descriptions, and messages
- URL detection in text fields (block or flag external links in descriptions)
- Character limits enforced

### Line 2: Detection (Automated Screening)

**MVP approach — keep it simple and manual-heavy:**

For MVP, automated screening is lightweight. The verification gate does most of the heavy lifting.

- **Image scanning:** Use Azure Content Moderator API (or similar) to flag potentially inappropriate images (adult content, violence, gore)
- **Video scanning:** Extract keyframes at 5-second intervals, run through image content moderator
- **Text screening:** Regex-based filter for known slurs, hate speech terms, and scam indicators
- **Flagged content goes to manual review queue** — not auto-removed (to avoid false positives blocking legitimate orgs)

**Post-MVP automated screening:**
- AI-based video content analysis
- Duplicate/near-duplicate content detection
- Sentiment analysis on text
- Fake org detection patterns

### Line 3: Community Reporting (Reactive)

**Report Button**
- Available on every post and every org profile
- Volunteers can report content they find problematic
- Report flow: tap report → select reason → optional description → submit

**Report Reasons:**
- Spam or misleading content
- Inappropriate or offensive content
- Fraudulent organization
- Not related to volunteering
- Other (requires description)

**Report Processing:**
- Reports enter the admin moderation queue
- Multiple reports on the same content increase priority
- Threshold: 3+ unique reports auto-hides content pending review
- Reporter is never revealed to the org

---

## Moderation Workflow

### Admin Moderation Queue

Accessible to admin users via `/admin/moderation`.

**Queue displays:**
- Content thumbnail/preview
- Report reason(s) and count
- Organization name and verification status
- Date posted
- Number of views (to assess exposure)

**Sorted by:** Report count (descending), then date (newest first)

**Admin Actions:**

| Action | Effect |
|--------|--------|
| Dismiss | Content is fine, clear reports, no action |
| Warn | Content stays up, org receives warning email about guidelines |
| Remove | Content removed from feed, org notified with reason |
| Suspend Org | All org content hidden, org cannot post, receives notice |
| Ban Org | Permanent removal, all content deleted, account deactivated |

### Escalation Path

```
First offense:  Warning email with specific guideline violated
Second offense: Content removed + formal warning
Third offense:  7-day posting suspension
Fourth offense: Permanent ban
```

Severe violations (hate speech, fraud, illegal content) skip to immediate suspension pending review.

---

## Content Guidelines for Nonprofits

Published on the platform and referenced during onboarding. Written in plain language.

### Do Post

- Videos and photos showcasing your volunteer opportunities
- Content showing the impact of volunteer work
- Behind-the-scenes looks at your organization's mission
- Calls for specific volunteer help (events, ongoing needs, skills-based)
- Stories from past volunteers (with their permission)
- Educational content about your cause area

### Don't Post

- Content unrelated to your organization's mission or volunteer needs
- Political endorsements or partisan messaging
- Fundraising appeals or donation requests (this platform is for volunteer recruitment)
- Content from other organizations (post only your own)
- Stock photos or generic content not related to your actual work
- Content with copyrighted music (use royalty-free audio)
- Anything violent, hateful, discriminatory, or sexually explicit

### Best Practices

- Film vertically (9:16) for the best feed experience
- Keep videos under 30 seconds — shorter performs better
- Show real people and real places (authenticity builds trust)
- Clearly state what you need, when, and where
- Use good lighting and clear audio
- Post consistently — aim for 2-3 posts per week

---

## Org Verification SLA

| Step | Timeline |
|------|----------|
| Org submits application | Day 0 |
| Admin reviews application | Within 48 hours |
| Decision communicated | Within 48 hours of review |
| Rejected org can reapply | After 7 days |

Target: 95% of applications reviewed within 48 hours.

---

## Privacy & Data Handling

- Volunteer profiles are only visible to orgs after the volunteer expresses interest
- Volunteer email is only shared with an org after the org accepts their interest
- Report submissions are anonymous (org never sees who reported them)
- Deleted content is removed from the feed immediately, purged from storage within 30 days
- Deactivated org content is hidden immediately, data retained for 90 days per policy

---

## Metrics to Track

| Metric | Purpose |
|--------|---------|
| Reports per day | Overall content health |
| Reports per post (average) | Baseline for flagging thresholds |
| False positive rate (dismissed reports / total) | Tune automated systems |
| Time from report to resolution | Admin responsiveness |
| Org warning/suspension/ban rate | Platform trustworthiness |
| Verification approval rate | Funnel health |
| Verification time (submission to decision) | SLA compliance |
