# Backend Performance — Resolved

Items from the backend performance review that are done. Numbering matches
[todo.md](todo.md) — resolved items keep their original number so references stay valid.

Partially-addressed items (**8**, **9**) stay in `todo.md` until closed.

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
