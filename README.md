# 📋 SwipeForCause — Documentation Index

> **TikTok for Volunteering** — A short-form video marketplace connecting volunteers with nonprofits.

---

## Documents

| # | Document | Description |
|---|----------|-------------|
| 00 | [Project Overview](./docs/00-PROJECT-OVERVIEW.md) | Vision, problem statement, target users, success metrics, scope, timeline, and risks |
| 01 | [MVP Product Requirements](./docs/01-MVP-PRD.md) | Complete feature specifications organized by epic, navigation structure, non-functional requirements |
| 02 | [Technical Architecture](./docs/02-TECHNICAL-ARCHITECTURE.md) | JimStack architecture (C# + React + PostgreSQL + Azure), project structure, media pipeline, infrastructure, costs |
| 03 | [Database Schema](./docs/03-DATABASE-SCHEMA.md) | Full PostgreSQL schema with all tables, indexes, relationships, seed data, and triggers |
| 04 | [API Endpoints](./docs/04-API-ENDPOINTS.md) | Complete REST API specification with request/response examples for all endpoints |
| 05 | [Nonprofit Portal PRD](./docs/05-NONPROFIT-PORTAL-PRD.md) | Org-side experience: registration, dashboard, content creation, opportunity management, volunteer interest handling |
| 06 | [Volunteer Experience PRD](./docs/06-VOLUNTEER-EXPERIENCE-PRD.md) | Consumer-side experience: feed UX, "Volunteer Now" flow, search, profiles, accessibility |
| 07 | [Content & Moderation](./docs/07-CONTENT-MODERATION.md) | Three-line defense strategy, moderation workflow, content guidelines, org verification SLA |
| 08 | [Go-to-Market Playbook](./docs/08-GO-TO-MARKET.md) | Cold start strategy, pre-launch seeding, launch plan, growth tactics, city expansion, monetization signals |

---

## Tech Stack Summary

| Layer | Technology |
|-------|-----------|
| Frontend | React + TypeScript + Tailwind + Vite |
| Backend | C# Web API (.NET 8), Vertical Slice Architecture |
| Database | PostgreSQL (Azure Flexible Server) |
| Auth | Clerk |
| Storage | Azure Blob Storage + CDN |
| Hosting | Azure App Service + Azure Static Web Apps |
| Email | SendGrid |
| Payments (future) | Stripe |

## Architecture Conventions

- Vertical slice architecture (not layered)
- `*Request` / `*Response` naming (not DTOs)
- Concrete classes (no interfaces until needed)
- Minimal folder nesting
- Cursor-based pagination
- GUIDs for all IDs
