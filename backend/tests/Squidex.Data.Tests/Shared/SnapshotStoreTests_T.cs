// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentAssertions.Equivalency;
using NodaTime;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Shared;

public abstract class SnapshotStoreTests<TEntity>
{
    protected abstract Task<ISnapshotStore<TEntity>> CreateSutAsync();

    protected abstract TEntity CreateEntity(DomainId id, int version);

    protected virtual bool CheckConsistencyOnWrite => true;

    protected virtual TEntity Cleanup(TEntity expected)
    {
        return expected;
    }

    [Fact]
    public async Task Should_insert_value()
    {
        var sut = await CreateSutAsync();

        var sourceKey = DomainId.NewGuid();
        var sourceValue = CreateEntity(sourceKey, 0);

        await sut.WriteAsync(new SnapshotWriteJob<TEntity>(sourceKey, sourceValue, 0));

        var found = await sut.ReadAsync(sourceKey);

        found.Value.Should().BeEquivalentTo(sourceValue, CompareOptions);
    }

    [Fact]
    public async Task Should_update_value()
    {
        var sut = await CreateSutAsync();

        var sourceKey = DomainId.NewGuid();
        var sourceValue0 = CreateEntity(sourceKey, 0);
        var sourceValue1 = CreateEntity(sourceKey, 1);

        await sut.WriteAsync(new SnapshotWriteJob<TEntity>(sourceKey, sourceValue0, 0));
        await sut.WriteAsync(new SnapshotWriteJob<TEntity>(sourceKey, sourceValue1, 1));

        var found = await sut.ReadAsync(sourceKey);

        found.Value.Should().BeEquivalentTo(sourceValue1, CompareOptions);
    }

    [Fact]
    public async Task Should_update_value_with_expected_value()
    {
        var sut = await CreateSutAsync();

        var sourceKey = DomainId.NewGuid();
        var sourceValue0 = CreateEntity(sourceKey, 0);
        var sourceValue1 = CreateEntity(sourceKey, 1);

        await sut.WriteAsync(new SnapshotWriteJob<TEntity>(sourceKey, sourceValue0, 0));
        await sut.WriteAsync(new SnapshotWriteJob<TEntity>(sourceKey, sourceValue1, 1, 0));

        var found = await sut.ReadAsync(sourceKey);

        found.Value.Should().BeEquivalentTo(sourceValue1, CompareOptions);
    }

    [Fact]
    public async Task Should_not_throw_exception_if_inserted_with_wrong_expected_version()
    {
        var sut = await CreateSutAsync();

        var sourceKey = DomainId.NewGuid();
        var sourceValue = CreateEntity(sourceKey, 0);

        await sut.WriteAsync(new SnapshotWriteJob<TEntity>(sourceKey, sourceValue, 2, 1));
    }

    [Fact]
    public async Task Should_throw_exception_if_update_expected_but_wrong_version_found()
    {
        if (!CheckConsistencyOnWrite)
        {
            return;
        }

        var sut = await CreateSutAsync();

        var sourceKey = DomainId.NewGuid();
        var sourceValue = CreateEntity(sourceKey, 42);

        await sut.WriteAsync(new SnapshotWriteJob<TEntity>(sourceKey, sourceValue, 42));

        var ex = await Assert.ThrowsAsync<InconsistentStateException>(() => sut.WriteAsync(new SnapshotWriteJob<TEntity>(sourceKey, sourceValue, 2, 1)));

        Assert.Equal(42, ex.VersionCurrent);
    }

    [Fact]
    public async Task Should_remove_entity()
    {
        var sut = await CreateSutAsync();

        var sourceKey = DomainId.NewGuid();
        var sourceValue = CreateEntity(sourceKey, 0);

        await sut.WriteAsync(new SnapshotWriteJob<TEntity>(sourceKey, sourceValue, 42));
        await sut.RemoveAsync(sourceKey);

        var found = await sut.ReadAsync(sourceKey);

        Assert.Null(found.Value);
    }

    [Fact]
    public async Task Should_remove_entities()
    {
        var sut = await CreateSutAsync();

        var sourceKey = DomainId.NewGuid();
        var sourceValue = CreateEntity(sourceKey, 0);

        await sut.WriteAsync(new SnapshotWriteJob<TEntity>(sourceKey, sourceValue, 42));
        await sut.ClearAsync();

        var found = await sut.ReadAsync(sourceKey);

        Assert.Null(found.Value);
    }

    [Fact]
    public async Task Should_query_all_entities()
    {
        var sut = await CreateSutAsync();

        var sourceKey1 = DomainId.NewGuid();
        var sourceValue1 = CreateEntity(sourceKey1, 0);

        var sourceKey2 = DomainId.NewGuid();
        var sourceValue2 = CreateEntity(sourceKey2, 0);

        await sut.WriteAsync(new SnapshotWriteJob<TEntity>(sourceKey1, sourceValue1, 41));
        await sut.WriteAsync(new SnapshotWriteJob<TEntity>(sourceKey2, sourceValue2, 42));

        var found = await sut.ReadAllAsync().ToListAsync();

        var found1 = found.Single(x => x.Key == sourceKey1);
        var found2 = found.Single(x => x.Key == sourceKey2);

        found1.Should().NotBeEquivalentTo(new SnapshotResult<TEntity>(sourceKey1, sourceValue1, 41), CompareOptions);
        found2.Should().NotBeEquivalentTo(new SnapshotResult<TEntity>(sourceKey2, sourceValue2, 41), CompareOptions);
    }

    [Fact]
    public async Task Should_write_many_and_query_all_entities()
    {
        var sut = await CreateSutAsync();

        var sourceKey1 = DomainId.NewGuid();
        var sourceValue1 = CreateEntity(sourceKey1, 0);

        var sourceKey2 = DomainId.NewGuid();
        var sourceValue2 = CreateEntity(sourceKey2, 0);

        await sut.WriteManyAsync([
            new SnapshotWriteJob<TEntity>(sourceKey1, sourceValue1, 41),
            new SnapshotWriteJob<TEntity>(sourceKey2, sourceValue2, 42),
        ]);

        var found = await sut.ReadAllAsync().ToListAsync();

        var found1 = found.Single(x => x.Key == sourceKey1);
        var found2 = found.Single(x => x.Key == sourceKey2);

        found1.Should().NotBeEquivalentTo(new SnapshotResult<TEntity>(sourceKey1, sourceValue1, 41), CompareOptions);
        found2.Should().NotBeEquivalentTo(new SnapshotResult<TEntity>(sourceKey2, sourceValue2, 41), CompareOptions);
    }

    protected virtual EquivalencyAssertionOptions<T> CompareOptions<T>(EquivalencyAssertionOptions<T> options)
    {
        return options
            .RespectingDeclaredTypes()
            .Using<Instant>(c => c.Subject.Should().BeCloseTo(c.Expectation, Duration.FromSeconds(1)))
                .WhenTypeIs<Instant>();
    }
}
