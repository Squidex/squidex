# Build & Test Reference

> **Last verified:** Crawl Exercise 2 (2026-05-29, .NET 10, macOS arm64)

---

## Chosen module: `Squidex.Domain.Apps.Core.Model`

All commands below are run from the **`backend/`** directory unless stated otherwise.

---

## Backend build

```bash
cd backend

# Full solution restore + build
dotnet restore Squidex.slnx
dotnet build Squidex.slnx

# Build only the chosen module (fast)
dotnet build src/Squidex.Domain.Apps.Core.Model/Squidex.Domain.Apps.Core.Model.csproj
```

Expected output: `Build succeeded. 0 Warning(s) 0 Error(s)`

---

## Backend tests — chosen module only

```bash
cd backend

# Run all tests in the Core model test project
dotnet test tests/Squidex.Domain.Apps.Core.Tests/Squidex.Domain.Apps.Core.Tests.csproj

# Run a single test by name filter
dotnet test tests/Squidex.Domain.Apps.Core.Tests/Squidex.Domain.Apps.Core.Tests.csproj \
  --filter "Should_enumerate_all_configured_language_keys"
```

Expected output: `Passed! - Failed: 0, Passed: N, Skipped: 0`

---

## Backend tests — full suite

```bash
cd backend
dotnet test tests/tests.sln
```

---

## Frontend (Angular)

```bash
cd frontend
npm ci
npm run build          # production build
npm test               # Vitest unit tests (headless)
```

---

## End-to-end (Playwright)

```bash
cd tools/e2e
npm ci
npx playwright test
```

---

## Docker / local stack

```bash
# Build image from repo root
docker build -t squidex:local .
```

---

## TODO

- [ ] Document env vars needed for `dotnet run` (MongoDB connection string etc.)
- [ ] Add seed-data / migration steps for local dev
- [ ] Link to CI workflow files once confirmed
