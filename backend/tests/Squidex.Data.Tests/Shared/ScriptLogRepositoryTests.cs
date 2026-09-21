// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Scripting;
using Squidex.Domain.Apps.Entities.Scripting.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Shared;

public abstract class ScriptLogRepositoryTests
{
    private readonly Instant now = Instant.FromUnixTimeSeconds(SystemClock.Instance.GetCurrentInstant().ToUnixTimeSeconds());

    protected abstract Task<IScriptLogRepository> CreateSutAsync();

    [Fact]
    public async Task Should_insert_and_query_logs()
    {
        var sut = await CreateSutAsync();

        var appId = DomainId.NewGuid();

        var record = CreateRecord(appId, "contents/my-schema/create", 0);

        await sut.InsertManyAsync([record]);

        var actual = await sut.QueryAsync(appId, null, 0, 10);

        actual.Should().BeEquivalentTo([record]);
    }

    [Fact]
    public async Task Should_query_newest_first()
    {
        var sut = await CreateSutAsync();

        var appId = DomainId.NewGuid();

        await sut.InsertManyAsync(
        [
            CreateRecord(appId, "assets/create", 1),
            CreateRecord(appId, "assets/create", 3),
            CreateRecord(appId, "assets/create", 2),
        ]);

        var actual = await sut.QueryAsync(appId, null, 1, 10);

        Assert.Equal([now.Plus(Duration.FromSeconds(2)), now.Plus(Duration.FromSeconds(1))], actual.Select(x => x.Timestamp));
    }

    [Fact]
    public async Task Should_query_by_name_prefix()
    {
        var sut = await CreateSutAsync();

        var appId = DomainId.NewGuid();

        await sut.InsertManyAsync(
        [
            CreateRecord(appId, "contents/schema1/create", 1),
            CreateRecord(appId, "contents/schema1/update", 2),
            CreateRecord(appId, "contents/schema2/create", 3),
            CreateRecord(appId, "assets/create", 4),
        ]);

        var actual = await sut.QueryAsync(appId, "contents/schema1", 0, 10);

        Assert.Equal(["contents/schema1/update", "contents/schema1/create"], actual.Select(x => x.Name));
    }

    [Fact]
    public async Task Should_trim_to_max_count()
    {
        var sut = await CreateSutAsync();

        var appId = DomainId.NewGuid();
        var appIdOther = DomainId.NewGuid();

        await sut.InsertManyAsync(Enumerable.Range(0, 10).Select(i => CreateRecord(appId, "assets/create", i)));
        await sut.InsertManyAsync(Enumerable.Range(0, 10).Select(i => CreateRecord(appIdOther, "assets/create", i)));

        await sut.TrimAsync(appId, 3);

        var actual = await sut.QueryAsync(appId, null, 0, 100);

        Assert.Equal(
        [
            now.Plus(Duration.FromSeconds(9)),
            now.Plus(Duration.FromSeconds(8)),
            now.Plus(Duration.FromSeconds(7)),
        ], actual.Select(x => x.Timestamp));

        var other = await sut.QueryAsync(appIdOther, null, 0, 100);

        Assert.Equal(10, other.Count);
    }

    [Fact]
    public async Task Should_not_trim_if_below_max_count()
    {
        var sut = await CreateSutAsync();

        var appId = DomainId.NewGuid();

        await sut.InsertManyAsync(Enumerable.Range(0, 3).Select(i => CreateRecord(appId, "assets/create", i)));

        await sut.TrimAsync(appId, 3);

        var actual = await sut.QueryAsync(appId, null, 0, 100);

        Assert.Equal(3, actual.Count);
    }

    [Fact]
    public async Task Should_delete_old_logs()
    {
        var sut = await CreateSutAsync();

        var appId = DomainId.NewGuid();

        await sut.InsertManyAsync(Enumerable.Range(0, 5).Select(i => CreateRecord(appId, "assets/create", i)));

        await sut.DeleteOlderThanAsync(now.Plus(Duration.FromSeconds(3)));

        var actual = await sut.QueryAsync(appId, null, 0, 100);

        Assert.Equal(2, actual.Count);
    }

    [Fact]
    public async Task Should_delete_logs_of_app()
    {
        var sut = await CreateSutAsync();

        var appId = DomainId.NewGuid();

        await sut.InsertManyAsync(Enumerable.Range(0, 5).Select(i => CreateRecord(appId, "assets/create", i)));

        await sut.DeleteAsync(appId);

        var actual = await sut.QueryAsync(appId, null, 0, 100);

        Assert.Empty(actual);
    }

    private ScriptLogRecord CreateRecord(DomainId appId, string name, int seconds)
    {
        return new ScriptLogRecord
        {
            AppId = appId,
            Name = name,
            Entries =
            [
                new ScriptLogEntry("log", "Hello"),
                new ScriptLogEntry("warn", "World"),
            ],
            TotalEntries = 5,
            Timestamp = now.Plus(Duration.FromSeconds(seconds)),
        };
    }
}
