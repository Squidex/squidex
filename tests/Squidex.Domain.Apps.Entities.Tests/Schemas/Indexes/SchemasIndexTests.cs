// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes
{
    public class SchemasIndexTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly ISchemasByAppIndexGrain index = A.Fake<ISchemasByAppIndexGrain>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly SchemasIndex sut;

        public SchemasIndexTests()
        {
            A.CallTo(() => grainFactory.GetGrain<ISchemasByAppIndexGrain>(appId.Id, null))
                .Returns(index);

            sut = new SchemasIndex(grainFactory);
        }

        [Fact]
        public async Task Should_resolve_schema_by_id()
        {
            var schema = SetupSchema("my-schema", 0, false);

            var actual = await sut.GetSchemaAsync(appId.Id, schema.Id);

            Assert.Same(actual, schema);
        }

        [Fact]
        public async Task Should_resolve_schema_by_name()
        {
            var schema = SetupSchema("my-schema", 0, false);

            A.CallTo(() => index.GetSchemaIdAsync(schema.SchemaDef.Name))
                .Returns(schema.Id);

            var actual = await sut.GetSchemaAsync(appId.Id, schema.SchemaDef.Name);

            Assert.Same(actual, schema);
        }

        [Fact]
        public async Task Should_resolve_schemas_by_id()
        {
            var schema = SetupSchema("my-schema", 0, false);

            A.CallTo(() => index.GetSchemaIdsAsync())
                .Returns(new List<Guid> { schema.Id });

            var actual = await sut.GetSchemasAsync(appId.Id);

            Assert.Same(actual[0], schema);
        }

        [Fact]
        public async Task Should_return_empty_schema_if_schema_not_created()
        {
            var schema = SetupSchema("my-schema", -1, false);

            A.CallTo(() => index.GetSchemaIdsAsync())
                .Returns(new List<Guid> { schema.Id });

            var actual = await sut.GetSchemasAsync(appId.Id);

            Assert.Empty(actual);
        }

        [Fact]
        public async Task Should_return_empty_schema_if_schema_deleted()
        {
            var schema = SetupSchema("my-schema", 0, true);

            A.CallTo(() => index.GetSchemaIdAsync(schema.SchemaDef.Name))
                .Returns(schema.Id);

            var actual = await sut.GetSchemasAsync(appId.Id);

            Assert.Empty(actual);
        }

        [Fact]
        public async Task Should_also_return_schema_if_deleted_allowed()
        {
            var schema = SetupSchema("my-schema", -1, true);

            A.CallTo(() => index.GetSchemaIdAsync(schema.SchemaDef.Name))
                .Returns(schema.Id);

            var actual = await sut.GetSchemasAsync(appId.Id, true);

            Assert.Empty(actual);
        }

        [Fact]
        public async Task Should_clean_index_if_not_consistent()
        {
            var schema = SetupSchema("my-schema", -1, false);

            A.CallTo(() => index.GetSchemaIdsAsync())
                .Returns(new List<Guid> { schema.Id });

            await sut.GetSchemasAsync(appId.Id);

            A.CallTo(() => index.RemoveSchemaAsync(schema.Id))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_add_schema_to_index_on_create()
        {
            var schemaId = Guid.NewGuid();
            var schemaName = "my-schema";

            var context =
                new CommandContext(new CreateSchema { SchemaId = schemaId, Name = schemaName, AppId = appId }, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => index.AddSchemaAsync(schemaId, schemaName))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_remove_schema_from_index_on_delete()
        {
            var schema = SetupSchema("my-schema", 0, false);

            var context =
                new CommandContext(new DeleteSchema { SchemaId = schema.Id }, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => index.RemoveSchemaAsync(schema.Id))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_forward_call_when_rebuilding()
        {
            var schemas = new Dictionary<string, Guid>();

            await sut.RebuildAsync(appId.Id, schemas);

            A.CallTo(() => index.RebuildAsync(schemas))
                .MustHaveHappened();
        }

        private ISchemaEntity SetupSchema(string name, long version, bool deleted)
        {
            var schemaEntity = A.Fake<ISchemaEntity>();

            var schemaId = Guid.NewGuid();

            A.CallTo(() => schemaEntity.SchemaDef)
                .Returns(new Schema(name));
            A.CallTo(() => schemaEntity.Id)
                .Returns(schemaId);
            A.CallTo(() => schemaEntity.AppId)
                .Returns(appId);
            A.CallTo(() => schemaEntity.Version)
                .Returns(version);
            A.CallTo(() => schemaEntity.IsDeleted)
                .Returns(deleted);

            var schemaGrain = A.Fake<ISchemaGrain>();

            A.CallTo(() => schemaGrain.GetStateAsync())
                .Returns(J.Of(schemaEntity));

            A.CallTo(() => grainFactory.GetGrain<ISchemaGrain>(schemaId, null))
                .Returns(schemaGrain);

            return schemaEntity;
        }
    }
}
