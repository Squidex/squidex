// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using LoremNET;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Shared;

public abstract class SnapshotStoreTests
{
    protected abstract Task<ISnapshotStore<TestValue>> CreateSutAsync();

    [Fact]
    public async Task Should_insert_value()
    {
        var sut = await CreateSutAsync();

        var sourceId = DomainId.NewGuid();
        var sourceValue = new TestValue { Value = $"{sourceId}" };

        await sut.WriteAsync(new SnapshotWriteJob<TestValue>(sourceId, sourceValue, 0));

        var expected = await sut.ReadAsync(sourceId);

        Assert.Equal(sourceValue, expected.Value);
    }

    [Fact]
    public async Task Should_update_value()
    {
        var sut = await CreateSutAsync();

        var sourceId = DomainId.NewGuid();
        var sourceValue0 = new TestValue { Value = $"{sourceId}_0" };
        var sourceValue1 = new TestValue { Value = $"{sourceId}_1" };

        await sut.WriteAsync(new SnapshotWriteJob<TestValue>(sourceId, sourceValue0, 0));
        await sut.WriteAsync(new SnapshotWriteJob<TestValue>(sourceId, sourceValue1, 1));

        var found = await sut.ReadAsync(sourceId);

        Assert.Equal(sourceValue1, found.Value);
    }

    [Fact]
    public async Task Should_update_value_with_expected_value()
    {
        var sut = await CreateSutAsync();

        var sourceId = DomainId.NewGuid();
        var sourceValue0 = new TestValue { Value = $"{sourceId}_0" };
        var sourceValue1 = new TestValue { Value = $"{sourceId}_1" };

        await sut.WriteAsync(new SnapshotWriteJob<TestValue>(sourceId, sourceValue0, 0));
        await sut.WriteAsync(new SnapshotWriteJob<TestValue>(sourceId, sourceValue1, 1, 0));

        var found = await sut.ReadAsync(sourceId);

        Assert.Equal(sourceValue1, found.Value);
    }

    [Fact]
    public async Task Should_not_throw_exception_if_inserted_with_wrong_expected_version()
    {
        var sut = await CreateSutAsync();

        var sourceId = DomainId.NewGuid();
        var sourceValue = new TestValue { Value = $"{sourceId}" };

        await sut.WriteAsync(new SnapshotWriteJob<TestValue>(sourceId, sourceValue, 2, 1));
    }

    [Fact]
    public async Task Should_throw_exception_if_update_expected_but_wrong_version_found()
    {
        var sut = await CreateSutAsync();

        var sourceId = DomainId.NewGuid();
        var sourceValue = new TestValue { Value = $"{sourceId}" };

        await sut.WriteAsync(new SnapshotWriteJob<TestValue>(sourceId, sourceValue, 42));

        var ex = await Assert.ThrowsAsync<InconsistentStateException>(() => sut.WriteAsync(new SnapshotWriteJob<TestValue>(sourceId, sourceValue, 2, 1)));

        Assert.Equal(42, ex.VersionCurrent);
    }

    [Fact]
    public async Task Should_remove_entity()
    {
        var sut = await CreateSutAsync();

        var sourceId = DomainId.NewGuid();
        var sourceValue = new TestValue { Value = $"{sourceId}" };

        await sut.WriteAsync(new SnapshotWriteJob<TestValue>(sourceId, sourceValue, 42));
        await sut.RemoveAsync(sourceId);

        var found = await sut.ReadAsync(sourceId);

        Assert.Null(found.Value);
    }

    [Fact]
    public async Task Should_remove_entities()
    {
        var sut = await CreateSutAsync();

        var sourceId = DomainId.NewGuid();
        var sourceValue = new TestValue { Value = $"{sourceId}" };

        await sut.WriteAsync(new SnapshotWriteJob<TestValue>(sourceId, sourceValue, 42));
        await sut.ClearAsync();

        var found = await sut.ReadAsync(sourceId);

        Assert.Null(found.Value);
    }

    [Fact]
    public async Task Should_query_all_entities()
    {
        var sut = await CreateSutAsync();

        var sourceId1 = DomainId.NewGuid();
        var sourceValue1 = new TestValue { Value = $"{sourceId1}" };

        var sourceId2 = DomainId.NewGuid();
        var sourceValue2 = new TestValue { Value = $"{sourceId2}" };

        await sut.WriteAsync(new SnapshotWriteJob<TestValue>(sourceId1, sourceValue1, 41));
        await sut.WriteAsync(new SnapshotWriteJob<TestValue>(sourceId2, sourceValue2, 42));

        var found = await sut.ReadAllAsync().ToListAsync();

        Assert.Contains(new SnapshotResult<TestValue>(sourceId1, sourceValue1, 41), found);
        Assert.Contains(new SnapshotResult<TestValue>(sourceId2, sourceValue2, 42), found);
    }

    [Fact]
    public async Task Should_write_many_and_query_all_entities()
    {
        var sut = await CreateSutAsync();

        var sourceId1 = DomainId.NewGuid();
        var sourceValue1 = new TestValue { Value = $"{sourceId1}" };

        var sourceId2 = DomainId.NewGuid();
        var sourceValue2 = new TestValue { Value = $"{sourceId2}" };

        await sut.WriteManyAsync([
            new SnapshotWriteJob<TestValue>(sourceId1, sourceValue1, 41),
            new SnapshotWriteJob<TestValue>(sourceId2, sourceValue2, 42),
        ]);

        var found = await sut.ReadAllAsync().ToListAsync();

        Assert.Contains(new SnapshotResult<TestValue>(sourceId1, sourceValue1, 41), found);
        Assert.Contains(new SnapshotResult<TestValue>(sourceId2, sourceValue2, 42), found);
    }
}
