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
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes
{
    public class SchemasIndexTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly ISchemasByAppIndexGrain index = A.Fake<ISchemasByAppIndexGrain>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
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
            var schema = SetupSchema(0, false);

            var actual = await sut.GetSchemaAsync(appId.Id, schema.Id);

            Assert.Same(actual, schema);
        }

        [Fact]
        public async Task Should_resolve_schema_by_name()
        {
            var schema = SetupSchema(0, false);

            A.CallTo(() => index.GetIdAsync(schema.SchemaDef.Name))
                .Returns(schema.Id);

            var actual = await sut.GetSchemaAsync(appId.Id, schema.SchemaDef.Name);

            Assert.Same(actual, schema);
        }

        [Fact]
        public async Task Should_resolve_schemas_by_id()
        {
            var schema = SetupSchema(0, false);

            A.CallTo(() => index.GetIdsAsync())
                .Returns(new List<Guid> { schema.Id });

            var actual = await sut.GetSchemasAsync(appId.Id);

            Assert.Same(actual[0], schema);
        }

        [Fact]
        public async Task Should_return_empty_schema_if_schema_not_created()
        {
            var schema = SetupSchema(-1, false);

            A.CallTo(() => index.GetIdsAsync())
                .Returns(new List<Guid> { schema.Id });

            var actual = await sut.GetSchemasAsync(appId.Id);

            Assert.Empty(actual);
        }

        [Fact]
        public async Task Should_return_empty_schema_if_schema_deleted()
        {
            var schema = SetupSchema(0, true);

            A.CallTo(() => index.GetIdAsync(schema.SchemaDef.Name))
                .Returns(schema.Id);

            var actual = await sut.GetSchemasAsync(appId.Id);

            Assert.Empty(actual);
        }

        [Fact]
        public async Task Should_also_return_schema_if_deleted_allowed()
        {
            var schema = SetupSchema(-1, true);

            A.CallTo(() => index.GetIdAsync(schema.SchemaDef.Name))
                .Returns(schema.Id);

            var actual = await sut.GetSchemasAsync(appId.Id, true);

            Assert.Empty(actual);
        }

        [Fact]
        public async Task Should_add_schema_to_index_on_create()
        {
            var token = RandomHash.Simple();

            A.CallTo(() => index.ReserveAsync(schemaId.Id, schemaId.Name))
                .Returns(token);

            var context =
                new CommandContext(Create(schemaId.Name), commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => index.AddAsync(token))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_throw_exception_when_app_already_exist()
        {
            A.CallTo(() => index.ReserveAsync(appId.Id, appId.Name))
                .Returns((string)null);

            var context =
                new CommandContext(Create(schemaId.Name), commandBus)
                    .Complete();

            await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
        }

        [Fact]
        public async Task Should_not_add_to_name_index_on_create_if_name_invalid()
        {
            var context =
                new CommandContext(Create("INVALID"), commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => index.ReserveAsync(appId.Id, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_remove_schema_from_index_on_delete()
        {
            var schema = SetupSchema(0, false);

            var context =
                new CommandContext(new DeleteSchema { SchemaId = schema.Id }, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => index.RemoveAsync(schema.Id))
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

        private CreateSchema Create(string name)
        {
            return new CreateSchema { SchemaId = schemaId.Id, Name = name, AppId = appId };
        }

        private ISchemaEntity SetupSchema(long version, bool deleted)
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
                .Returns(deleted);

            var schemaGrain = A.Fake<ISchemaGrain>();

            A.CallTo(() => schemaGrain.GetStateAsync())
                .Returns(J.Of(schemaEntity));

            A.CallTo(() => grainFactory.GetGrain<ISchemaGrain>(schemaId.Id, null))
                .Returns(schemaGrain);

            return schemaEntity;
        }
    }
}
