# Backend Performance — Open

Analysis of `backend/src` (2131 C# files, ~187k LOC). Ordered by severity: expected
production impact × how hot the code path is.

Severity key: **S1** critical (can dominate request latency or take the process down),
**S2** high (measurable on every request in a common path), **S3** moderate (steady
overhead / allocation churn), **S4** low (worth fixing while nearby).

Item numbers are stable and never reused. Completed items move to
[resolved.md](resolved.md) keeping their number, so gaps in the sequence here are
expected — items **4**–**22** and **24**–**30** are closed and live there.

**Status: 4 open of 30 — items 1, 2, 3 (one root cause) and 23. The other 26 are in [resolved.md](resolved.md).**

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

## S2 — High

### 23. Full-text search inlines up to 1000 ids into a MongoDB `$in`
`backend/src/Squidex.Domain.Apps.Entities/Contents/Queries/ContentQueryParser.cs:93,107`

```csharp
var textQuery = new TextQuery(query.FullText, 1000) { PreferredSchemaId = schema.Id };
var fullTextIds = await textIndex.SearchAsync(context.App, textQuery, context.Scope(), ct);
...
searchFilters.Add(ClrFilter.In("id", fullTextIds.Select(x => x.ToString()).ToList()));
```

Every full-text content query becomes: one search round trip, then a second query whose filter
carries up to 1000 GUID strings — roughly 37 KB of BSON — forcing 1000 index seeks. The
`Select(x => x.ToString())` also allocates 1000 strings per query.

There is a correctness edge here too, which is why it outranks pure throughput items:
the index returns the top 1000 *by relevance*, but the outer query then re-sorts by the default
`LastModified` and pages over that. Relevance order is discarded, and anything past the 1000
cap is silently missing — invisible to the caller, who just sees fewer results than exist.

**Fix:** push paging into the text index so it returns only the page (plus a total), rather
than a fixed 1000-id prefix that the outer query re-sorts.

---

## Suggested order of attack

1. **Item 23** — full-text paging. Really a correctness fix that happens to also be faster:
   the 1000-id cap silently truncates results and discards relevance order today.
2. **Items 1–3, engine pooling** — the largest single cost, and the most invasive change on
   the list. It touches the security boundary of user-authored scripts, since a pooled
   engine must not carry state from one script into the next. **Profile before writing it**:
   the estimate that engine construction dominates a scripted content list is read off the
   loops, not taken from a trace.

Nothing else is outstanding. Everything cheap, every correctness-shaped finding and
everything in the stability category is closed.

---

## Method / caveats

Findings come from static reading of the hot paths (content query + enrichment pipeline,
GraphQL execution, write/validation path, event consumers, HTTP pipeline, MongoDB and EF
repositories, asset serving, response/DTO construction) plus scripted scans for
sync-over-async, awaits inside loops, uncached `Regex`, and repeated LINQ materialisation.

**No profiling or benchmarking was run.** The ordering is a reasoned estimate of impact,
not measured data. Counts like "200 × 4 engine constructions" or "2000 link generations"
are derived from reading the loops, not observed. Confirm the expensive items with a
profiler against a representative workload before investing in the larger refactors.

Items 21–30 were added in a second pass over areas the first pass had not covered: the
EF data layer, asset serving and transformation, response DTO and link construction, the
full-text search path, and the remaining enrichment steps. Two candidates were dropped
during that pass after checking them: per-content permission checks (already memoized in
`Resources.Can`) and the lazily built static maps in `Adapt` (a benign race that at worst
builds the same dictionary twice).

Line numbers were verified against the working tree at the time of writing.
