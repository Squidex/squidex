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
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes
{
    public class SchemasByAppIndexCommandMiddlewareTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly ISchemasByAppIndex index = A.Fake<ISchemasByAppIndex>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly SchemasByAppIndexCommandMiddleware sut;

        public SchemasByAppIndexCommandMiddlewareTests()
        {
            A.CallTo(() => grainFactory.GetGrain<ISchemasByAppIndex>(appId.Id, null))
                .Returns(index);

            sut = new SchemasByAppIndexCommandMiddleware(grainFactory);
        }

        [Fact]
        public async Task Should_add_schema_to_index_on_create()
        {
            var context =
                new CommandContext(new CreateSchema { SchemaId = schemaId.Id, Name = schemaId.Name, AppId = appId }, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => index.AddSchemaAsync(schemaId.Id, schemaId.Name))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_remove_schema_from_index_on_delete()
        {
            var schemaGrain = A.Fake<ISchemaGrain>();
            var schemaState = Mocks.Schema(appId, schemaId);

            A.CallTo(() => grainFactory.GetGrain<ISchemaGrain>(schemaId.Id, null))
                .Returns(schemaGrain);

            A.CallTo(() => schemaGrain.GetStateAsync())
                .Returns(J.AsTask(schemaState));

            var context =
                new CommandContext(new DeleteSchema { SchemaId = schemaId.Id }, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => index.RemoveSchemaAsync(schemaId.Id))
                .MustHaveHappened();
        }
    }
}
