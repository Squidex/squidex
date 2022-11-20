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
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.TestHelpers;
using Squidex.Infrastructure.Validation;
using Squidex.Messaging;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes;

public class SchemasIndexTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly TestState<NameReservationState.State> state;
    private readonly ISchemaRepository schemaRepository = A.Fake<ISchemaRepository>();
    private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
    private readonly SchemasIndex sut;

    public SchemasIndexTests()
    {
        state = new TestState<NameReservationState.State>($"{appId.Id}_Schemas");

        ct = cts.Token;

        var replicatedCache =
            new ReplicatedCache(new MemoryCache(Options.Create(new MemoryCacheOptions())), A.Fake<IMessageBus>(),
                Options.Create(new ReplicatedCacheOptions { Enable = true }));

        sut = new SchemasIndex(schemaRepository, replicatedCache, state.PersistenceFactory);
    }

    [Fact]
    public async Task Should_resolve_schema_by_name()
    {
        var expected = SetupSchema();

        A.CallTo(() => schemaRepository.FindAsync(appId.Id, schemaId.Name, ct))
            .Returns(expected);

        var actual1 = await sut.GetSchemaAsync(appId.Id, schemaId.Name, false, ct);
        var actual2 = await sut.GetSchemaAsync(appId.Id, schemaId.Name, false, ct);

        Assert.Same(expected, actual1);
        Assert.Same(expected, actual2);

        A.CallTo(() => schemaRepository.FindAsync(appId.Id, schemaId.Name, ct))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_resolve_schema_by_name_and_id_if_cached_before()
    {
        var expected = SetupSchema();

        A.CallTo(() => schemaRepository.FindAsync(appId.Id, schemaId.Name, ct))
            .Returns(expected);

        var actual1 = await sut.GetSchemaAsync(appId.Id, schemaId.Name, true, ct);
        var actual2 = await sut.GetSchemaAsync(appId.Id, schemaId.Name, true, ct);
        var actual3 = await sut.GetSchemaAsync(appId.Id, schemaId.Id, true, ct);

        Assert.Same(expected, actual1);
        Assert.Same(expected, actual2);
        Assert.Same(expected, actual3);

        A.CallTo(() => schemaRepository.FindAsync(appId.Id, schemaId.Name, ct))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_resolve_schema_by_id()
    {
        var expected = SetupSchema();

        A.CallTo(() => schemaRepository.FindAsync(appId.Id, schemaId.Id, ct))
            .Returns(expected);

        var actual1 = await sut.GetSchemaAsync(appId.Id, schemaId.Id, false, ct);
        var actual2 = await sut.GetSchemaAsync(appId.Id, schemaId.Id, false, ct);

        Assert.Same(expected, actual1);
        Assert.Same(expected, actual2);

        A.CallTo(() => schemaRepository.FindAsync(appId.Id, schemaId.Id, ct))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_resolve_schema_by_id_and_name_if_cached_before()
    {
        var expected = SetupSchema();

        A.CallTo(() => schemaRepository.FindAsync(appId.Id, schemaId.Id, ct))
            .Returns(expected);

        var actual1 = await sut.GetSchemaAsync(appId.Id, schemaId.Id, true, ct);
        var actual2 = await sut.GetSchemaAsync(appId.Id, schemaId.Id, true, ct);
        var actual3 = await sut.GetSchemaAsync(appId.Id, schemaId.Name, true, ct);

        Assert.Same(expected, actual1);
        Assert.Same(expected, actual2);
        Assert.Same(expected, actual3);

        A.CallTo(() => schemaRepository.FindAsync(appId.Id, schemaId.Id, ct))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_resolve_schemas()
    {
        var expected = SetupSchema();

        A.CallTo(() => schemaRepository.QueryAllAsync(appId.Id, ct))
            .Returns(new List<ISchemaEntity> { expected });

        var actual = await sut.GetSchemasAsync(appId.Id, ct);

        Assert.Same(actual[0], expected);
    }

    [Fact]
    public async Task Should_return_empty_schemas_if_schema_not_created()
    {
        var expected = SetupSchema(EtagVersion.Empty);

        A.CallTo(() => schemaRepository.QueryAllAsync(appId.Id, ct))
            .Returns(new List<ISchemaEntity> { expected });

        var actual = await sut.GetSchemasAsync(appId.Id, ct);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_return_empty_schemas_if_schema_deleted()
    {
        var expected = SetupSchema(0, true);

        A.CallTo(() => schemaRepository.QueryAllAsync(appId.Id, ct))
            .Returns(new List<ISchemaEntity> { expected });

        var actual = await sut.GetSchemasAsync(appId.Id, ct);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_take_and_remove_reservation_if_created()
    {
        A.CallTo(() => schemaRepository.FindAsync(appId.Id, schemaId.Name, ct))
            .Returns(Task.FromResult<ISchemaEntity?>(null));

        var command = Create(schemaId.Name);

        var context =
            new CommandContext(command, commandBus)
                .Complete();

        NameReservation? madeReservation = null;

        await sut.HandleAsync(context, (c, ct) =>
        {
            madeReservation = state.Snapshot.Reservations.FirstOrDefault();

            return Task.CompletedTask;
        }, ct);

        Assert.Empty(state.Snapshot.Reservations);

        Assert.Equal(schemaId.Id, madeReservation?.Id);
        Assert.Equal(schemaId.Name, madeReservation?.Name);
    }

    [Fact]
    public async Task Should_clear_reservation_if_schema_creation_failed()
    {
        A.CallTo(() => schemaRepository.FindAsync(appId.Id, schemaId.Name, ct))
            .Returns(Task.FromResult<ISchemaEntity?>(null));

        var command = Create(schemaId.Name);

        var context =
            new CommandContext(command, commandBus)
                .Complete();

        NameReservation? madeReservation = null;

        await Assert.ThrowsAnyAsync<Exception>(() => sut.HandleAsync(context, (c, ct) =>
        {
            madeReservation = state.Snapshot.Reservations.FirstOrDefault();

            throw new InvalidOperationException();
        }, ct));

        Assert.Empty(state.Snapshot.Reservations);

        Assert.Equal(schemaId.Id, madeReservation?.Id);
        Assert.Equal(schemaId.Name, madeReservation?.Name);
    }

    [Fact]
    public async Task Should_not_create_schema_if_name_is_reserved()
    {
        state.Snapshot.Reservations.Add(new NameReservation(RandomHash.Simple(), schemaId.Name, DomainId.NewGuid()));

        A.CallTo(() => schemaRepository.FindAsync(appId.Id, schemaId.Name, ct))
            .Returns(Task.FromResult<ISchemaEntity?>(null));

        var command = Create(schemaId.Name);

        var context =
            new CommandContext(command, commandBus)
                .Complete();

        await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context, ct));
    }

    [Fact]
    public async Task Should_not_create_schema_if_name_is_taken()
    {
        A.CallTo(() => schemaRepository.FindAsync(appId.Id, schemaId.Name, ct))
            .Returns(SetupSchema());

        var command = Create(schemaId.Name);

        var context =
            new CommandContext(command, commandBus)
                .Complete();

        await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context, ct));

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<NameReservationState.State>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_make_an_update_for_other_command()
    {
        var schema = SetupSchema();

        var command = new UpdateSchema { SchemaId = schemaId, AppId = appId };

        var context =
            new CommandContext(command, commandBus)
                .Complete(schema);

        await sut.HandleAsync(context, ct);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<NameReservationState.State>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    private CreateSchema Create(string name)
    {
        return new CreateSchema { SchemaId = schemaId.Id, Name = name, AppId = appId };
    }

    private ISchemaEntity SetupSchema(long version = 0, bool isDeleted = false)
    {
        var schema = A.Fake<ISchemaEntity>();

        A.CallTo(() => schema.SchemaDef).Returns(new Schema(schemaId.Name));
        A.CallTo(() => schema.Id).Returns(schemaId.Id);
        A.CallTo(() => schema.AppId).Returns(appId);
        A.CallTo(() => schema.Version).Returns(version);
        A.CallTo(() => schema.IsDeleted).Returns(isDeleted);

        return schema;
    }
}
