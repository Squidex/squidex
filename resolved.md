# Backend Performance — Resolved

Items from the backend performance review that are done. Numbering matches
[todo.md](todo.md) — resolved items keep their original number so references stay valid.

Item **6** is not listed here: it is open but accepted as won't-fix, and stays documented
in `todo.md` so it is not re-reported as a new finding.

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
