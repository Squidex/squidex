// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Squidex.Caching;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.Schemas.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes
{
    public class SchemasIndexTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly ISchemasByAppIndexGrain index = A.Fake<ISchemasByAppIndexGrain>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
        private readonly SchemasIndex sut;

        public SchemasIndexTests()
        {
            A.CallTo(() => grainFactory.GetGrain<ISchemasByAppIndexGrain>(appId.Id.ToString(), null))
                .Returns(index);

            var cache =
                new ReplicatedCache(new MemoryCache(Options.Create(new MemoryCacheOptions())), new SimplePubSub(A.Fake<ILogger<SimplePubSub>>()),
                    Options.Create(new ReplicatedCacheOptions { Enable = true }));

            sut = new SchemasIndex(grainFactory, cache);
        }

        [Fact]
        public async Task Should_resolve_schema_by_name()
        {
            var (expected, _) = SetupSchema();

            A.CallTo(() => index.GetIdAsync(schemaId.Name))
                .Returns(schemaId.Id);

            var actual1 = await sut.GetSchemaByNameAsync(appId.Id, schemaId.Name, false);
            var actual2 = await sut.GetSchemaByNameAsync(appId.Id, schemaId.Name, false);

            Assert.Same(expected, actual1);
            Assert.Same(expected, actual2);

            A.CallTo(() => grainFactory.GetGrain<ISchemaGrain>(A<string>._, null))
                .MustHaveHappenedTwiceExactly();

            A.CallTo(() => index.GetIdAsync(A<string>._))
                .MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task Should_resolve_schema_by_name_and_id_if_cached_before()
        {
            var (expected, _) = SetupSchema();

            A.CallTo(() => index.GetIdAsync(schemaId.Name))
                .Returns(schemaId.Id);

            var actual1 = await sut.GetSchemaByNameAsync(appId.Id, schemaId.Name, true);
            var actual2 = await sut.GetSchemaByNameAsync(appId.Id, schemaId.Name, true);
            var actual3 = await sut.GetSchemaAsync(appId.Id, schemaId.Id, true);

            Assert.Same(expected, actual1);
            Assert.Same(expected, actual2);
            Assert.Same(expected, actual3);

            A.CallTo(() => grainFactory.GetGrain<ISchemaGrain>(A<string>._, null))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => index.GetIdAsync(A<string>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_resolve_schema_by_id()
        {
            var (expected, _) = SetupSchema();

            var actual1 = await sut.GetSchemaAsync(appId.Id, schemaId.Id, false);
            var actual2 = await sut.GetSchemaAsync(appId.Id, schemaId.Id, false);

            Assert.Same(expected, actual1);
            Assert.Same(expected, actual2);

            A.CallTo(() => grainFactory.GetGrain<ISchemaGrain>(A<string>._, null))
                .MustHaveHappenedTwiceExactly();

            A.CallTo(() => index.GetIdAsync(A<string>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_resolve_schema_by_id_and_name_if_cached_before()
        {
            var (expected, _) = SetupSchema();

            var actual1 = await sut.GetSchemaAsync(appId.Id, schemaId.Id, true);
            var actual2 = await sut.GetSchemaAsync(appId.Id, schemaId.Id, true);
            var actual3 = await sut.GetSchemaByNameAsync(appId.Id, schemaId.Name, true);

            Assert.Same(expected, actual1);
            Assert.Same(expected, actual2);
            Assert.Same(expected, actual3);

            A.CallTo(() => grainFactory.GetGrain<ISchemaGrain>(A<string>._, null))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => index.GetIdAsync(A<string>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_resolve_schemas_by_id()
        {
            var (schema, _) = SetupSchema();

            A.CallTo(() => index.GetIdsAsync())
                .Returns(new List<DomainId> { schema.Id });

            var actual = await sut.GetSchemasAsync(appId.Id);

            Assert.Same(actual[0], schema);
        }

        [Fact]
        public async Task Should_return_empty_schemas_if_schema_not_created()
        {
            var (schema, _) = SetupSchema(EtagVersion.Empty);

            A.CallTo(() => index.GetIdsAsync())
                .Returns(new List<DomainId> { schema.Id });

            var actual = await sut.GetSchemasAsync(appId.Id);

            Assert.Empty(actual);
        }

        [Fact]
        public async Task Should_return_empty_schemas_if_schema_deleted()
        {
            var (schema, _) = SetupSchema(0, true);

            A.CallTo(() => index.GetIdsAsync())
                .Returns(new List<DomainId> { schema.Id });

            var actual = await sut.GetSchemasAsync(appId.Id);

            Assert.Empty(actual);
        }

        [Fact]
        public async Task Should_add_schema_to_index_if_creating()
        {
            var token = RandomHash.Simple();

            A.CallTo(() => index.ReserveAsync(schemaId.Id, schemaId.Name))
                .Returns(token);

            var command = Create(schemaId.Name);

            var context =
                new CommandContext(command, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => index.AddAsync(token))
                .MustHaveHappened();

            A.CallTo(() => index.RemoveReservationAsync(A<string>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_clear_reservation_if_schema_creation_failed()
        {
            var token = RandomHash.Simple();

            A.CallTo(() => index.ReserveAsync(schemaId.Id, schemaId.Name))
                .Returns(token);

            var command = Create(schemaId.Name);

            var context =
                new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            A.CallTo(() => index.AddAsync(token))
                .MustNotHaveHappened();

            A.CallTo(() => index.RemoveReservationAsync(token))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_add_to_indexes_if_name_is_taken()
        {
            A.CallTo(() => index.ReserveAsync(schemaId.Id, schemaId.Name))
                .Returns(Task.FromResult<string?>(null));

            var command = Create(schemaId.Name);

            var context =
                new CommandContext(command, commandBus)
                    .Complete();

            await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));

            A.CallTo(() => index.AddAsync(A<string>._))
                .MustNotHaveHappened();

            A.CallTo(() => index.RemoveReservationAsync(A<string>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_add_to_indexes_if_name_is_invalid()
        {
            var command = Create("INVALID");

            var context =
                new CommandContext(command, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => index.ReserveAsync(schemaId.Id, A<string>._))
                .MustNotHaveHappened();

            A.CallTo(() => index.RemoveReservationAsync(A<string>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_update_index_if_schema_is_updated()
        {
            var (_, schemaGrain) = SetupSchema();

            var command = new UpdateSchema { SchemaId = schemaId, AppId = appId };

            var context =
                new CommandContext(command, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => schemaGrain.GetStateAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_update_index_with_result_if_schema_is_updated()
        {
            var (schema, schemaGrain) = SetupSchema();

            var command = new UpdateSchema { SchemaId = schemaId, AppId = appId };

            var context =
                new CommandContext(command, commandBus)
                    .Complete(schema);

            await sut.HandleAsync(context);

            A.CallTo(() => schemaGrain.GetStateAsync())
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_remove_schema_from_index_if_deleted_and_exists()
        {
            var (schema, _) = SetupSchema(isDeleted: true);

            var command = new DeleteSchema { SchemaId = schemaId, AppId = appId };

            var context =
                new CommandContext(command, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => index.RemoveAsync(schema.Id))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_forward_call_if_rebuilding()
        {
            var schemas = new Dictionary<string, DomainId>();

            await sut.RebuildAsync(appId.Id, schemas);

            A.CallTo(() => index.RebuildAsync(schemas))
                .MustHaveHappened();
        }

        private CreateSchema Create(string name)
        {
            return new CreateSchema { SchemaId = schemaId.Id, Name = name, AppId = appId };
        }

        private (ISchemaEntity, ISchemaGrain) SetupSchema(long version = 0, bool isDeleted = false)
        {
            var schemaEntity = A.Fake<ISchemaEntity>();

            A.CallTo(() => schemaEntity.SchemaDef)
                .Returns(new Schema(schemaId.Name));
            A.CallTo(() => schemaEntity.Id)
                .Returns(schemaId.Id);
            A.CallTo(() => schemaEntity.AppId)
                .Returns(appId);
            A.CallTo(() => schemaEntity.Version)
                .Returns(version);
            A.CallTo(() => schemaEntity.IsDeleted)
                .Returns(isDeleted);

            var schemaGrain = A.Fake<ISchemaGrain>();

            A.CallTo(() => schemaGrain.GetStateAsync())
                .Returns(J.Of(schemaEntity));

            var key = DomainId.Combine(appId, schemaId.Id).ToString();

            A.CallTo(() => grainFactory.GetGrain<ISchemaGrain>(key, null))
                .Returns(schemaGrain);

            return (schemaEntity, schemaGrain);
        }
    }
}
