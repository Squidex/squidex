# Backend Performance — Resolved

Items from the backend performance review that are done. Numbering matches
[todo.md](todo.md) — resolved items keep their original number so references stay valid.

Most entries are fixes. Items **6** and **14** are closed as *accepted*, **21** as *rejected*
and **23** as *bounded but not fixed* — kept here so they are not re-reported as new findings.
Item **18** records a finding that turned out to be wrong.

---

### 4. Streaming export enriched contents one at a time — **FIXED**
`backend/src/Squidex.Domain.Apps.Entities/Contents/Queries/ContentQueryService.cs:49`

**Was:** `StreamAsync` called `contentEnricher.EnrichAsync(content, ...)` per item. The
single-item overload wraps the content in `Enumerable.Repeat(content, 1)` and runs the
whole pipeline for it — a new result `List`, a new schema-cache `Dictionary`, and every
`IContentEnricherStep` twice. Every batching optimisation in `ResolveReferences`,
`ResolveAssets` and `ConvertData` was defeated, so reference resolution degenerated to
one DB round trip per content. A 100k-content export meant 100k pipeline setups.

**Now:**

```csharp
await foreach (var batch in contents.Batch(50, ct).WithCancellation(ct))
{
    var enriched = await contentEnricher.EnrichAsync(batch, context, ct);
    foreach (var content in enriched)
    {
        yield return content;
    }
}
```

`Batch` yields `List<T>`, which binds to the `IEnumerable<Content>` overload, and that
overload calls `EnrichInternalAsync(contents, cloneData: false, ...)` — matching the
previous single-item behaviour. Reference resolution now amortises across 50 contents
instead of one DB round trip each.

**Follow-up:** 50 is conservative next to the 200-item batches used elsewhere
(`RuleEnqueuer.BatchSize`). Once profiled, a larger batch would amortise further.

---

### 5. `WriteManyAsync` iterated the unfiltered job list — **FIXED**
`backend/src/Squidex.Data.MongoDb/Domain/Apps/Entities/Contents/MongoContentRepository_SnapshotStore.cs:138`

**Was:** the method built `validJobs` via `jobs.Where(x => IsValid(x.Value)).ToList()`
and then looped over `jobs`. Two defects in one — the corrupt-data guard was bypassed
(the comment above it notes the data "might throw an exception if we do not ignore it"),
and the sequence was enumerated twice, re-running any upstream projection.

**Now:** `foreach (var job in jobs)` → `foreach (var job in validJobs)`.

---

### 7. Regex rebuilt per content write — **FIXED (the expensive part)**
`backend/src/Squidex.Domain.Apps.Core.Operations/ValidateContent/Validators/PatternValidator.cs`

**Was:** every content write constructs a fresh `ContentValidator` and with it a whole
validator object graph. For each pattern field that included
`new Regex($"^{pattern}$", options, Timeout)` in the constructor — a full pattern parse
and interpreter build. A 10k-item import with 5 pattern fields did 50k pattern parses.

**Now:** `PatternValidator` resolves its `Regex` from a process-wide, 1000-entry
`Squidex.Caching.LRUCache<(string Pattern, RegexOptions Options), Regex>`. The same
import does 5 parses.

**Why a plain static cache and not an async-local / request-scoped one.** The cacheable
unit turned out to be *only* the `Regex`, and a `Regex` has no dependency on the request
at all — it is a pure function of (pattern, options), and `Regex` instances are
thread-safe for matching. So a process-wide cache is both simpler and strictly more
effective than a request-scoped one, which would rebuild each pattern once per request.

**Why the validator tree itself is still rebuilt per write.** Caching the graph — even
per request — is not safe. It captures per-item state at several levels:

| Captured state | Where |
| --- | --- |
| `context.Root.PreviousData` | `DefaultValidatorsFactory` → `NotChangedValidator` |
| `context.Action` (Publish vs not) | `IsRequired` in both factories — changes *which* validators are emitted |
| `context.Mode` (Optimized) | `DependencyValidatorsFactory` short-circuits entirely |
| `context.Root.App` / `.Schema` | closures in `CheckAssets` / `CheckContentsByIds` / `CheckUniqueness` |

A bulk import is a single request but each item carries its own `PreviousData` and
`CommandId`, so even an `ILocalCache` keyed by schema would hand back a graph wired to
the previous item. The remaining per-write cost is a few hundred small gen-0 allocations
(dictionaries and `AggregateValidator` arrays) — real, but an order of magnitude below
the pattern parse that was removed. Reworking the factories to split
"schema-shaped, cacheable" from "context-bound" validators is the follow-up if profiling
says the churn still matters.

`RegexOptions.Compiled` was deliberately *not* added: it moves cost into IL emit and the
generated code can never be unloaded, which is a bad trade for user-authored patterns.

**The cache access is locked, and has to be.** `LRUCache` is a plain `Dictionary` plus a
`LinkedList` with no synchronisation, and its `TryGetValue` *mutates* the recency list —
so there is no lock-free read path. Verified empirically against the shipped
`Squidex.Caching` 8.0.3 assembly: 8 threads hammering an unguarded instance produced
`InvalidOperationException: The LinkedList node does not belong to current LinkedList`,
`ArgumentException: An item with the same key has already been added`, and repeated
`NullReferenceException`s. (The assembly *does* reference `Monitor`, but from other types
in the package — not `LRUCache`.) Validators are constructed concurrently on every
content write, so this path is genuinely contended.

`new Regex(...)` is built *outside* the lock, so pattern parsing is never serialised
across threads; a cold race can build the same pattern twice, which only wastes a little
work and never returns anything incorrect. The critical section is just the dictionary
and linked-list updates.

`MemoryCache` would remove the lock, but `PatternValidator` is constructed without
dependency injection, so it would have to create and hold its own cache instance. The lock
is the smaller change and it is already covered by the concurrency harness below.

**Verified:** `dotnet build` clean (0 warnings); a harness mirroring `GetRegex` ran 1.6M
operations over 8 threads against 3000 distinct patterns in a 1000-entry cache
(continuous eviction) with 0 exceptions, 0 wrong matches and the cache correctly bounded
at 1000; full `Squidex.Domain.Apps.Core.Tests` suite green (1247), plus 25 validation
tests in `Squidex.Domain.Apps.Entities.Tests`.

---

### 8. GraphQL field-selection data loader never matched its results — **FIXED**
`backend/src/Squidex.Domain.Apps.Entities/Contents/GraphQL/GraphQLExecutionContext.cs`

**Was:** two separate defects in the `GetContentsLoaderWithFields` path, which serves
every GraphQL reference resolved under the `@optimizeFieldQueries` directive.

1. `BuildKeys` wrote `keys[i] = (ids[0], fields)` — every key in the batch was the
   *first* id, so one content was requested N times and the other N−1 never were.
2. The batch callback keyed its result dictionary by a freshly merged field set:

   ```csharp
   var fields = batch.SelectMany(x => x.Fields).ToHashSet();
   return result.ToDictionary(x => (x.Id, fields));
   ```

   `NonCachingBatchLoader` then looks the results up with the *original* key. The key
   type is `(DomainId, HashSet<string>)` and `HashSet<T>` has no structural equality, so
   the tuple comparer fell back to reference equality and **no lookup ever matched**.
   Contents were fetched from the database and thrown away; every field-selected
   reference resolved to `null`.

   This was unconditional, not a race: `SharedExtensions.FieldNames()` builds a *new*
   `HashSet` per resolver invocation (`new FieldNameResolver(...).Iterate(...)`), so the
   requested instance and the merged instance were never the same object.

**Now:** `(1)` was fixed to `ids[i]`. For `(2)`, the key is compared by value:

```csharp
private static readonly IEqualityComparer<HashSet<string>> FieldsComparer = HashSet<string>.CreateSetComparer();

private sealed class ContentWithFieldsComparer : IEqualityComparer<(DomainId Id, HashSet<string> Fields)>
{
    public bool Equals((DomainId Id, HashSet<string> Fields) x, (DomainId Id, HashSet<string> Fields) y)
        => x.Id.Equals(y.Id) && FieldsComparer.Equals(x.Fields, y.Fields);

    public int GetHashCode((DomainId Id, HashSet<string> Fields) obj)
        => HashCode.Combine(obj.Id, FieldsComparer.GetHashCode(obj.Fields));
}
```

and the callback groups by field selection instead of merging:

```csharp
var result = new Dictionary<(DomainId Id, HashSet<string> Fields), EnrichedContent>(ContentWithFieldsComparer.Instance);

foreach (var byFields in batch.GroupBy(x => x.Fields, FieldsComparer))
{
    var contents = await QueryContentsByIdsAsync(byFields.Select(x => x.Id), byFields.Key, ct);

    foreach (var content in contents)
    {
        result[(content.Id, byFields.Key)] = content;
    }
}
```

Grouping rather than merging matters for correctness: a batch can hold several different
field selections, and merging them would hand a caller fields it did not request. Because
the grouping is by *value*, identical selections coming from different resolvers still
collapse into a single query — which the old reference-equality behaviour could not do.

`HashSet<string>.CreateSetComparer()` is cached in a static; it allocates a new comparer
on every call.

**Verified:** a new regression test,
`GraphQLQueriesTests.Should_resolve_referenced_contents_when_field_queries_are_optimized`,
resolves a reference under `@optimizeFieldQueries`. It **fails on the pre-fix code** and
passes after — red-to-green, not just green. Full GraphQL suite (79) and full
`Squidex.Domain.Apps.Entities.Tests` (1527) green.

---

### 9. Generic query-model cache key collided across apps — **FIXED**
`backend/src/Squidex.Domain.Apps.Entities/Contents/Queries/ContentQueryParser.cs:276-294`

**Was:** the cross-schema (`schema == null`) cache key was the constant
`"EDM/__generic"` / `"JSON/__generic"`. The cached model is built from
`context.App.PartitionResolver()`, so whichever app populated the cache first imposed its
languages on every other app's cross-schema `/contents` queries for the 60-minute cache
lifetime — wrong filters accepted, correct ones rejected, across tenants.

An intermediate fix replaced it with `$"EDM/{app.Version}/{withHidden}"`, which did not
close the hole: `App.Version` is `Entity.Version`, a per-aggregate event-stream position,
so two apps with the same event count still collided.

**Now:** the key carries the app identity (commit `b7103a12`):

```csharp
return $"EDM/{app.Id}/{app.Version}/{withHidden}";
return $"EDM/{app.Id}/{app.Version}/{schema.Id}_{schema.Version}/{withHidden}";
```

`app.Id` is a globally unique `DomainId`, so no two apps can share a key.

**Deliberately not changed: the `app.Version` over-invalidation.** Keying on `app.Version`
means any app-level event (a contributor edit, a settings tweak) rebuilds the EDM models
of every schema in the app. Narrowing it to a language-specific token looked attractive —
`PartitionResolver` is just `app.Languages.ToResolver()` — but `BuildDataSchema` also
reads `partitioning.GetName(...)` and `IsOptional`, so a key built from the language
*codes* alone could serve a stale model after a language rename or fallback change.
`app.Version` is conservative but provably correct: it changes whenever anything about
the app does. Trading guaranteed correctness for a cache-hit-rate win is the wrong
direction here, so it stays until someone establishes the model's exact dependency set.

---

### 6. Sync-over-async on the authentication path — **CLOSED: ACCEPTED, WON'T FIX**
`backend/src/Squidex/Areas/IdentityServer/Config/Dynamic/DynamicSchemeProvider.cs:129`

```csharp
var scheme = GetSchemeCoreAsync(name, default).Result;
```

`Get(string? name)` blocks a thread-pool thread on a DB round trip, which in a hot path
is a classic thread-pool starvation source.

**Closed as accepted, not fixed.** This is dynamic OIDC scheme resolution — reached only
for team-level auth domains, not on ordinary API traffic — so the risk does not justify
the rework. Recorded here rather than deleted so it is not re-reported as a new finding.

If it ever moves onto a hot path, the fix is to cache scheme results synchronously
(populated by an async initializer / background refresh) so `Get` can return without
blocking.

Same pattern elsewhere, also accepted:
- `Squidex.Domain.Apps.Entities/Contents/DomainObject/Guards/ScriptingExtensions.cs:144` — `.Wait()` on full content validation inside a script callback.
- `Squidex.Data.MongoDb/Infrastructure/MongoRepositoryBase.cs:26` — `InitializeAsync(default).Wait()`.

---

### 12. `ReaderWriterLockSlim` used exclusively for write locks in the ETag path — **FIXED**
`backend/src/Squidex.Web/Pipeline/CachingManager.cs`

**Was:** `CacheContext` guarded `AddDependency`, `AddDependency<T>`, `AddHeader` and
`Finish` with `ReaderWriterLockSlim` — but every one of them took `EnterWriteLock`. No
code path ever took a read lock, so the reader/writer bookkeeping was pure overhead at
roughly 2–3× the cost of a plain monitor. `AddDependency` is called once per content,
once per schema and once per resolved reference, so a 200-item list with references took
on the order of a thousand write-lock round trips per request.

**Now:** a plain `Lock` (`System.Threading.Lock`, matching `DisposableObjectBase`), with
each `EnterWriteLock`/`try`/`finally`/`ExitWriteLock` block collapsed to `lock (...)`.

Two incidental improvements fell out of the rewrite:

- `Dispose()` no longer has a lock to dispose, so `CacheContext` only disposes the hasher.
- `AddHeader` had its `EnterWriteLock` *inside* the `try`, so a throw from the acquire
  would have hit `ExitWriteLock` on an unheld lock and masked the original error with a
  `SynchronizationLockException`. `lock` cannot express that shape.

Nothing about the concurrency contract changed — every operation mutates the hasher and
the sets, so there was never anything a read lock could have protected.

**Verified:** build clean, `Squidex.Web.Tests` green (167).

---

### 13. Rules dictionary rebuilt per event inside the batch loop — **FIXED**
`backend/src/Squidex.Domain.Apps.Entities/Rules/RuleEnqueuer.cs`

**Was:** `On(...)` receives batches of 200 events and ran
`Rules = rules.ToReadonlyDictionary(x => x.Id)` for *each* one — a full `Dictionary`
build plus a wrapper allocation per event, even though the events in a batch are
overwhelmingly from the same app.

Note the rules *lookup* was already cheap: `RulesCacheDuration` defaults to 10s, so
`appProvider.GetRulesAsync` was memoized. The waste was purely the per-event indexing.

**Now:** the batch is grouped by app, so rules are resolved and indexed once per app and
the context is built once per group:

```csharp
foreach (var byApp in events.GroupBy(GetAppId))
{
    if (byApp.Key == null) { continue; }

    var rules = await GetRulesAsync(byApp.Key.Id);
    if (rules.Count == 0) { continue; }

    var context = new RulesContext { AppId = byApp.Key, Rules = rules.ToReadonlyDictionary(x => x.Id), ... };

    foreach (var @event in byApp) { ... }
}
```

`GetAppId` returns `null` for restored events and non-`AppEvent` payloads, so they all
collect into one group that is skipped — replacing the two per-event `continue` guards.

**Why `GroupBy` rather than memoizing per app inside the original loop.** The first
attempt kept the original per-event loop and cached the indexed dictionary in a
`Dictionary<DomainId, ...>`, specifically to avoid reordering events. `GroupBy` does
reorder across apps, so that had to be checked rather than assumed:

- Rules are scoped to a single app (`context.AppId`, `context.Rules`), so a rule cannot
  observe another app's events.
- `RuleQueueWriter` is app-agnostic — it accumulates `CreateFlowInstanceRequest` values
  and flushes every 100 regardless of origin.
- `ruleUsageTracker.TrackAsync` is an additive counter per (app, rule, day).
- `GroupBy` preserves source order *within* each group, which is the ordering that can
  actually matter.

Nothing cross-app is order-sensitive, so `GroupBy` is safe — and it is both simpler and
slightly more correct than the memo: keying on `NamedId<DomainId>` (a `sealed record`,
so value equality over id *and* name) means an app renamed mid-batch yields two groups
each carrying its own correct name, where the memo keyed on `.Id` would have reused the
first name seen.

**Verified:** a new test,
`RuleEnqueuerTests.Should_handle_events_of_multiple_apps_with_the_rules_of_each_app`,
feeds an interleaved two-app batch and asserts each event is handled with its own app's
rules, that the grouped order is what reaches the service, and — via `Assert.Same` on the
`Rules` instance — that indexing happens once per app rather than once per event. It
**fails on the pre-fix code** (the ordering assertion shows `app1, app2, app1, app2`
against the expected `app1, app1, app2, app2`) and passes after. Existing coverage did
not include a multi-app batch at all: `Should_handle_events_in_batches` repeats the *same*
event ten times. Full `Squidex.Domain.Apps.Entities.Tests` green (1528).

---

### 20. Script cache key embedded the entire script source — **FIXED**
`backend/src/Squidex.Domain.Apps.Core.Operations/Scripting/Internal/CacheParser.cs:20`

**Was:**

```csharp
var cacheKey = $"{typeof(CacheParser)}_Script_{script}";
```

Every parse allocated a new string holding a full copy of the script body, and
`IMemoryCache` then retained that copy as the key — so each cached script was held twice.

**Now:** `var cacheKey = (typeof(CacheParser), script);`

The tuple boxes once (one small allocation) but holds a *reference* to the existing
script string, so nothing is copied and the cache no longer keeps a second copy alive.

**Honest limit:** this removes the allocation and the duplicate retention, not the hash.
`ValueTuple.GetHashCode` still calls `string.GetHashCode()` on the source, which is O(n)
— .NET does not cache string hash codes. Removing that too would mean keying by schema id
+ script version, which needs that context plumbed into `CacheParser` and changes its API.
Not worth it unless profiling says the hash itself shows up.

---

### Tuple cache keys — sweep of the other call sites

Same change applied where the key was an interpolated string and the cache accepts
`object`. Beyond skipping the string build, a tuple also avoids *formatting* non-string
parts (`DateOnly`, `long`), which the interpolation did on every call.

| Site | Key before | Key now |
| --- | --- | --- |
| `CachingUsageTracker.GetForMonthAsync` | `$"{typeof(..)}_UsageForMonth_{key}_{date}_{category}"` | `(typeof(..), nameof(GetForMonthAsync), key, date, category)` |
| `CachingUsageTracker.GetAsync` | `$"{typeof(..)}_Usage_{key}_{fromDate}_{toDate}_{category}"` | `(typeof(..), nameof(GetAsync), key, fromDate, toDate, category)` |
| `EventEnricher.FindUserAsync` | `$"{typeof(..)}_Users_{actor.Identifier}"` | `(typeof(EventEnricher), actor.Identifier)` |
| `RuleEnqueuer.GetRulesAsync` | `$"{typeof(..)}_Rules_{appId}"` | `(typeof(RuleEnqueuer), appId)` |
| `UsageGate.CacheKey` | `$"{appId}_Plan"` | `(typeof(UsageGate), nameof(GetPlanForAppAsync), appId)` |
| `UsageGate` notified flag | bare `DomainId` | `(typeof(UsageGate), nameof(TrackNotified), appId)` |
| `CachingGraphQLResolver` | `$"GraphQLModel_{appId}_{etag}"` | `(typeof(CachingGraphQLResolver), app.Id, app.Version)` |
| `AppProvider` × 11 | `$"APPS_ID_{appId}"`, `$"GetSchemasAsync({appId})"`, … | `(nameof(AppProvider), "APPS_ID", appId)`, … |

Notes:

- `CachingUsageTracker.GetForMonthAsync` runs on **every API request** (via
  `UsageGate.IsBlockedAsync`) and its old key formatted a `DateOnly` — a culture lookup
  plus an allocation — before building an ~80-character string.
- `CachingGraphQLResolver` no longer needs
  `app.Version.ToString(CultureInfo.InvariantCulture)`; the tuple carries the `long`
  directly, so `System.Globalization` was dropped from the file.
- `UsageGate`'s notified flag previously used a bare `DomainId` as the key. It was safe
  only because that `MemoryCache` is private to the class; it is now explicit.
- `AppProvider` keys carry `nameof(AppProvider)` plus the lookup name, preserving the
  namespacing the old string prefixes provided. The two `TeamCacheKey` overloads and
  `CachingGraphQLResolver.CreateCacheKey` had a single call site each and were inlined;
  `AppCacheKey` and `SchemaCacheKey` have three each and stayed as helpers.

**Three sites were deliberately left as strings:**

- `MongoCountCollection.GetOrAddAsync(string key, …)` — used by `QueryByQuery` and
  `MongoAssetRepository`. That key is **persisted as a MongoDB document id**, not an
  in-memory cache key. Changing it would change stored data.
- `DataLoaderContext.GetOrAddLoader(string loaderKey, …)` — the GraphQL.DataLoader API
  takes a `string`, so `GraphQLExecutionContext.GetContent` cannot use a tuple.
- `Singletons<IMongoClient>.GetOrAdd(string, …)` — typed `string`, and startup-only.

**Verified:** build clean (0 warnings). `Squidex.Domain.Apps.Core.Tests` (1243),
`Squidex.Domain.Apps.Entities.Tests` (1528), `Squidex.Infrastructure.Tests` (1031) and
`Squidex.Web.Tests` (167) all green.

---

### 14. `AppProvider` copies cached schema/rule lists on every call — **CLOSED: ACCEPTED**
`backend/src/Squidex.Domain.Apps.Entities/AppProvider.cs`

`GetSchemasAsync` and `GetRulesAsync` end with `?.ToList() ?? []`, a defensive copy of the
cached list on every call including cache hits, and `GetRuleAsync` copies the whole rule
list just to `Find` one element.

**Closed as accepted, not fixed.** The copy is a single shallow `List` allocation of
already-immutable elements; returning the cached instance directly would expose it to
mutation by callers, which is a worse trade than the allocation. Recorded here so it is
not re-reported as a new finding.

---

### 15. Faulted tasks were cached permanently in `CollectionProvider` — **FIXED**
`backend/src/Squidex.Data.MongoDb/Domain/Apps/Entities/Contents/CollectionProvider.cs`

**Was:**

```csharp
return collections.GetOrAdd((appId, schemaId), CreateCollectionAsync);
```

Two defects. `CreateCollectionAsync` creates indexes, so it can fail transiently — and
`GetOrAdd` stored the returned `Task` including a *faulted* one for the process lifetime,
so a single Mongo hiccup on first access permanently broke queries for that app/schema
until restart. Separately, `GetOrAdd` may invoke its factory concurrently for the same
key, issuing duplicate `CreateManyAsync` calls.

**Now:** the dictionary holds `Lazy<Task<...>>` with `LazyThreadSafetyMode.ExecutionAndPublication`,
so the factory runs exactly once per key even under concurrent access, and the entry is
evicted when it fails:

```csharp
var collection = collections.GetOrAdd(key, CreateLazyCollection);

return AwaitCollectionAsync(key, collection);
...
try
{
    return await collection.Value;
}
catch
{
    collections.TryRemove(new KeyValuePair<...>(key, collection));
    throw;
}
```

The removal uses the `TryRemove(KeyValuePair)` overload, which only removes when the value
is still the *same* `Lazy` instance. The plain `TryRemove(key)` would race: a second thread
that had already retried and succeeded would have its good entry discarded by the first
thread's cleanup.

A `using` alias for the key tuple was tried first, but StyleCop's SA1008 rejects the space
before the parenthesis in `using X = (A, B);`, so the tuple type is written out instead.

**Verified:** build clean, `Squidex.Data.Tests` (180) and all other suites green.

---

### 16. `IsFrontendClient` re-scanned claims on every access — **FIXED (verified)**
`backend/src/Squidex.Domain.Apps.Entities/Context.cs:32,51`
`backend/src/Squidex.Infrastructure/Security/Extensions.cs:70`

**Was:** `public bool IsFrontendClient => UserPrincipal.IsInClient(DefaultClients.Frontend);`
— a computed property whose implementation was `principal.Claims.Any(x => ...)`, walking
every identity and every claim and allocating an enumerator plus a delegate per call. It is
read from several enrichment steps and from `ConvertData.GenerateConverter` per schema
group, so it ran many times per request against a value that cannot change.

**Now:** a get-only auto-property assigned once in the private constructor, and
`IsInClient` rewritten from LINQ `Any` to a plain `foreach`, dropping the closure.

**Verification found the commit did not compile.** Line 32 read
`public bool IsFrontendClient { get; };` — a stray semicolon, `error CS1597: Semicolon
after method or accessor block is not valid`. Removed the semicolon.

Beyond compiling, the assignment is correct for every construction path: the public
`Context(ClaimsPrincipal, App)` chains to the private constructor via `: this(...)`,
`Anonymous` and `Admin` both go through that public one, and `HeaderBuilder.Build` calls
the private 4-argument constructor directly. All four paths therefore set the field.

---

### 17. `ResolvingReferences()` re-evaluated per content — **FIXED**
`backend/src/Squidex.Domain.Apps.Entities/Contents/Queries/Steps/ResolveReferences.cs`

**Was:** `SchemaExtensions.ResolvingReferences` is a lazy
`Fields.OfType<...>().Where(...)` that is never materialized, and `AddReferenceIds` called
it *inside* the per-content loop — so the full field scan plus two LINQ iterator
allocations happened once per content instead of once per schema.

**Now:** hoisted out of the loop.

```csharp
var fields = schema.ResolvingReferences().ToList();

foreach (var content in contents)
{
    content.Data.AddReferencedIds(fields, ids, components);
}
```

(The other call site, the outer `foreach` in `ResolveReferencesAsync`, enumerates the
sequence exactly once and was left alone.)

**The double `GroupBy` was deliberately left alone.** `ResolveReferences.EnrichAsync` and
`ConvertData` each build `contents.GroupBy(x => x.SchemaId.Id)` twice. This does *not*
cause duplicate schema fetches: `ContentEnricher` passes a `ProvideSchema` delegate backed
by a per-call `schemaCache` dictionary, so the second grouping resolves every schema from
memory. The only real cost is re-materializing the LINQ `Lookup` — one extra pass over the
contents and one set of bucket allocations per step.

Deduplicating it was tried and reverted: the gain is small enough that it does not justify
threading a materialized `List<IGrouping<...>>` through the method signatures.

**Verified:** build clean, all suites green.

---

### 10. Unbounded in-memory request-log queue — **FIXED**
`backend/src/Squidex.Infrastructure/Log/BackgroundRequestLogStore.cs`
`backend/src/Squidex.Infrastructure/Log/RequestLogStoreOptions.cs`

**Was:** `jobs` was an unbounded `ConcurrentQueue<Request>`. `LogAsync` enqueues on every
API request while the flush timer drains only once per `WriteIntervall` (1s by default).
If `InsertManyAsync` threw — Mongo unreachable, disk full — the drain aborted and the
surviving entries stayed queued while new ones kept arriving. A sustained storage outage
under load grew the queue until the process ran out of memory: the request *log* taking
down the whole server.

**Now:** a soft bound with an explicit drop counter.

```csharp
if (Volatile.Read(ref jobsCount) >= options.MaxPendingItems)
{
    Interlocked.Increment(ref jobsDropped);
    return Task.CompletedTask;
}

Interlocked.Increment(ref jobsCount);

jobs.Enqueue(request);
```

`jobsCount` is decremented as the drain dequeues, so the queue accepts entries again once
it has been written. Each drain reports what it dropped via a new
`LogRequestLogDropped` message, so the gap in the request log is visible rather than
silent. `MaxPendingItems` defaults to 50,000 — roughly 50 seconds of headroom at 1000
requests/second — and is configurable.

The bound is deliberately *soft*: two threads can both observe `jobsCount < max` and both
enqueue, so the queue can overshoot by the number of concurrent writers. That is fine for
a backpressure limit and avoids a lock on the hot path.

A `Channel` with `BoundedChannelFullMode.DropWrite` was the alternative. The counter was
chosen because it keeps the existing drain loop unchanged and makes the drop explicit at
the call site instead of hiding it behind a channel option.

**Verified:** two new tests —
`Should_drop_logs_when_pending_queue_is_full` and
`Should_accept_logs_again_after_pending_queue_has_been_written`. Both **fail on the
pre-fix code**. The second was additionally mutation-checked: removing the
`Interlocked.Decrement` from the drain loop kills it and nothing else, confirming it
really covers the recovery path rather than passing incidentally. This required splitting
the test helper, because the existing `WaitForCompletion` disposes the store and so cannot
be used to drain twice. `Squidex.Infrastructure.Tests` green (1033).

---

### 11. Cross-schema content queries never used the cached total — **FIXED**
`backend/src/Squidex.Data.MongoDb/Domain/Apps/Entities/Contents/Operations/QueryByQuery.cs`

**Was:**

```csharp
var (filter, isDefault) = CreateFilter(app.Id, schemas.Select(x => x.Id), ...);
```

`isDefault` was computed and then discarded. The multi-schema overload had no
`else if (isDefault)` branch, unlike the single-schema overload thirty lines below which
routes through `countCollection.GetOrAddAsync`. So the "all schemas" `/contents` endpoint
ran a full uncached `CountDocumentsAsync` over every content in the app on each page.

**Now:** the branch is mirrored, keyed by app plus the schema set:

```csharp
else if (isDefault)
{
    var totalKey = CreateTotalKey(app, schemas);

    contentTotal = await countCollection.GetOrAddAsync(totalKey, ct => Collection.Find(filter).CountDocumentsAsync(ct), ct);
}
```

**The key needs care, which is why it is not just an interpolated list.** The schema set
depends on the caller's permissions and arrives in no guaranteed order, so the ids are
sorted before hashing — otherwise the same query would produce different keys and never
hit. And the key becomes the `_id` of the count document, where MongoDB caps index keys at
1024 bytes; a raw join of 37-character ids would exceed that at roughly 27 schemas. Hashing
gives a bounded, deterministic key:

```csharp
var schemaIds = schemas.Select(x => x.Id.ToString()).Order(StringComparer.Ordinal);

return $"{app.Id}_Schemas_{string.Join('_', schemaIds).ToSha256Base64()}";
```

The `_Schemas_` marker keeps this key space distinct from the single-schema overload's
`$"{appId}_{schemaId}"`. The two must not share entries in any case: their filters differ
(`Filter.In` vs `Filter.Eq`, and different existence guards), so the counts are not
interchangeable.

**Verified:** build clean, all suites green.

---

### 18. Sequential N+1 schema and component lookups — **CLOSED: FINDING WAS WRONG**
`backend/src/Squidex.Domain.Apps.Entities/AppProviderExtensions.cs`
`backend/src/Squidex/Areas/Api/Controllers/Contents/Generator/SchemasOpenApiGenerator.cs`

The original finding claimed the OpenAPI docs endpoint "serialises 100 round trips" for an
app with 100 schemas. **That is not true, and the claim was never verified.**

`ContentOpenApiController` calls `appProvider.GetSchemasAsync(AppId, ...)` *before*
`GenerateAsync`, and `AppProvider.GetSchemasAsync` writes every schema into the
request-scoped local cache under `SchemaCacheKey(appId, schema.Id)`. Inside
`GetComponentsAsync`, the component lookup is
`appProvider.GetSchemaAsync(appId, schemaId, false, ct)`, which reads that exact same key
through `GetOrCreate`. Component schemas belong to the same app by construction, so every
one of those lookups is a local-cache hit. Zero database round trips, the loop just walks
an in-memory dictionary.

**The remaining path is real but small and not worth the risk.** `ContentEnricher` does
*not* pre-warm the cache, so a content query whose schema has component fields does pay one
round trip per distinct component schema, sequentially, on the first use in a request —
typically a handful.

Parallelising the resolver was considered and rejected. `GetComponentsAsync` is recursive
over a shared `Dictionary<DomainId, Schema>` and relies on inserting each schema *before*
recursing into it, which is what breaks reference cycles between component schemas.
Running the lookups concurrently would mean unsynchronised writes to that dictionary and
would lose the cycle guarantee, in exchange for saving a couple of milliseconds on a path
that only pays the cost once per request. `AppProvider.GetOrCreate` also has a
check-then-act race that concurrency would expose.

---

### 19. Header parsing re-split and re-allocated on every read — **FIXED**
`backend/src/Squidex.Domain.Apps.Entities/Context.cs`
`backend/src/Squidex.Domain.Apps.Entities/ContextHeaders.cs`

**Was:** `AsStrings` ran `value.Split(...).Select(x => x.Trim()).Distinct()` on every call —
a split array, two LINQ iterators and an internal `HashSet` each time. The same headers are
read repeatedly per request: `ConvertData.GenerateConverter` reads `Languages()` and
`ResolveUrls()` once per schema group, and `Fields()` is read from several steps. The
headers never change once a request is running.

**Now:** `Context` parses each header once into a `string[]` and keeps it.

```csharp
private readonly ConcurrentDictionary<string, string[]> headerValues = new (StringComparer.OrdinalIgnoreCase);
```

A `ConcurrentDictionary` rather than a plain one, because a `Context` is shared between the
parallel resolvers of a GraphQL query. The cache is cleared whenever `Headers` is assigned,
which is the only way it can change (`Context.Change`).

`Fields()` and `Languages()` still build their own `HashSet` per call, deliberately. Their
results are handed to callers that retain them — `Q.WithFields`, `ExcludeOtherFields` — so
returning a shared instance would let one caller mutate another's copy. Caching the parsed
`string[]` removes the expensive part while leaving ownership exactly as it was.

**This also fixed a latent crash.** The rewrite uses
`StringSplitOptions.RemoveEmptyEntries | TrimEntries`, which drops whitespace-only entries.
The old order — split, *then* trim — turned a header like `X-Languages: " , "` into a
single empty string, and `Language.GetLanguage("")` calls `Guard.NotNullOrEmpty` and
throws. Verified the difference against the runtime rather than assuming it.

**Verified:** a new `ContextHeadersTests` covering splitting, trimming, deduplication,
memoization (`Assert.Same`), invalidation on change and on removal, clone isolation, and
the whitespace case. Two of them **fail on the pre-fix code** — the memoization test and
the whitespace test — which are exactly the two behaviours that changed.
`Squidex.Domain.Apps.Entities.Tests` green (1540).

---

### Immutable `Context` (follow-up to 19)
`backend/src/Squidex.Domain.Apps.Entities/Context.cs`
`backend/src/Squidex.Domain.Apps.Entities/IContextProvider.cs`
`backend/src/Squidex.Web/ContextProvider.cs`

`Context` was mutable in two ways: `Headers { get; private set; }` changed by `Change()`,
and a public `App { get; set; }`. That is what forced the header cache added in item 19 to
carry invalidation logic.

**Now `IContextProvider.Context` has a setter and `Context` is immutable.** Both setters
are gone, along with `Change()` and `ICloneBuilder.Update()`; `Clone()` and a new
`WithApp()` return a new instance. The header cache needs no invalidation at all — a
`Context` parses each header at most once for its whole lifetime.

The three mutation sites in the codebase became replacements:

```csharp
contextProvider.Context = contextProvider.Context.WithApp(app);                       // AppCommandMiddleware
contextProvider.Context = contextProvider.Context.Clone(b => b.WithNoEnrichment()…);   // both bulk middlewares
```

`ContextProvider` stores it symmetrically to how it reads it — `HttpContext.Features` when
there is a request, the `AsyncLocal` fallback when there is not. `AppResolver` already
replaced the whole context this way, so the pattern was established.

**Why this is safe.** Replacing a reference is only equivalent to mutating in place if
nobody holds the old one. Every consumer of `IContextProvider` was checked:
`AssetCommandMiddleware`, `ContentCommandMiddleware`, `RuleCommandMiddleware`,
`EnrichWithAppIdCommandMiddleware` and both bulk middlewares all read
`contextProvider.Context` fresh at the point of use. None capture it in a field or across
an await that spans a replacement.

**Three existing tests failed and were right to.** Their doubles pinned the getter with
`A.CallTo(() => provider.Context).Returns(ctx)`, which made a *replacement* invisible while
the old in-place mutation had been visible. The fakes now assign (`provider.Context = ctx`)
so FakeItEasy tracks the property like the real provider, and
`AppCommandMiddlewareTests` asserts through `ApiContextProvider.Context.App` rather than
through a now-stale local reference.

**New `ContextProviderTests`** covers both storage paths: reading from and writing to
`HttpContext.Features`, header population, and the `AsyncLocal` fallback. Writing it
surfaced a trap worth knowing about — `A.Fake<IHttpContextAccessor>()` returns a *dummy*
`HttpContext` rather than `null`, so the fallback path is never reached unless the fake is
explicitly configured to return null.

**Verified:** build clean. Entities 1541, Web 176, Core 1243, Infrastructure 1033,
Data 180 — all green.

---

### 21. Content DTO link generation — **CLOSED: REJECTED**
`backend/src/Squidex/Areas/Api/Controllers/Contents/Models/ContentDto.cs:156`

`CreateLinksAsync` issues up to ten `IUrlHelper.Action` calls per content, so a 200-item
frontend page runs on the order of 2000 link generations.

**Rejected, not fixed.** The proposed fix — building URLs from a cached per-schema prefix
and concatenating the id — bypasses the ASP.NET routing system. Links would stop reflecting
the actual route table, so any change to a route template, a route constraint, or the path
base would silently produce wrong URLs. That is not a trade worth making for link
generation, whatever it costs. Recorded here so it is not re-reported as a new finding.

If this ever does show up in a profile, the answer has to stay inside the routing system —
for example ASP.NET's own `LinkGenerator` with a cached endpoint lookup — not around it.

---

### 22. The EF data layer never used `AsNoTracking` — **FIXED**
`backend/src/Squidex.Data.EntityFramework/ContentDbContext.cs`
`backend/src/Squidex.Data.EntityFramework/Infrastructure/Extensions.cs:116,150`
plus the entity-materializing reads in the content and asset repositories

**Was:** not a single `AsNoTracking()` in the layer and no `QueryTrackingBehavior` setting
anywhere. Every entity from every read query got a change-tracking snapshot — on entities
that carry a full content `Data` blob, so roughly double the memory per content read.

**Now, and the split matters:**

- `ContentDbContext` gets `QueryTrackingBehavior.NoTracking` as its **default**. That
  context is content-only, and every content write goes through `BulkInsertAsync`, never by
  mutating a queried entity.
- `AppDbContext` keeps its default, with `AsNoTracking()` applied to the individual read
  paths: both `QueryAsync` helpers in `Infrastructure/Extensions.cs` (which most repository
  reads funnel through), `EFContentRepository.FindContentAsync`,
  `EFAssetRepository.StreamAll`, the `ReadAllAsync` / single-read paths of the content, asset
  and asset-folder snapshot stores, `DynamicTables`, and both paths of the generic
  `EFSnapshotStore`.

**Why `AppDbContext` was not flipped globally — corrected.** The first version of this note
claimed ASP.NET Identity's `UserStore.SetTokenAsync` would silently stop persisting under a
global `NoTracking` default, because it assigns `token.Value = value` with no `Update` call.
**That was wrong**, and it was asserted from memory rather than checked. Tested against
Identity 10.0.6 + EF SQLite with the default flipped both ways: the token round trip and the
user update both persist correctly. The reason is that the EF `UserStore` reaches tokens via
`DbSet.FindAsync`, and `Find`/`FindAsync` track the entity regardless of
`QueryTrackingBehavior` — they are not LINQ queries.

**The real reason, found by auditing the shared libraries** (`D:\squidex-tools\libs`).
`AppDbContext` is not only Squidex's own repositories — `OnModelCreating` also mounts
`UseOpenIddict()`, `UseAssetKeyValueStore` (Tus), `UseChatStore()`, `UseFlows()`,
`UseCronJobs()`, `UseMessagingDataStore()`, `UseMessagingTransport()` and Identity. Two of
those stores read an entity with a **LINQ query**, mutate it, and call `SaveChanges` with no
`Update`, which is exactly the pattern a `NoTracking` default turns into a silent no-op:

| Store | Code | Effect under a global `NoTracking` default |
| --- | --- | --- |
| `Squidex.AI.EntityFramework/EFChatStore.SetAsync` | `Where(...).FirstOrDefaultAsync()` then `entity.Value = json` | conversation updates never persist |
| `Squidex.Messaging.EntityFramework/EFSubscription` | `query.FirstOrDefaultAsync()` then `efMessage.TimeHandled = now` | **message is never marked handled** |

The messaging one is the blocker. That assignment *is* the queue's claim on a message, and
the `DbUpdateConcurrencyException` it can raise is the only thing stopping two processes
consuming the same message. With no tracked change, `SaveChangesAsync` issues no UPDATE, so
`TimeHandled` stays null, the concurrency guard can never fire, the callback still runs, and
the next poll matches the same row again — silent infinite redelivery plus duplicate
processing across processes, with no exception anywhere.

Everything else audited clean: `EFCronJobStore`, `EFAssetKeyValueStore`, `EFEventStore`,
`EFMessagingDataStore` and `EFTransport` all `AddAsync` new entities; `EFFlowStateStore` uses
`ExecuteUpdateAsync` and bulk upsert; OpenIddict uses explicit `Attach` + `Update`; Identity
was verified empirically (see above) and calls `_userStore.Update(user)` explicitly.

**So the flip is two one-line fixes away.** Adding `dbContext.Update(entity)` before
`SaveChangesAsync` in those two stores would make `AppDbContext` safe to default to
`NoTracking` — and would also remove a latent fragility, since both currently depend on the
tracking configuration of a `DbContext` the library does not own.

`ContentDbContext` has no such tenants, which is what makes the global flip safe there.

**Caveat on the explicit approach, which is real.** Enumerating read sites is fragile: a
later sweep found five more entity-materializing reads that the first pass missed —
`EFAssetFolderRepository_SnapshotStore` (both paths), `DynamicTables`, and both paths of the
generic `EFSnapshotStore`, which backs *every* domain object snapshot and streams the whole
table on a rebuild. Those have been fixed too, but a global default would not have needed
finding them.

The `ReadAllAsync` streams were the worst individual case: they walk every content or asset
in the database for a rebuild, so tracking retained the entire table in the change tracker.

All seven `SaveChangesAsync` call sites in the layer were checked first — every one
constructs a new entity and `Add`s or bulk-inserts it. None mutate a queried entity, which
is what makes the change safe.

---

### 24. Queries by id spent an extra round trip counting a bounded set — **FIXED**
`backend/src/Squidex.Data.MongoDb/Infrastructure/Queries/LimitExtensions.cs:16`
`backend/src/Squidex.Data.MongoDb/Domain/Apps/Entities/Contents/Operations/QueryByIds.cs:59`
`backend/src/Squidex.Data.MongoDb/Domain/Apps/Entities/Assets/MongoAssetRepository.cs:112`

**Was:** both id-query paths ran `CountDocumentsAsync` to get the total even though the
filter is `In(ids)`. Since `ContentQueryParser.WithPaging` sets `Take = q.Ids.Count` for id
queries, the guard fired whenever every requested id was found — the normal case — so this
was an extra round trip on the reference-resolution path.

**Now:** a shared predicate decides when a count can tell you anything new.

```csharp
public static bool NeedsTotalById(this ClrQuery query, int idCount)
{
    return query.Skip > 0 || query.Take < idCount || query.Random > 0;
}
```

**The `Random` term is the non-obvious one.** Both paths finish through
`ToListRandomAsync`, which — when `query.Random > 0` — returns a random *sample* of the
matches rather than all of them. In that case the returned count is not the match count, so
the count query is still required. The first version of this fix omitted that and would have
reported the sample size as the total.

`NoTotal` semantics are unchanged: it still short-circuits to `-1` before this predicate is
consulted, rather than opportunistically returning a total the caller asked not to have.

---

### 25. `ResolvingAssets()` re-evaluated per content — **FIXED**
`backend/src/Squidex.Domain.Apps.Entities/Contents/Queries/Steps/ResolveAssets.cs:129`

The same defect as item 17, in the sibling step: `AddAssetIds` called the lazy
`schema.ResolvingAssets()` inside the per-content loop, rescanning every field of the schema
and allocating two LINQ iterators per content. Hoisted to a single `ToList()` above the loop.

---

### 26. `CalculatePreviewText` filtered all schema fields once per content — **FIXED**
`backend/src/Squidex.Domain.Apps.Entities/Contents/Queries/Steps/CalculatePreviewText.cs:31`

`schema.Fields.Where(x => x.RawProperties is RichTextFieldProperties)` sat in the inner loop,
re-scanning every field for every content to produce a list identical for the whole group.
Hoisted, with an early return when the schema has no rich-text fields at all — which is the
common case and previously still paid a full field scan per content.

---

### 27. `EnrichForCaching` re-added the same schema and app dependency per content — **FIXED**
`backend/src/Squidex.Domain.Apps.Entities/Contents/Queries/Steps/EnrichForCaching.cs`

**Was:** all three `AddDependency` calls sat in the per-content loop, but only the content one
varies. The other two re-added a key already in the set, so `CachingManager` took its lock and
did a `HashSet.Add` that returned false — a 200-item page paid ~600 lock acquisitions to do
~202 useful ones.

**Now:** the app and schema dependencies are added once per schema group.

**They were deliberately left *inside* the group loop rather than hoisted to the top of the
method.** Hoisting looks tidier but changes behaviour for an empty result: with no contents
there are no groups, so today nothing is added, `hasDependency` stays false, and the response
gets no ETag. Adding the app dependency unconditionally would start emitting an ETag for
empty responses — a change in caching behaviour that has nothing to do with this finding.
Once per group is still 1 instead of 200 for the normal single-schema query.

---

### Verification note for items 22 and 24

Build clean; `Squidex.Domain.Apps.Entities.Tests` (1541), `Squidex.Domain.Apps.Core.Tests`
(1243), `Squidex.Infrastructure.Tests` (1033), `Squidex.Web.Tests` (176) and the runnable part
of `Squidex.Data.Tests` (180) are all green.

**That green is weaker than it looks for items 22 and 24.** `Squidex.Data.Tests` contains
~1349 tests, of which only 180 run without the `Dependencies` / `TestContainer` categories —
the ~1169 excluded ones are exactly the EF and MongoDB integration tests that would actually
exercise `AsNoTracking` and `NeedsTotalById` against a real database. Those two items are
reasoned-correct and compile, but they are **not covered by any test that was run here**.
They should be validated against a container run before release.

Items 25, 26 and 27 are pure hoists with no behavioural change and are covered by the
enrichment tests that did run.

---

### 28. Asset downloads used an exception as the legacy-path fallback — **FIXED**
`backend/src/Squidex.Domain.Apps.Entities/Assets/DefaultAssetFileStore.cs`

**Was:** `GetFileSizeAsync` and `DownloadAsync` tried the current file name, caught
`AssetNotFoundException`, and retried with the legacy name (no app ID). On an instance that
still holds assets under the old scheme, *every* access to those assets threw and caught
first — and against a cloud store the failed attempt is a full network round trip, so the
fallback roughly doubled the latency of every legacy asset served.

**Now:** the outcome is remembered per asset in the injected `IMemoryCache`, keyed by
`(typeof(DefaultAssetFileStore), appId, id)` with a one hour sliding lifetime, so the wrong
name is only tried once. `IMemoryCache` rather than `Squidex.Caching.LRUCache` because it is
thread safe on its own — see item 7 for what `LRUCache` does under concurrent access.

**The memo is a hint, not a decision.** `FileNames(...)` returns both names ordered by what
was last seen to work, and the other one is still tried on failure. That matters because an
asset can move between schemes — a migration, or an eviction followed by a re-probe — and a
cache that *decided* rather than *hinted* would turn a stale entry into a hard failure. The
cost of a wrong hint is one extra round trip, exactly what the code did before.

Two things fell out of it: the `options.FolderPerApp` case now short-circuits to a single
name with no try/catch at all, and the partial-write hazard flagged in the finding (a retry
appending to a stream the first attempt already wrote to) is now hit far less often, since a
warm asset takes the right branch first. It is not *fixed* — that would need the asset store
to guarantee it writes nothing before failing.

---

### 29. Removing items while iterating a `JsonArray` was quadratic — **FIXED**
`backend/src/Squidex.Domain.Apps.Core.Operations/ConvertContent/ContentConverter.cs:145,175`

**Was:** `ConvertArray` and `ConvertComponents` both removed in place with
`array.RemoveAt(i); i--;`. `JsonArray` derives from `List<JsonValue>`, so each removal shifts
every following element — dropping *k* of *n* items costs O(n·k), and the case where many
items are dropped (entries referencing deleted component schemas) is exactly the case where
the array is large.

**Now:** a single compaction pass with a write index, then one `RemoveRange` for the tail.

```csharp
var target = 0;

for (var i = 0; i < array.Count; i++)
{
    var oldValue = array[i];

    var (removed, newValue) = ConvertArrayItem(field, oldValue);
    if (removed)
    {
        continue;
    }

    array[target] = ReferenceEquals(newValue.Value, oldValue.Value) ? oldValue : newValue;
    target++;
}

array.RemoveRange(target, array.Count - target);
```

The write index is always `<= i`, so a slot is only ever overwritten after it has been read —
no read-after-write hazard, and the surviving order is preserved.

**Verified with new tests** — `ContentConversionRemovalTests`, 27 cases covering nine removal
patterns (none, first, middle, last, adjacent pairs, alternating, all) across three ways an
item gets dropped: a non-object in an array, a component of an unknown schema, and a
component with no discriminator.

Two checks on the tests themselves, because this is a behaviour-preserving rewrite rather
than a bug fix:

- They pass against **both** the original `RemoveAt` implementation and the new one, which is
  the property that actually matters here — they pin the contract rather than the code.
- Mutation check: deleting the `RemoveRange` line fails 24 of the 27, so they are not
  vacuous.

A first attempt at these tests drove removal through a custom `IContentItemConverter` that
stripped the discriminator; that never removed anything, because `ConvertComponent` checks
the discriminator *before* calling `ConvertNested`. The tests now use inherently invalid
items, which is both simpler and closer to the real cause.

---

### 30. `stream.ToArray()` copied straight back out of the pooled buffer — **FIXED**
`backend/src/Squidex.Domain.Apps.Entities/Assets/Transformations.cs:79`

**Was:** `GetTextAsync` downloaded into a `DefaultPools.MemoryStream`
(`RecyclableMemoryStreamManager`) and then called `ToArray()`, allocating a fresh array of
the whole file and copying the pooled buffer into it — for a file at the 4 MB limit, straight
onto the large object heap on every call.

**Now:**

```csharp
var bytes = new ReadOnlySpan<byte>(stream.GetBuffer(), 0, (int)stream.Length);
```

`Convert.ToBase64String` and `Encoding.GetString` all have `ReadOnlySpan<byte>` overloads, so
nothing downstream changed.

Worth being precise about why `GetBuffer` is better rather than just "avoids a copy":
`RecyclableMemoryStream.GetBuffer()` still consolidates into a single contiguous buffer when
the stream spans several blocks. The difference is that the buffer it returns comes from the
pool and goes back on dispose, whereas `ToArray` allocates a new GC array every time.
`RecyclableMemoryStream` documents `ToArray` as the call to avoid for exactly this reason.

---

### 23. Full-text search loads a fixed 1000 ids — **CLOSED: BOUNDED, NOT FIXED**
`backend/src/Squidex.Domain.Apps.Entities/Contents/Queries/ContentQueryParser.cs:93`
`backend/src/Squidex.Domain.Apps.Entities/Contents/ContentsOptions.cs`
`backend/src/Squidex/appsettings.json`

**Was:** `new TextQuery(query.FullText, 1000)` — a hardcoded literal. The ids come back and go
into an `In("id", …)` filter, so a query matching more than 1000 items silently loses the
rest, relevance order is discarded, and up to 1000 GUID strings travel to the database on
every page.

**Now:** the limit is `ContentsOptions.MaxFullTextResults`, configurable as
`contents:maxFullTextResults` and documented in `appsettings.json` with what raising it costs.
Default unchanged at 1000.

**Why this is closed as bounded rather than fixed.** I proposed paging the text index and
walked it back after working through the constraint: **the full text index and the content
store are separate, independently configured stores**, and any combination is legal —
Mongo+Mongo, Elastic+Mongo, Elastic+SQL, Azure+anything. `MongoContentRepository` takes
`store:mongoDb:contentDatabase` while the text index resolves the default `IMongoDatabase`;
in EF the index is on `AppDbContext` and contents are on `ContentDbContext`.

So this is a cross-store join, always, and the 1000 is not a magic number — it is the join
buffer. No value for it is correct, because how many survive depends on a filter the index
has never seen.

Paging the index only works when the index alone decides both membership *and* order. It
does not, in two common cases:

| Case | Ids that must cross the boundary |
| --- | --- |
| search only, relevance-ordered | the page (~20) |
| search + explicit `$orderby` | all matches |
| search + `$filter` | all matches, or an iterative top-up |

And the default sort is `lastModified` — `WithSorting` adds it when the caller gives none —
so today's pipeline is already "the 1000 most relevant, displayed newest first", which is
neither. Making paging work would mean changing the default sort for full-text queries to
relevance: a behaviour change, not an optimisation.

There is also no architectural escape. Pushing the filter into the index means indexing
arbitrary user-filterable fields — reimplementing the query engine on the search side.
Pushing relevance into the content store means the store needs the scores. Either way the
boundary just moves, and because the backends pair arbitrarily you would owe it for every
combination.

**Left undone, deliberately, and worth knowing about:** a truncated result is still
indistinguishable from a complete one. A caller paging a 5000-hit search gets a confident
wrong total and silently loses the remainder. Returning `total = -1` when the cap is hit —
the codebase's existing "unknown" convention, used by `NoTotal` — would make it visible
without any interface change. Defaulting full-text queries to relevance order is arguably a
bug fix on its own.

---

### 31. Every request rebuilt the caller's permission set — **FIXED**
`backend/src/Squidex.Domain.Apps.Core.Model/Apps/Roles.cs`

**Was:** `AppResolver` runs on every API request and resolves the caller's role through
`Roles.TryGet` → `Role.ForApp(app, isFrontend)`, which rebuilt the permission set each time:
a prefix `Permission` (three `string.Replace` calls), then a concatenated string and a
`Permission` per role permission, ten more for a frontend caller, plus a `HashSet`, a
`PermissionSet` and a `Role`.

**Now:** `Roles` memoizes the resolved role in a `ConcurrentDictionary` keyed by
(app, name, isFrontend). `Role` is a record and immutable, so the result is a pure function
of that key.

**The cache lives on `Roles`, not on `Role`, for two reasons.** `Role` is a `record`, so its
synthesized `Equals`/`GetHashCode` cover every instance field — adding a cache field would
make two logically equal roles compare unequal. And `Roles` instances hang off the cached
`App`, so the natural lifetime is already right.

**It is bounded, and that is not cosmetic.** `App.Roles` defaults to the *shared static*
`Roles.Empty`, so for every app without custom roles the cache lives on one instance shared
across all tenants and would grow with the number of apps. It is capped at 1000 entries and
cleared wholesale on overflow; hitting the cap degrades to the old behaviour rather than
leaking. An app with its own roles has its own `Roles` instance and never approaches it.

`Microsoft.Extensions.Caching.Memory` would have been the nicer bound, but
`Squidex.Domain.Apps.Core.Model` is a pure model project with no caching dependency and it
did not seem worth adding one there.

---

### 32. Any app change threw away the whole GraphQL schema — **FIXED**
`backend/src/Squidex.Domain.Apps.Entities/Contents/GraphQL/CachingGraphQLResolver.cs:67`

**Was:** the cache key was `(typeof(CachingGraphQLResolver), app.Id, app.Version)`.
`app.Version` bumps on *any* app event — a contributor, a client, a role, a setting — so each
one was a cold miss, and the next GraphQL request paid a full `BuildSchema`: a content type, a
result type and a component type per schema, each initialised with a GraphQL field per schema
field, plus queries, mutations and a `FieldMap`.

**Now:** the key is `(typeof(CachingGraphQLResolver), app.Id)`.

**The version was redundant, not load-bearing.** The entry is already created with a
validator, and `SchemasHashKey.Create` builds its dictionary starting with
`[app.Id] = app.Version` before adding every schema version. So the app version was in the
validator all along — having it in the key too meant app changes could never *reach* the
validator, they just missed.

The behavioural difference is which path a change takes: an app-level change now goes through
the validator like a schema change does — the cached schema is served and refreshed — instead
of blocking the next request on a rebuild. That is the same eventual-consistency trade the
design already makes for schema changes, which are the more visible ones.

---

### 33. Asset and content tokens serialized an object per item — **FIXED**
`backend/src/Squidex.Domain.Apps.Entities/Assets/Queries/Steps/CalculateTokens.cs`
`backend/src/Squidex.Domain.Apps.Entities/Contents/Queries/Steps/CalculateTokens.cs`

**Was:** both steps allocated a fresh anonymous object per item to hold the edit token, when
only one or two of its fields actually vary:

```csharp
foreach (var asset in assets)
{
    var token = new { a = asset.AppId.Name, i = asset.Id.ToString(), u = url };

    asset.EditToken = Convert.ToBase64String(serializer.SerializeToBytes(token));
}
```

**Now:** a private `Token` class is created once per call and its properties are assigned per
item. The short wire names are kept with `[JsonPropertyName]`, so the properties can have
readable names without changing the format.

**There is a content version of this too**, which the original finding missed — it carries a
fourth field (`s`, the schema name) and sits on the content list path, which is hotter than
the asset one. Both are fixed.

**The wire format is load-bearing and was pinned first.** The token is base64 of a JSON object
with single-letter keys, decoded by the frontend, and the existing tests only asserted
`EditToken != null` — nothing covered the shape. So a test asserting the exact decoded string
was added and confirmed green against the *old* code before the change, then again after:

```csharp
var expected = $$"""{"a":"{{asset.AppId.Name}}","i":"{{asset.Id}}","u":"https://squidex.io"}""";
```

This removes the per-item object allocation, not the per-item serialization — the serializer
still runs once per item. Emitting the constant prefix once and varying only the id would go
further, at the cost of hand-writing JSON.

---

### 35. `JobWorker` cached a faulted task for the process lifetime — **FIXED**
`backend/src/Squidex.Domain.Apps.Entities/Jobs/JobWorker.cs`

**Was:** `processors.GetOrAdd(appId, async key => …)` stored the `Task<JobProcessor>`, so a
transient failure in `LoadAsync` was cached permanently — jobs for that app never ran again,
and the failure was invisible because every caller saw the *same* exception rather than a new
one. The same defect as item 15.

**Now:** the entry is removed when the task faults, comparing by reference so a newer
successful entry added by another caller is not discarded. The removal takes the same lock
that guards the dictionary.
