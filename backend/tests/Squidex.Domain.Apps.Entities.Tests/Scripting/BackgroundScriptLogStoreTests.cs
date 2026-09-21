// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Scripting.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Scripting;

public class BackgroundScriptLogStoreTests : GivenContext
{
    private readonly IScriptLogRepository repository = A.Fake<IScriptLogRepository>();
    private readonly IClock clock = A.Fake<IClock>();
    private readonly ScriptLogOptions options = new ScriptLogOptions();
    private readonly Instant now = SystemClock.Instance.GetCurrentInstant();
    private readonly BackgroundScriptLogStore sut;

    public BackgroundScriptLogStoreTests()
    {
        A.CallTo(() => clock.GetCurrentInstant())
            .Returns(now);

        sut = new BackgroundScriptLogStore(Options.Create(options), repository, A.Fake<ILogger<BackgroundScriptLogStore>>())
        {
            Clock = clock,
        };
    }

    [Fact]
    public async Task Should_write_log_with_context()
    {
        var records = new List<ScriptLogRecord>();

        A.CallTo(() => repository.InsertManyAsync(A<IEnumerable<ScriptLogRecord>>._, A<CancellationToken>._))
            .Invokes(x => records.AddRange(x.GetArgument<IEnumerable<ScriptLogRecord>>(0)!));

        sut.Log(AppId.Id, "contents/my-schema/create", CreateLog("Hello"));

        await WaitForCompletion();

        var record = Assert.Single(records);

        Assert.Equal(AppId.Id, record.AppId);
        Assert.Equal("contents/my-schema/create", record.Name);
        Assert.Equal(now, record.Timestamp);
        Assert.Equal([new ScriptLogEntry("log", "Hello")], record.Entries);
        Assert.Equal(1, record.TotalEntries);
    }

    [Fact]
    public async Task Should_trim_logs_of_written_apps()
    {
        var otherAppId = DomainId.NewGuid();

        sut.Log(AppId.Id, "assets/create", CreateLog("Hello"));
        sut.Log(AppId.Id, "assets/create", CreateLog("Hello"));
        sut.Log(otherAppId, "assets/create", CreateLog("Hello"));

        await WaitForCompletion();

        A.CallTo(() => repository.TrimAsync(AppId.Id, options.MaxItemsPerApp, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => repository.TrimAsync(otherAppId, options.MaxItemsPerApp, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_delete_old_logs()
    {
        sut.Log(AppId.Id, "assets/create", CreateLog("Hello"));

        await WaitForCompletion();

        A.CallTo(() => repository.DeleteOlderThanAsync(now.Minus(Duration.FromDays(options.RetentionInDays)), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_not_write_log_without_entries()
    {
        using var log = ScriptLog.Begin("my-script");

        sut.Log(AppId.Id, "assets/create", log);

        await WaitForCompletion();

        A.CallTo(() => repository.InsertManyAsync(A<IEnumerable<ScriptLogRecord>>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_write_log_if_disabled()
    {
        options.Enabled = false;

        sut.Log(AppId.Id, "assets/create", CreateLog("Hello"));

        await WaitForCompletion();

        A.CallTo(() => repository.InsertManyAsync(A<IEnumerable<ScriptLogRecord>>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_drop_logs_if_queue_is_full()
    {
        options.MaxPendingItems = 2;

        var records = new List<ScriptLogRecord>();

        A.CallTo(() => repository.InsertManyAsync(A<IEnumerable<ScriptLogRecord>>._, A<CancellationToken>._))
            .Invokes(x => records.AddRange(x.GetArgument<IEnumerable<ScriptLogRecord>>(0)!));

        for (var i = 0; i < 5; i++)
        {
            sut.Log(AppId.Id, "assets/create", CreateLog("Hello"));
        }

        await WaitForCompletion();

        Assert.Equal(2, records.Count);
    }

    [Fact]
    public async Task Should_query_logs_from_repository()
    {
        var records = new List<ScriptLogRecord> { new ScriptLogRecord() };

        A.CallTo(() => repository.QueryAsync(AppId.Id, "contents", 10, 20, CancellationToken))
            .Returns(records);

        var actual = await sut.QueryAsync(AppId.Id, "contents", 10, 20, CancellationToken);

        Assert.Same(records, actual);
    }

    [Fact]
    public async Task Should_limit_query_to_max_items()
    {
        await sut.QueryAsync(AppId.Id, null, -5, 1000, CancellationToken);

        A.CallTo(() => repository.QueryAsync(AppId.Id, null, 0, options.MaxItemsPerApp, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_query_logs_if_disabled()
    {
        options.Enabled = false;

        var actual = await sut.QueryAsync(AppId.Id, null, 0, 20, CancellationToken);

        Assert.Empty(actual);

        A.CallTo(() => repository.QueryAsync(A<DomainId>._, A<string?>._, A<int>._, A<int>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_delete_logs_if_app_is_deleted()
    {
        await ((IDeleter)sut).DeleteAppAsync(App, CancellationToken);

        A.CallTo(() => repository.DeleteAsync(AppId.Id, CancellationToken))
            .MustHaveHappened();
    }

    private static ScriptLog CreateLog(string message)
    {
        using var log = ScriptLog.Begin("my-script");

        log.Add("log", message);

        return log;
    }

    private async Task WaitForCompletion()
    {
        sut.Next();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        while (sut.HasPendingJobs)
        {
            cts.Token.ThrowIfCancellationRequested();

            await Task.Delay(20, cts.Token);
        }

        sut.Dispose();
    }
}
