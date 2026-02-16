# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

ScrollForCause — a TikTok-style volunteer marketplace connecting nonprofits with volunteers. Landing page is live; MVP is under active development across 36 GitHub Issues.

## Commands

### Backend (C# .NET 8 API) — from `src/api/`

```bash
dotnet build                                    # Build solution
dotnet run --project ScrollForCause.Api          # Run API (localhost:5000)
dotnet watch run --project ScrollForCause.Api    # Run with hot reload
dotnet test                                     # Run all xUnit tests
dotnet test --filter "FullyQualifiedName~ClassName.MethodName"  # Single test

# EF Core migrations (run from src/api/)
dotnet ef migrations add <Name> --project ScrollForCause.Api
dotnet ef database update --project ScrollForCause.Api
```

### Frontend (React + TypeScript) — from `src/web/`

```bash
npm ci                  # Install dependencies
npm run dev             # Vite dev server
npm run build           # TypeScript check + Vite build
npm run lint            # ESLint
npm test                # Vitest (single run)
npm run test:watch      # Vitest (watch mode)
```

Frontend tests are co-located: `Component.test.tsx` next to `Component.tsx`.

### Landing page — `landing/`

Static HTML, no build step. Deployed via Azure Static Web Apps workflow.

## Architecture

### Backend — Vertical Slice Architecture

Each feature is a **single file** containing Request, Response, Handler, and Validator. No layered architecture, no interfaces until needed.

```
src/api/ScrollForCause.Api/
├── Program.cs                 # Minimal API setup (EF Core, FluentValidation, CORS, Swagger)
├── Features/                  # One folder per domain area, one file per slice
├── Database/
│   ├── AppDbContext.cs        # EF Core DbContext with full entity configuration
│   ├── Entities/              # Entity classes (GUIDs, UTC timestamps, soft deletes via IsActive)
│   └── Migrations/
└── Common/                    # PagedResponse (cursor-based), ErrorResponse, GlobalExceptionHandler
```

Naming: `CreatePostRequest` / `CreatePostResponse` (not DTOs). Pagination is cursor-based (no offset). `public partial class Program { }` at the bottom of Program.cs enables integration test access.

### Frontend

```
src/web/src/
├── App.tsx              # QueryClientProvider + RouterProvider
├── index.css            # Tailwind v4 CSS-based config + design tokens as CSS custom properties
├── routes/index.tsx     # Route definitions
├── api/client.ts        # Fetch wrapper (base URL from VITE_API_URL)
├── components/shared/
├── hooks/
└── types/index.ts       # Shared TypeScript types mirroring API responses
```

Tailwind v4 with `@tailwindcss/vite` plugin — no `tailwind.config.js` or PostCSS config. Design tokens are CSS custom properties in `index.css`.

### Design Tokens

| Token | Value |
|-------|-------|
| Coral | `#FF6B4A` |
| Teal | `#0A9B8E` |
| Navy | `#1A1A2E` |
| Cream | `#FDF8F4` |
| Display font | Fraunces |
| Body font | DM Sans |

Mobile-first: primary viewport 375px–428px.

## Code Style

- **Backend:** Nullable enabled, implicit usings, minimal API pattern
- **Frontend:** Strict TypeScript, Prettier (single quotes, semicolons, trailing commas, 100 char width, 2-space indent)

## Database

PostgreSQL via EF Core. Dev connection: `Host=localhost;Database=swipeforcause;Username=postgres;Password=postgres`

All entities use GUIDs for IDs, UTC ISO 8601 timestamps, and soft deletes (`IsActive` flag).

## Git Workflow

- Feature work in **git worktrees** at `/c/Code/SwipeForCause-wt-{issueNumber}`
- **Squash merge only** — merge commits and rebase merging are disabled
- Auto-delete head branches is enabled on GitHub
- After merge: pull main → remove worktree → delete local branch → rebase remaining feature branches
- GitHub CLI at `/c/Program Files/GitHub CLI` (add to PATH in bash)

## Documentation

Comprehensive specs live in `docs/` (PRD, technical architecture, database schema, API endpoints, moderation, go-to-market). Decision log at `docs/decisions.md`.
