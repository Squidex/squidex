# Backend Improvements

All four items are fixed on `migration-improvements`.

## 1. Protected assets were served with `Cache-Control: public` — done

`AssetContentController` now sends `private,no-store` when `asset.IsProtected`, so a CDN or shared
proxy can no longer store the file under a URL that needs no credentials. The client-supplied
`?cache=` value is clamped to `assets:maxCacheDuration` (default 365 days).

## 2. GraphQL had no depth or complexity limit — done

`AddSquidexGraphQL` registers the complexity analyzer from `graphQL:maxDepth` (default 30, 0
disables) and `graphQL:maxComplexity` (default 0 — off, because the factor depends on the schema).
Apollo tracing instrumented every field of every request; it is now behind
`graphQL:enableTracing`, off by default.

## 3. Image resizing had no deduplication and no concurrency cap — done

New `AssetResizeGate` serializes per `(appId, assetId, fileVersion, suffix)` and bounds the total
number of parallel resizes via `assets:maxConcurrentResizes` (default: number of cores). Requests
that lose the race re-check the file store after the gate instead of decoding the image again.
Applied to both `AssetContentController` and `AppImageController`.

## 4. In-process caches had no size limit — done

`UsageGate`, `AssetUsageTracker` and the extensions' `ClientPool` now set `SizeLimit` on their
`MemoryCache` and a `Size` on every entry, so they no longer grow with the number of tenants,
assets or client configurations.
