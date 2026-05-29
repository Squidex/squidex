## Summary
- Updated ai-track-docs/build-test.md with exact verified build and test commands for the chosen module (Squidex.Domain.Apps.Core.Model)
- Added `Should_enumerate_all_configured_language_keys` to LanguagesConfigTests.cs — verifies `AllKeys` enumerates all configured language codes after sequential `Set` calls
- Files touched: ai-track-docs/build-test.md, backend/tests/Squidex.Domain.Apps.Core.Tests/Model/Apps/LanguagesConfigTests.cs

## Evidence
- Tests/logs/metrics:
  ```
  dotnet test tests/Squidex.Domain.Apps.Core.Tests/Squidex.Domain.Apps.Core.Tests.csproj \
    --filter "Should_enumerate_all_configured_language_keys"
  → Passed! - Failed: 0, Passed: 1, Skipped: 0, Total: 1, Duration: 41 ms
  ```

## Risk & Rollback
- Risk: low
- Rollback: git revert 5a53520efda1d18c4b8332c578d146472bb71fff

## Track
- Level: Crawl
- Exercise: Ex2
