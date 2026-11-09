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
- Define colors only in `src/app/theme/_colors.scss`: literal values, the light/dark palettes and computed colors (`shade()`, `fade()`, ...). Everywhere else use the `$color-*` variables. In TypeScript, read theme colors with `ThemeService.getColor()`. Exceptions are semantic data colors (avatar colors, rule icon colors, chart colors).
- In tests, keep `verify` calls and `expect` calls next to each other, without blank lines between them.
- Use signals in components: `input()`, `output()`, `computed()`, `linkedSignal()` instead of `@Input`, `@Output` and `ngOnChanges`.
- Make all signal fields `readonly`.
- Document component inputs and outputs with a short `/** ... */` comment. Do not add any other comments to components.
- Use `inject()` in components instead of constructor parameters.
- Order class members:
  1. `private readonly` fields, sorted by name.
  2. Other `private` fields, sorted by name.
  3. Protected injections (`inject()`).
  4. Inputs and outputs, including models that are part of the public interface.
  5. Basic signals (`signal()`, `linkedSignal()`, internal models, view and content queries).
  6. Computeds.
  7. Resources.
  8. Every other protected field.
- Signals are `protected`, unless they are part of the public interface (inputs, outputs, public models).
- Always put an empty line after a signal field (inputs, outputs, models, signals, computeds, resources, queries), even if it is a single line.
- Never inline `inject()` calls into other expressions. Assign every injection to its own field and use that field.
- Load data from services with resources (`rxResource`, `resource`), never with `toSignal`.
- Inline small helper functions that are used only once when the surrounding code already makes clear what they do (e.g. the body of a well-named `computed`). Keep a named helper when its name explains logic that is not obvious at the call site.
- Do not put blank lines between single-line `private readonly` and `private` fields, neither inside a group nor between the two groups. Fields that span multiple lines are separated by blank lines.
- Pass submit actions to `submit()`/`submitAndUpdate()` as block-body arrows with an explicit `return`.
- In signal form schemas, sort the rule calls by validator name, then by the field order of the model.
- Format signal form rules with a `when` option like this, with a blank line between multi-line rules:

```ts
hidden(path.inlineEditable,
    {when: ({ valueOf }) => valueOf(path.editor) === 'Radio' }
);
```

- Use single quotes in TypeScript whenever possible.
- Keep each import statement on a single line.
- Component constructors only contain effects. Initialize everything else (signals, computeds, resources, forms) directly in the field declaration, so fields stay type safe.

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

- Use xUnit assertions (`Assert.Equal`, `Assert.True`, `Assert.Single`, `Assert.Empty`, ...) for simple properties and values.
- Use FluentAssertions (`Should().BeEquivalentTo(...)`) only for deep, structural comparisons of objects and collections.
- In Verify snapshot tests, verify one result per `Verify`/`VerifyJson` call. Do not combine several results into one snapshot.
- Prefer scrubbing (`ScrubInlineGuids`, `AddScrubber`, ...) over ignoring members (`IgnoreMember`) for values that change between runs or environments. Only ignore members if they cannot be scrubbed.

### Best Practices

- Code style is enforced by StyleCop (`backend/stylecop.json`) and `.editorconfig` — follow the surrounding file's conventions.
- Do not write XML comments.

## Shared best practices

- Do write precise short comments and only when needed.
- Do not comment a class or a method, only put comments inside functions or above variables.
- Do not comment properties in general (for example event properties). Only comment behavior inside classes.
- Exception: properties of API models (DTOs) keep their XML comments, because they are used for the OpenAPI documentation.
- Do not write conditions that span multiple lines. Split them into multiple `if` statements (e.g. early returns) instead.
- Try to keep files under 300 lines. Split larger classes by responsibility. This does not apply to tests.
- Do not put a blank line between a variable assignment and an `if` statement that checks this variable.
- Use the same prefix for related variables, e.g. `postsAll` and `postsTaken` instead of `allPosts` and `taken`.
- Use primary constructors whenever possible.
- In test classes, put helper methods after all tests.

## Maintaining this file

- When the user states a new general coding guideline, confirm it with the user and then add it to this file automatically.