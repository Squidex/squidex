// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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
    public class SchemasIndexIntegrationTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly SchemasByAppIndexGrain index;
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly SchemasIndex sut;

        public SchemasIndexIntegrationTests()
        {
            index = new SchemasByAppIndexGrain(A.Fake<IGrainState<SchemasByAppIndexGrain.GrainState>>());
            index.ActivateAsync(appId.Id).Wait();

            A.CallTo(() => grainFactory.GetGrain<ISchemasByAppIndexGrain>(appId.Id, null))
                .Returns(index);

            sut = new SchemasIndex(grainFactory);
        }

        [Fact]
        public async Task Should_also_add_schema_to_index_on_create_when_index_is_inconsistent()
        {
            var schemaIdOld = Guid.NewGuid();
            var schemaIdNew = Guid.NewGuid();

            var schemaName = "my-schema";

            SetupSchema(schemaIdOld, schemaName, -1, false);

            await index.AddSchemaAsync(schemaIdOld, schemaName);

            var context =
                new CommandContext(Create(schemaIdNew, schemaName), commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            var foundIndex = await index.GetSchemaIdAsync(schemaName);

            Assert.Equal(schemaIdNew, foundIndex);
        }

        private CreateSchema Create(Guid schemaId, string name)
        {
            return new CreateSchema { SchemaId = schemaId, Name = name, AppId = appId };
        }

        private ISchemaEntity SetupSchema(Guid schemaId, string name, long version, bool deleted)
        {
            var schemaEntity = A.Fake<ISchemaEntity>();

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
