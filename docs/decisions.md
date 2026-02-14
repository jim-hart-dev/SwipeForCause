# Decision Log

Major architectural and workflow decisions for the SwipeForCause project.

| # | Date | Decision | Context | Alternatives Considered |
|---|------|----------|---------|------------------------|
| 1 | 2026-02-14 | Solution file (`SwipeForCause.slnx`) lives at `src/api/`, not repo root | Keeps .NET artifacts scoped to the API folder, consistent with `src/web/` having its own `package.json` | Repo root (rejected — pollutes top-level with .NET-specific files) |
| 2 | 2026-02-14 | Squash merge only for PRs | Clean linear history on main; one commit per feature | Merge commits (noisy history), rebase merge (loses PR association) |
| 3 | 2026-02-14 | No formal PR approval required | Solo developer; same GitHub account creates and reviews PRs. User reviews diff on GitHub and clicks "Squash and merge" directly. | Bot account for Claude (extra account overhead), GitHub Actions auto-merge via labels (unnecessary complexity) |
| 4 | 2026-02-14 | Auto-delete head branches enabled | Keeps remote clean after merge; no manual branch cleanup on GitHub | Manual deletion (easy to forget, stale branches accumulate) |
| 5 | 2026-02-14 | Feature work uses git worktrees | Parallel development across issues without branch switching; each worktree at `/c/Code/SwipeForCause-wt-{N}` | Feature branches with `git switch` (context switching overhead, can't work in parallel) |
