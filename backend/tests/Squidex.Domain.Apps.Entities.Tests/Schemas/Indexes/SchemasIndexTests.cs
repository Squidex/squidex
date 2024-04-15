// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Caching;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.Schemas.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.TestHelpers;
using Squidex.Infrastructure.Validation;
using Squidex.Messaging;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes;

public class SchemasIndexTests : GivenContext
{
    private readonly TestState<NameReservationState.State> state;
    private readonly ISchemaRepository schemaRepository = A.Fake<ISchemaRepository>();
    private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
    private readonly SchemaCacheOptions options = new SchemaCacheOptions();
    private readonly SchemasIndex sut;

    public SchemasIndexTests()
    {
        options.CacheDuration = TimeSpan.FromMinutes(5);

        state = new TestState<NameReservationState.State>($"{AppId.Id}_Schemas");

        var replicatedCache =
            new ReplicatedCache(new MemoryCache(Options.Create(new MemoryCacheOptions())), A.Fake<IMessageBus>());

        sut = new SchemasIndex(schemaRepository, replicatedCache, state.PersistenceFactory, Options.Create(options));
    }

    [Fact]
    public async Task Should_resolve_schema_by_name()
    {
        A.CallTo(() => schemaRepository.FindAsync(AppId.Id, SchemaId.Name, CancellationToken))
            .Returns(Schema);

        var actual1 = await sut.GetSchemaAsync(AppId.Id, SchemaId.Name, false, CancellationToken);
        var actual2 = await sut.GetSchemaAsync(AppId.Id, SchemaId.Name, false, CancellationToken);

        Assert.Same(Schema, actual1);
        Assert.Same(Schema, actual2);

        A.CallTo(() => schemaRepository.FindAsync(AppId.Id, SchemaId.Name, CancellationToken))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_resolve_schema_by_name_and_id_if_cached_before()
    {
        A.CallTo(() => schemaRepository.FindAsync(AppId.Id, SchemaId.Name, CancellationToken))
            .ReturnsLazily(() => Schema with { Version = 3 });

        var actual1 = await sut.GetSchemaAsync(AppId.Id, SchemaId.Name, true, CancellationToken);
        var actual2 = await sut.GetSchemaAsync(AppId.Id, SchemaId.Name, true, CancellationToken);
        var actual3 = await sut.GetSchemaAsync(AppId.Id, SchemaId.Id, true, CancellationToken);

        Assert.Same(actual1, actual2);
        Assert.Same(actual1, actual3);

        A.CallTo(() => schemaRepository.FindAsync(AppId.Id, SchemaId.Name, CancellationToken))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_not_resolve_schema_by_name_and_id_if_cached_before_but_disabled()
    {
        options.CacheDuration = default;

        A.CallTo(() => schemaRepository.FindAsync(AppId.Id, SchemaId.Name, CancellationToken))
            .ReturnsLazily(() => Schema with { Version = 3 });

        var actual1 = await sut.GetSchemaAsync(AppId.Id, SchemaId.Name, true, CancellationToken);
        var actual2 = await sut.GetSchemaAsync(AppId.Id, SchemaId.Name, true, CancellationToken);
        var actual3 = await sut.GetSchemaAsync(AppId.Id, SchemaId.Id, true, CancellationToken);

        Assert.NotSame(actual1, actual2);
        Assert.NotSame(actual1, actual3);

        A.CallTo(() => schemaRepository.FindAsync(AppId.Id, SchemaId.Name, CancellationToken))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_resolve_schema_by_id()
    {
        A.CallTo(() => schemaRepository.FindAsync(AppId.Id, SchemaId.Id, CancellationToken))
            .Returns(Schema);

        var actual1 = await sut.GetSchemaAsync(AppId.Id, SchemaId.Id, false, CancellationToken);
        var actual2 = await sut.GetSchemaAsync(AppId.Id, SchemaId.Id, false, CancellationToken);

        Assert.Same(Schema, actual1);
        Assert.Same(Schema, actual2);

        A.CallTo(() => schemaRepository.FindAsync(AppId.Id, SchemaId.Id, CancellationToken))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_resolve_schema_by_id_and_name_if_cached_before()
    {
        A.CallTo(() => schemaRepository.FindAsync(AppId.Id, SchemaId.Id, CancellationToken))
            .ReturnsLazily(() => Schema with { Version = 3 });

        var actual1 = await sut.GetSchemaAsync(AppId.Id, SchemaId.Id, true, CancellationToken);
        var actual2 = await sut.GetSchemaAsync(AppId.Id, SchemaId.Id, true, CancellationToken);
        var actual3 = await sut.GetSchemaAsync(AppId.Id, SchemaId.Name, true, CancellationToken);

        Assert.Same(actual1, actual2);
        Assert.Same(actual1, actual3);

        A.CallTo(() => schemaRepository.FindAsync(AppId.Id, SchemaId.Id, CancellationToken))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_not_resolve_schema_by_id_and_name_if_cached_before_but_disabled()
    {
        options.CacheDuration = default;

        A.CallTo(() => schemaRepository.FindAsync(AppId.Id, SchemaId.Id, CancellationToken))
            .ReturnsLazily(() => Schema with { Version = 3 });

        var actual1 = await sut.GetSchemaAsync(AppId.Id, SchemaId.Id, true, CancellationToken);
        var actual2 = await sut.GetSchemaAsync(AppId.Id, SchemaId.Id, true, CancellationToken);
        var actual3 = await sut.GetSchemaAsync(AppId.Id, SchemaId.Name, true, CancellationToken);

        Assert.NotSame(actual1, actual2);
        Assert.NotSame(actual1, actual3);

        A.CallTo(() => schemaRepository.FindAsync(AppId.Id, SchemaId.Id, CancellationToken))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_resolve_schemas()
    {
        A.CallTo(() => schemaRepository.QueryAllAsync(AppId.Id, CancellationToken))
            .Returns([Schema]);

        var actual = await sut.GetSchemasAsync(AppId.Id, CancellationToken);

        Assert.Same(actual[0], Schema);
    }

    [Fact]
    public async Task Should_return_empty_schemas_if_schema_not_created()
    {
        Schema = Schema with { Version = EtagVersion.Empty };

        A.CallTo(() => schemaRepository.QueryAllAsync(AppId.Id, CancellationToken))
            .Returns([Schema]);

        var actual = await sut.GetSchemasAsync(AppId.Id, CancellationToken);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_return_empty_schemas_if_schema_deleted()
    {
        Schema = Schema with { IsDeleted = true };

        A.CallTo(() => schemaRepository.QueryAllAsync(AppId.Id, CancellationToken))
            .Returns([Schema]);

        var actual = await sut.GetSchemasAsync(AppId.Id, CancellationToken);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_take_and_remove_reservation_if_created()
    {
        A.CallTo(() => schemaRepository.FindAsync(AppId.Id, SchemaId.Name, CancellationToken))
            .Returns(Task.FromResult<Schema?>(null));

        var command = Create(SchemaId.Name);

        var context =
            new CommandContext(command, commandBus)
                .Complete();

        NameReservation? madeReservation = null;

        await sut.HandleAsync(context, (c, ct) =>
        {
            madeReservation = state.Snapshot.Reservations.FirstOrDefault();

            return Task.CompletedTask;
        }, CancellationToken);

        Assert.Empty(state.Snapshot.Reservations);

        Assert.Equal(SchemaId.Id, madeReservation?.Id);
        Assert.Equal(SchemaId.Name, madeReservation?.Name);
    }

    [Fact]
    public async Task Should_clear_reservation_if_schema_creation_failed()
    {
        A.CallTo(() => schemaRepository.FindAsync(AppId.Id, SchemaId.Name, CancellationToken))
            .Returns(Task.FromResult<Schema?>(null));

        var command = Create(SchemaId.Name);

        var context =
            new CommandContext(command, commandBus)
                .Complete();

        NameReservation? madeReservation = null;

        await Assert.ThrowsAnyAsync<Exception>(() => sut.HandleAsync(context, (c, ct) =>
        {
            madeReservation = state.Snapshot.Reservations.FirstOrDefault();

            throw new InvalidOperationException();
        }, CancellationToken));

        Assert.Empty(state.Snapshot.Reservations);

        Assert.Equal(SchemaId.Id, madeReservation?.Id);
        Assert.Equal(SchemaId.Name, madeReservation?.Name);
    }

    [Fact]
    public async Task Should_not_create_schema_if_name_is_reserved()
    {
        state.Snapshot.Reservations.Add(new NameReservation(RandomHash.Simple(), SchemaId.Name, DomainId.NewGuid()));

        A.CallTo(() => schemaRepository.FindAsync(AppId.Id, SchemaId.Name, CancellationToken))
            .Returns(Task.FromResult<Schema?>(null));

        var command = Create(SchemaId.Name);

        var context =
            new CommandContext(command, commandBus)
                .Complete();

        await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context, CancellationToken));
    }

    [Fact]
    public async Task Should_not_create_schema_if_name_is_taken()
    {
        A.CallTo(() => schemaRepository.FindAsync(AppId.Id, SchemaId.Name, CancellationToken))
            .Returns(Schema);

        var command = Create(SchemaId.Name);

        var context =
            new CommandContext(command, commandBus)
                .Complete();

        await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context, CancellationToken));

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<NameReservationState.State>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_make_an_update_for_other_command()
    {
        var command = new UpdateSchema { SchemaId = SchemaId, AppId = AppId };

        var context =
            new CommandContext(command, commandBus)
                .Complete(Schema);

        await sut.HandleAsync(context, CancellationToken);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<NameReservationState.State>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    private CreateSchema Create(string name)
    {
        return new CreateSchema { SchemaId = SchemaId.Id, Name = name, AppId = AppId };
    }
}
