# Backend Performance — Open

Analysis of `backend/src` (2131 C# files, ~187k LOC). Ordered by severity: expected
production impact × how hot the code path is.

Severity key: **S1** critical (can dominate request latency or take the process down),
**S2** high (measurable on every request in a common path), **S3** moderate (steady
overhead / allocation churn), **S4** low (worth fixing while nearby).

Item numbers are stable and never reused. Completed items move to
[resolved.md](resolved.md) keeping their number, so gaps in the sequence here are
expected — items **4**–**17** and **20** are closed and live there.

**Status: 5 open of 20 — items 1, 2, 3, 18, 19. The other 15 are in [resolved.md](resolved.md).**

---

## S1 — Critical

### 1. A fresh Jint `Engine` is constructed for every script evaluation
`backend/src/Squidex.Domain.Apps.Core.Operations/Scripting/JintScriptEngine.cs:144`

`CreateEngine` calls `new Engine(...)` on every `Execute` / `ExecuteAsync` /
`TransformAsync`. Building a Jint engine allocates a complete JS realm (global object,
`Object`/`Array`/`JSON`/`Math`/`RegExp` prototypes, intrinsics) plus runs every
registered `IJintExtension.Extend`. Script *parsing* is cached via `CacheParser`, but
engine construction — the expensive half — is not.

This is the root cause of items 2 and 3, which is why it ranks first.

**Fix:** pool engines (`ObjectPool<Engine>`) keyed by the option set, resetting globals
between uses; or hoist one engine per enrichment batch instead of per item.

---

### 2. Workflow enrichment runs one Jint engine per content *per transition*
`backend/src/Squidex.Domain.Apps.Entities/Contents/DynamicContentWorkflow.cs:90,118`
`backend/src/Squidex.Domain.Apps.Entities/Contents/Queries/Steps/EnrichWithWorkflows.cs:22,30,31`

`EnrichWithWorkflows` loops over every content and awaits `GetNextAsync` and
`CanUpdateAsync` sequentially. `GetNextAsync` loops over every transition and calls
`IsTrue`, which calls `scriptEngine.Evaluate` whenever the transition has an
expression — a new engine each time (item 1).

A frontend content list of 200 items with a workflow having 3 conditional transitions
executes **200 × (3 + 1) = 800 engine constructions** in one request, serially.

`GetWorkflowAsync` additionally re-scans `app.Workflows.Values` with
`SchemaIds.Contains(schemaId)` on every one of those calls.

**Fix:** cache the resolved `Workflow` per (appId, schemaId) for the batch; memoize
condition results per (transition, contentData); reuse one engine.

---

### 3. Query scripts execute one engine per content, serially
`backend/src/Squidex.Domain.Apps.Entities/Contents/Queries/Steps/ScriptContent.cs:57`

`foreach (var content in group) await TransformAsync(...)` — every content in the page
gets its own engine construction plus its own
`CancellationTokenSource.CreateLinkedTokenSource`. Any schema with a query script pays
this on every read.

**Fix:** same as item 1 — reuse the engine across the group; per-content state is
already isolated in `ContentScriptVars`.

---

## S3 — Moderate

### 18. Sequential N+1 schema and component lookups
`backend/src/Squidex.Domain.Apps.Entities/AppProviderExtensions.cs:30`
`backend/src/Squidex/Areas/Api/Controllers/Contents/Generator/SchemasOpenApiGenerator.cs:39,48`

`ResolveSchemasAsync` awaits `appProvider.GetSchemaAsync` once per id in a loop; the
OpenAPI generator awaits `GetComponentsAsync(schema, ...)` once per schema in a loop.
For an app with 100 schemas the OpenAPI docs endpoint serialises 100 round trips that
have no dependency on each other.

**Fix:** `await Task.WhenAll(...)` over the lookups, or add a batch accessor. Both are
warm-cache paths, which is why this sits at S3 rather than S2.

---

### 19. Header parsing re-splits and re-allocates on every read
`backend/src/Squidex.Domain.Apps.Entities/Contents/ContentHeaders.cs:140,150`
`backend/src/Squidex.Domain.Apps.Entities/ContextHeaders.cs:133`

```csharp
public static HashSet<string>? Fields(this Context context)
    => context.AsStrings(KeyFields).ToHashSet();

public static HashSet<Language> Languages(this Context context)
    => context.AsStrings(KeyLanguages).Select(Language.GetLanguage).ToHashSet();
```

`AsStrings` does `value.Split(...).Select(Trim).Distinct()`. Each call allocates the
split array, two LINQ iterators and a `HashSet`. `ConvertData.GenerateConverter` calls
`Languages()` **and** `ResolveUrls().ToList()` per schema group, and `Fields()` is read
from several steps. The headers never change for the lifetime of a `Context`.

**Fix:** memoize the parsed values on `Context`, invalidating in the clone builder.

---

## Suggested order of attack

1. **Engine pooling (items 1–3)** — by a wide margin the largest remaining cost, and the
   only one left that can dominate a request. One change in `JintScriptEngine` addresses
   it, and items 2 and 3 mostly disappear with it.
2. **Items 18 and 19** — steady-state allocation and a warm-cache N+1; both are small and
   neither is likely to show up next to item 1.

Everything correctness-shaped is closed, as is everything in the stability category. What
remains is pure throughput work — exactly the category that should be profiled before it
is written. Item 1 in particular is worth measuring first: the estimate that it dominates
a scripted content list comes from reading the loops, not from a trace.

---

## Method / caveats

Findings come from static reading of the hot paths (content query + enrichment pipeline,
GraphQL execution, write/validation path, event consumers, HTTP pipeline, MongoDB
repositories) plus scripted scans for sync-over-async, awaits inside loops, uncached
`Regex`, and repeated LINQ materialisation. **No profiling or benchmarking was run** —
the ordering is a reasoned estimate of impact, not measured data. Item counts like
"200 × 4 engine constructions" are derived from reading the loops, not observed.
Confirm items 1–3 with a profiler against a representative workload before investing in
the larger refactors.

Line numbers were re-verified against the working tree after the first round of fixes.
