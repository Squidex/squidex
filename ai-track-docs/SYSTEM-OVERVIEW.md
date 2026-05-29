# System Overview

> **Status:** Updated — Crawl Exercise 1  
> **Chosen low-risk module:** `Squidex.Domain.Apps.Core.Model` (see justification below)

---

## What is Squidex?

Squidex is an open-source **headless CMS** with full content-API, real-time events, and a plugin system.

---

## Languages & runtimes

| Layer | Language / Runtime | Key tooling |
|-------|--------------------|-------------|
| Backend API | C# / .NET (ASP.NET Core) | `dotnet`, xUnit, NSubstitute |
| Frontend SPA | TypeScript / Angular | Angular CLI, Vitest |
| Plugin SDK | TypeScript / Preact | Webpack (preact-cli) |
| End-to-end tests | TypeScript / Node | Playwright |
| Load tests | TypeScript / k6 | k6 binary |
| Diagrams / docs | Mermaid, Markdown | — |

---

## Entry points

| Entry point | Path |
|-------------|------|
| Backend host | `backend/src/Squidex/Program.cs` |
| Backend DI / middleware | `backend/src/Squidex/Startup.cs` |
| Frontend bootstrap | `frontend/src/main.ts` |
| Frontend Angular root | `frontend/src/app/` |
| Docker image | `Dockerfile` (root) |

---

## Test approach

| Suite | Location | Count / files | Runner |
|-------|----------|---------------|--------|
| Backend unit & integration | `backend/tests/` | 7 `.csproj` projects | `dotnet test` / xUnit |
| Frontend unit | `frontend/src/**/*.spec.ts` | ~140 spec files | Vitest |
| End-to-end | `tools/e2e/` | ~36 TypeScript files | Playwright |
| Load | `tools/k6/` | k6 scripts | k6 binary |

CI entry points are in `.github/workflows/` (check root for workflow files).

---

## Top-level directory map

| Directory | Description |
|-----------|-------------|
| `backend/src/Squidex` | ASP.NET Core host — API controllers, middleware, startup |
| `backend/src/Squidex.Domain.Apps.Core.Model` | Pure domain model — value objects, aggregates, schemas |
| `backend/src/Squidex.Domain.Apps.Core.Operations` | Domain operations / processing logic |
| `backend/src/Squidex.Domain.Apps.Entities` | Entity read/write handlers, command bus |
| `backend/src/Squidex.Infrastructure` | Cross-cutting utilities — eventing, queries, tasks, logging |
| `backend/src/Squidex.Shared` | Shared constants — permission IDs, text resources (12 files) |
| `backend/src/Squidex.Data.MongoDb` | MongoDB data adapters |
| `backend/src/Squidex.Data.EntityFramework` | SQL / EF Core data adapters |
| `frontend/` | Angular SPA |
| `sdk/` | Preact-based plugin/widget SDK |
| `tools/e2e/` | Playwright end-to-end tests |
| `tools/k6/` | k6 load tests |
| `ai-track-docs/` | AI-track reference docs (this folder) |
| `.copilot-track/` | Copilot-track helper prompts & crawl evidence |

---

## Three low-risk modules

| # | Module | Why low-risk |
|---|--------|--------------|
| 1 | `Squidex.Domain.Apps.Core.Model` | Pure C# value objects / record types. No I/O, no DI, no HTTP. 112 test files provide tight safety net. Changes are confined to domain invariants. |
| 2 | `Squidex.Infrastructure` | Cross-cutting utilities (tasks, queries, timers). 96 test files. Broadly used but changes to leaf utilities are isolated. |
| 3 | `Squidex.Shared` | Tiny module (12 files): permission ID constants and `.resx` text resources. Zero runtime logic — almost impossible to break anything. |

### ✅ Chosen module: `Squidex.Domain.Apps.Core.Model`

**Justification:**
- **No I/O or external dependencies** — pure in-memory domain types; changes can never break database adapters, API routing, or authentication.
- **Rich test suite** — 112 test files in `Squidex.Domain.Apps.Core.Tests` give immediate red/green feedback.
- **Well-scoped** — sub-directories (`Apps`, `Schemas`, `Contents`, `Assets`, `Rules`, `Comments`, `Teams`) make it easy to pick a single concept to modify without touching others.
- **Reusable across exercises** — schema validation, field type models, and content value types appear in later Walk/Run exercises, so understanding this module pays forward.

---

## Key external dependencies

- MongoDB (primary datastore) or Entity Framework / SQL (alternative)
- ASP.NET Core Identity / OpenID Connect (auth)
- Angular Material (UI components)
- Vitest, Playwright, k6 (testing)

---

## TODO

- [ ] Document auth flow (OpenID Connect provider config)
- [ ] Identify environment variables required for `dotnet run` locally
- [ ] Confirm CI workflow file names in `.github/workflows/`
