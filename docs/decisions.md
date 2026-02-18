# Decision Log

Major architectural and workflow decisions for the ScrollForCause project.

| # | Date | Decision | Context | Alternatives Considered |
|---|------|----------|---------|------------------------|
| 1 | 2026-02-14 | Solution file (`ScrollForCause.slnx`) lives at `src/api/`, not repo root | Keeps .NET artifacts scoped to the API folder, consistent with `src/web/` having its own `package.json` | Repo root (rejected — pollutes top-level with .NET-specific files) |
| 2 | 2026-02-14 | Squash merge only for PRs | Clean linear history on main; one commit per feature | Merge commits (noisy history), rebase merge (loses PR association) |
| 3 | 2026-02-14 | No formal PR approval required | Solo developer; same GitHub account creates and reviews PRs. User reviews diff on GitHub and clicks "Squash and merge" directly. | Bot account for Claude (extra account overhead), GitHub Actions auto-merge via labels (unnecessary complexity) |
| 4 | 2026-02-14 | Auto-delete head branches enabled | Keeps remote clean after merge; no manual branch cleanup on GitHub | Manual deletion (easy to forget, stale branches accumulate) |
| 5 | 2026-02-14 | Feature work uses git worktrees | Parallel development across issues without branch switching; each worktree at `/c/Code/ScrollForCause-wt-{N}` | Feature branches with `git switch` (context switching overhead, can't work in parallel) |

## GitHub Secrets & Variables

Secrets and variables configured in **Settings → Secrets and variables → Actions**.

### Secrets

| Secret | Workflow | Purpose | How to Rotate |
|--------|----------|---------|---------------|
| `AZURE_WEBAPP_PUBLISH_PROFILE` | `api.yml` | Authenticates deploy to Azure App Service `app-sfc-api-prod` | Azure Portal → App Service → Deployment Center → Manage publish profile → Reset → Download → Update secret |
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | `web.yml` | Authenticates deploy of React frontend to Azure Static Web Apps | Azure Portal → Static Web App (frontend) → Manage deployment token → Reset → Update secret |
| `AZURE_STATIC_WEB_APPS_API_TOKEN_JOLLY_FLOWER_0EEE7CC10` | `azure-static-web-apps-orange-sky-0acc5c30f.yml` | Authenticates deploy of landing page to Azure Static Web Apps | Azure Portal → Static Web App (landing) → Manage deployment token → Reset → Update secret |

> `AZURE_STATIC_WEB_APPS_API_TOKEN_AGREEABLE_ISLAND_04C7FA710` was an orphaned secret from a deleted Azure resource. Deleted 2026-02-16.

### Variables

| Variable | Workflow | Purpose |
|----------|----------|---------|
| `VITE_API_URL` | `web.yml` | API base URL injected at build time (e.g., `https://app-sfc-api-prod.azurewebsites.net/api/v1`) |
| `VITE_CLERK_PUBLISHABLE_KEY` | `web.yml` | Clerk publishable key injected at build time for frontend auth |
