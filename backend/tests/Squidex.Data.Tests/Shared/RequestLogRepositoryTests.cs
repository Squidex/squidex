// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure.Log;

namespace Squidex.Shared;

public abstract class RequestLogRepositoryTests
{
    protected abstract Task<IRequestLogRepository> CreateSutAsync();

    [Fact]
    public async Task Should_delete_by_key()
    {
        var sut = await CreateSutAsync();

        var key = Guid.NewGuid().ToString();

        var now = SystemClock.Instance.GetCurrentInstant();
        var timeMin = now.Minus(Duration.FromDays(300));
        var timeMax = now.Plus(Duration.FromDays(300));

        await sut.InsertManyAsync([
            new Request { Key = key },
            new Request { Key = key },
        ]);

        await sut.DeleteAsync(key);

        var found = await sut.QueryAllAsync(key, timeMin, timeMax).ToListAsync();

        Assert.DoesNotContain(found, x => x.Key == key);
    }

    [Fact]
    public async Task Should_query_by_several_factory()
    {
        var sut = await CreateSutAsync();

        var key1 = Guid.NewGuid().ToString();
        var key2 = Guid.NewGuid().ToString();

        var now = SystemClock.Instance.GetCurrentInstant();
        var timeMin = now.Minus(Duration.FromDays(300));
        var timeMax = now.Plus(Duration.FromDays(300));

        await sut.InsertManyAsync([
            new Request
            {
                Key = key1,
                Timestamp = now,
            },
            new Request
            {
                Key = key1,
                Timestamp = now.Plus(Duration.FromHours(1)),
            },
            new Request
            {
                Key = key1,
                Timestamp = now.Plus(Duration.FromHours(2)),
            },
            new Request
            {
                Key = key1,
                Timestamp = now.Plus(Duration.FromHours(3)),
            },
            new Request
            {
                Key = key2,
                Timestamp = now,
            },
            new Request
            {
                Key = key2,
                Timestamp = now.Plus(Duration.FromHours(1)),
            },
        ]);

        var byKey1 = await sut.QueryAllAsync(key1, timeMin, timeMax).ToListAsync();
        Assert.Equal(4, byKey1.Count);

        var byKey2 = await sut.QueryAllAsync(key2, timeMin, timeMax).ToListAsync();
        Assert.Equal(2, byKey2.Count);

        var byDate = await sut.QueryAllAsync(key1, now, now.Plus(Duration.FromDays(2))).ToListAsync();
        Assert.Equal(4, byDate.Count);

        var outOfDate = await sut.QueryAllAsync(key1, timeMin, now.Minus(Duration.FromDays(2))).ToListAsync();
        Assert.Empty(outOfDate);
    }
}
