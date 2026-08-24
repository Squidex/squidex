# Backend Performance — Open

Analysis of `backend/src` (2131 C# files, ~187k LOC). Ordered by severity: expected
production impact × how hot the code path is.

Severity key: **S1** critical (can dominate request latency or take the process down),
**S2** high (measurable on every request in a common path), **S3** moderate (steady
overhead / allocation churn), **S4** low (worth fixing while nearby).

Item numbers are stable and never reused. Completed items move to
[resolved.md](resolved.md) keeping their number, so gaps in the sequence here are
expected — items **4**, **5** and **7** are done and live there.

**Status: 17 open of 20 — 15 untouched, 1 half fixed (8), 1 attempted but still open (9).**

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

### 9. Generic query-model cache key still collides across apps — **ATTEMPTED, STILL OPEN**
`backend/src/Squidex.Domain.Apps.Entities/Contents/Queries/ContentQueryParser.cs:280,290`

The constant `"EDM/__generic"` was replaced with:

```csharp
if (schema == null) return $"EDM/{app.Version}/{withHidden}";
if (schema == null) return $"JSON/{app.Version}/{withHidden}";
```

**This does not close the hole.** `App.Version` is `Entity.Version` — a per-aggregate
event-stream position (`Squidex.Infrastructure/Commands/Entity.cs:24`), not a globally
unique value. Two different apps that have received the same number of events share the
same version, which is the common case for young or low-traffic apps. `EDM/7/False`
means "app A at v7" and "app B at v7" interchangeably.

The cached model is built from `context.App.PartitionResolver()`, so a colliding app
still parses cross-schema `/contents` queries against **another tenant's languages** —
wrong filters accepted, correct ones rejected, for the 60-minute cache lifetime.

The schema-scoped keys on lines 283 and 293 are safe: they embed `schema.Id`, a globally
unique `DomainId`.

**Fix:** put `app.Id` in the key, not just the version —
`$"EDM/{app.Id}/{app.Version}/{withHidden}"`.

Separately, and unchanged: keying on `app.Version` means *any* app-level event (a
contributor edit, a settings tweak) invalidates the EDM models of every schema in the
app, forcing expensive OData model rebuilds. Keying on the language-config version
instead would invalidate only when something the model actually depends on changes.

---

### 8. GraphQL field-selection data loader — **HALF FIXED**
`backend/src/Squidex.Domain.Apps.Entities/Contents/GraphQL/GraphQLExecutionContext.cs:162,166`

The key-building bug is fixed — line 188 is now `keys[i] = (ids[i], fields)`, so the
batch requests all N ids instead of the first one N times.

The second half is untouched. The batch callback still keys its result dictionary by the
*merged* `fields` set:

```csharp
var fields = batch.SelectMany(x => x.Fields).ToHashSet();   // line 162
var result = await QueryContentsByIdsAsync(batch.Select(x => x.Id), fields, ct);
return result.ToDictionary(x => (x.Id, fields));            // line 166
```

The keys the loader was *called* with hold the caller's `HashSet<string>` instance;
`fields` here is a freshly allocated one. `HashSet<T>` has no structural equality, so
the tuple comparer falls back to reference equality and **no lookup ever matches**. The
contents are fetched from the database and then thrown away; every field-selected
GraphQL content resolves to null.

**Fix:** supply an `IEqualityComparer` for the tuple key that compares field sets by
content, or key by a canonical string (sorted field names joined) instead of the set
itself.

---

### 6. Sync-over-async on the authentication path — **OPEN**
`backend/src/Squidex/Areas/IdentityServer/Config/Dynamic/DynamicSchemeProvider.cs:129`

The file was touched (a variable rename and whitespace tidy-up), but the blocking call
is unchanged — it just moved from line 134 to 129:

```csharp
var scheme = GetSchemeCoreAsync(name, default).Result;
```

`Get(string? name)` is an options-resolution hook invoked from the auth pipeline, so
each call parks a thread-pool thread on a DB round trip. Under load this is a classic
thread-pool starvation source, and it deadlocks outright if any sync context is ever
installed.

Same pattern, lower blast radius:
- `Squidex.Domain.Apps.Entities/Contents/DomainObject/Guards/ScriptingExtensions.cs:144` — `.Wait()` on full content validation inside a script callback.
- `Squidex.Data.MongoDb/Infrastructure/MongoRepositoryBase.cs:26` — `InitializeAsync(default).Wait()`.

**Fix:** cache scheme results synchronously (populated by an async initializer /
background refresh) so `Get` can return without blocking.

---

### 10. Unbounded in-memory request-log queue
`backend/src/Squidex.Infrastructure/Log/BackgroundRequestLogStore.cs:22,126`

`jobs` is an unbounded `ConcurrentQueue<Request>`; `LogAsync` enqueues on every API
request and the flush timer runs once per `WriteIntervall`. If `InsertManyAsync` throws
(Mongo unreachable, disk full), the `TrackAsync` loop aborts and the surviving items
stay queued while new ones keep arriving. A sustained storage outage under load grows
the queue until OOM — the logging subsystem takes down the whole process.

`BackgroundUsageTracker` uses a `ConcurrentDictionary` keyed by (key, category, date),
so it is naturally bounded and not affected.

**Fix:** bound the queue (drop-oldest with a counter, or `Channel` with
`BoundedChannelFullMode.DropWrite`) and log the drop count.

---

### 11. Cross-schema content queries never use the cached total
`backend/src/Squidex.Data.MongoDb/Domain/Apps/Entities/Contents/Operations/QueryByQuery.cs:56`

```csharp
var (filter, isDefault) = CreateFilter(app.Id, schemas.Select(x => x.Id), ...);
```

`isDefault` is computed and then discarded — the multi-schema overload has no
`else if (isDefault)` branch, unlike the single-schema overload 30 lines below which
routes through `countCollection.GetOrAddAsync`. So the "all schemas" `/contents`
endpoint runs a full `CountDocumentsAsync` over every content in the app on each page
request, uncached.

**Fix:** mirror the single-schema branch, keyed by app + sorted schema-id set.

---

## S3 — Moderate

### 12. `ReaderWriterLockSlim` used exclusively for write locks in the ETag path
`backend/src/Squidex.Web/Pipeline/CachingManager.cs:37,55,83,107,178`

`CacheContext` takes `EnterWriteLock` in `AddDependency`, `AddDependency<T>`,
`AddHeader` and `Finish`. No code path ever takes a read lock, so the reader/writer
machinery is pure overhead — `ReaderWriterLockSlim` costs roughly 2–3× a plain
`Monitor` acquisition.

`AddDependency` is called once per content, once per schema and once per resolved
reference, so a 200-item list with references takes on the order of a thousand
write-lock round trips per request.

**Fix:** a plain `lock` object.

---

### 13. Rules dictionary rebuilt per event inside the batch loop
`backend/src/Squidex.Domain.Apps.Entities/Rules/RuleEnqueuer.cs:106`

`On(...)` receives batches of 200 events and builds
`Rules = rules.ToReadonlyDictionary(x => x.Id)` for each one. Events in a batch are
overwhelmingly from the same app, so the same immutable dictionary is constructed up to
200 times per batch, alongside a fresh `RulesContext` record each iteration.

**Fix:** group the batch by `AppId` and build one `RulesContext` per group.

---

### 14. `AppProvider` copies cached schema/rule lists on every call
`backend/src/Squidex.Domain.Apps.Entities/AppProvider.cs:197,208,216`

`GetSchemasAsync` and `GetRulesAsync` end with `?.ToList() ?? []` — a defensive copy of
the cached list allocated per call, even on a cache hit. `GetRuleAsync` (line 216)
copies the entire rule list just to `Find` one element.

These are called per request in the query pipeline and per event in `RuleEnqueuer`.

**Fix:** return the cached `IReadOnlyList<T>` directly (the cached instances are already
immutable) and have `GetRuleAsync` search without materialising.

---

### 15. Faulted tasks are cached permanently in `CollectionProvider`
`backend/src/Squidex.Data.MongoDb/Domain/Apps/Entities/Contents/CollectionProvider.cs:21`

```csharp
return collections.GetOrAdd((appId, schemaId), CreateCollectionAsync);
```

`CreateCollectionAsync` creates indexes, so it can fail transiently. `GetOrAdd` stores
the returned `Task` — including a *faulted* one — for the process lifetime. One
transient Mongo hiccup during first access permanently breaks queries for that
app/schema until restart.

`GetOrAdd` can also invoke the factory concurrently for the same key, issuing duplicate
`CreateManyAsync` calls.

The same faulted-task-caching pattern exists in `AppProvider.GetOrCreate`
(`AppProvider.cs:213`), though the local cache is request-scoped so the window is small.

**Fix:** evict the entry when the task faults; wrap in `Lazy<Task<T>>` with
`ExecutionAndPublication` to deduplicate.

---

### 16. `IsFrontendClient` re-scans claims on every access
`backend/src/Squidex.Domain.Apps.Entities/Context.cs:32`

```csharp
public bool IsFrontendClient => UserPrincipal.IsInClient(DefaultClients.Frontend);
```

`IsInClient` is `principal.Claims.Any(x => ...)` — `ClaimsPrincipal.Claims` walks every
identity and every claim, and the LINQ `Any` allocates an enumerator per call. It is
read in the enrichment steps, in `ConvertData.GenerateConverter` (per schema group) and
in `ShouldEnrich` guards, so it runs many times per request against an unchanging value.

**Fix:** compute once in the constructor into a `readonly bool`.

---

### 17. `ResolvingReferences()` re-evaluated per content
`backend/src/Squidex.Domain.Apps.Entities/Contents/Queries/Steps/ResolveReferences.cs:63,141`

`SchemaExtensions.ResolvingReferences` is a lazy `Fields.OfType<...>().Where(...)` — it
is not materialised. Line 141 calls it inside `foreach (var content in contents)`, so
the full field scan plus two LINQ iterator allocations happen once per content rather
than once per schema.

`ResolveReferences.EnrichAsync` also enumerates `contents.GroupBy(...)` twice
(lines 37 and 47), as does `ConvertData` (lines 39 and 67) — safe for a `List`, wasteful
for anything lazy.

**Fix:** hoist to `var refFields = schema.ResolvingReferences().ToList();` outside the
loop; materialise `contents` once at the top of each step.

---

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

## S4 — Low

### 20. Script cache key embeds the entire script source
`backend/src/Squidex.Domain.Apps.Core.Operations/Scripting/Internal/CacheParser.cs:20`

```csharp
var cacheKey = $"{typeof(CacheParser)}_Script_{script}";
```

Every parse allocates a new string containing a copy of the whole script body and hashes
it end to end, and `IMemoryCache` retains that string as the key. Entries also have no
size limit, so each edit of a script adds another full-source-sized entry for the
10-minute window.

**Fix:** key by a precomputed hash of the source (or by schema id + script version).

---

## Suggested order of attack

1. **Finish items 9 and 8** — both are one-line-ish completions of work already started,
   and both are correctness bugs. Item 9 in particular still leaks one tenant's language
   config into another's query model whenever two apps share a version number.
2. **Engine pooling (items 1–3)** — one change in `JintScriptEngine` fixes the largest
   open read-path cost, and items 2 and 3 mostly disappear with it.
3. **Item 11** — small, self-contained; removes an uncached full-collection count from a
   paged endpoint.
4. **Items 6, 10** — stability under load rather than throughput; worth doing before the
   micro-optimisations.
5. **Everything else** — steady-state allocation and lock overhead; measure with a
   profiler on a representative content-list request before and after.

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
