# Build & Test Reference

> **Status:** Placeholder — verify commands during the Crawl phase and update.

## Backend (ASP.NET Core)

```bash
# Restore & build
cd backend
dotnet restore Squidex.slnx
dotnet build Squidex.slnx

# Run unit tests
dotnet test tests/tests.sln

# Run with coverage
cd tests && pwsh RunCoverage.ps1
```

## Frontend (Angular)

```bash
cd frontend
npm ci
npm run build
npm test
```

## End-to-end (Playwright)

```bash
cd tools/e2e
npm ci
npx playwright test
```

## Docker / local stack

```bash
docker build -t squidex:local .
```

## TODO — complete during Crawl

- [ ] Confirm env vars needed for `dotnet run`
- [ ] Add any seed-data or migration steps
- [ ] Note CI pipeline entry points
