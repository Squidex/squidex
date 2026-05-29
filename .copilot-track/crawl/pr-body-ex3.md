## Summary
- Collapsed 3 block-getter properties (`Master`, `AllKeys`, `Values`) to expression-body form (`=> expr`) in LanguagesConfig.cs
- Collapsed 2 single-return methods (`IsMaster`, `Contains`) to expression-body form
- Pure readability refactor — no logic, no behavior, no API surface changed
- Files touched: backend/src/Squidex.Domain.Apps.Core.Model/Apps/LanguagesConfig.cs

## Evidence
- Tests/logs/metrics:
  ```
  dotnet test tests/Squidex.Domain.Apps.Core.Tests/Squidex.Domain.Apps.Core.Tests.csproj \
    --filter "FullyQualifiedName~LanguagesConfigTests"
  → Passed! - Failed: 0, Passed: 20, Skipped: 0, Total: 20, Duration: 76 ms
  ```

## Risk & Rollback
- Risk: low
- Rollback: git revert 0a3f9c4c38d25565cb894ab8849f02fdbd588f37

## Track
- Level: Crawl
- Exercise: Ex3
