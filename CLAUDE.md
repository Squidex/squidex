# Squidex

Headless CMS. Angular frontend in `frontend/`, ASP.NET Core backend in `backend/`.

## Frontend

- Angular app in `frontend/`, source under `src/app`:
  - `framework/` — generic, reusable UI components and utilities (no domain knowledge).
  - `shared/` — Squidex-specific services, state stores and components.
  - `features/` — the actual screens (apps, assets, content, rules, schemas, settings, teams, ...).
  - `shell/` — app frame, navigation, layout.
- State is handled with the state store pattern from `framework/state.ts` (immutable value objects + `State<T>` subclasses), not with a third-party store library.
- Commands:

```bash
npm start
```

```bash
npm test
```

```bash
npm run lint
```

### Best Practices

- i18n texts live in `backend/i18n`, translations are generated into the frontend — do not edit generated translation files by hand.
- Do not write JsDoc comments.

## Backend

- .NET solution `backend/Squidex.slnx`. Projects under `backend/src`, tests under `backend/tests`, optional integrations under `backend/extensions`.
- Layering: `Squidex.Infrastructure` (generic building blocks) → `Squidex.Domain.Apps.*` (core model, operations, events, entities) → `Squidex.Web` / `Squidex` (API host).
- Event-sourced domain: aggregates emit events from `Squidex.Domain.Apps.Events`, state is projected into MongoDB or EF Core (`Squidex.Data.MongoDb`, `Squidex.Data.EntityFramework`).
- Run tests with the filter below — some tests need external setup (real databases, Docker/Testcontainers) and will fail without it:

### Tests 

Some tests need test setup or test containers which are slow. Run the tests like this to skip these tests.

```bash
dotnet test --filter "Category!=Dependencies & Category!=TestContainer"
```

### Best Practices

- Code style is enforced by StyleCop (`backend/stylecop.json`) and `.editorconfig` — follow the surrounding file's conventions.
- Do not write XML comments.

## Shared best practices

- Do write precise short comments and only when needed.
- Do not comment a class or a method, only put comments inside functions or above variables.