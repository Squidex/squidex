# System Overview

> **Status:** Placeholder — fill in during the Crawl phase.

## What is Squidex?

Squidex is an open-source headless CMS built on ASP.NET Core (backend) and Angular (frontend).

## Top-level directories

| Directory | Description |
|-----------|-------------|
| `backend/` | ASP.NET Core solution — domain, infrastructure, API |
| `frontend/` | Angular SPA |
| `sdk/` | Preact-based plugin/widget SDK |
| `tools/e2e/` | Playwright end-to-end tests |
| `tools/k6/` | k6 load tests |
| `ai-track-docs/` | AI-track reference docs (this folder) |
| `.copilot-track/` | Copilot-track helper prompts & crawl evidence |

## Key external dependencies

- MongoDB (primary datastore) or Entity Framework (SQL alternative)
- ASP.NET Core Identity / OpenID Connect
- Angular CLI, Vitest (unit), Playwright (e2e)

## TODO — complete during Crawl

- [ ] List major bounded contexts / domain aggregates
- [ ] Document auth flow
- [ ] Note any feature flags or environment variables required for local dev
