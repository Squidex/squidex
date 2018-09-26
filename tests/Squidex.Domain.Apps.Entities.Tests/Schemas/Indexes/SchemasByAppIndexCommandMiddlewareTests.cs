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
        private readonly Guid appId = Guid.NewGuid();
        private readonly SchemasByAppIndexCommandMiddleware sut;

        public SchemasByAppIndexCommandMiddlewareTests()
        {
            A.CallTo(() => grainFactory.GetGrain<ISchemasByAppIndex>(appId, null))
                .Returns(index);

            sut = new SchemasByAppIndexCommandMiddleware(grainFactory);
        }

        [Fact]
        public async Task Should_add_schema_to_index_on_create()
        {
            var context =
                new CommandContext(new CreateSchema { SchemaId = appId, Name = "my-schema", AppId = BuildAppId() }, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => index.AddSchemaAsync(appId, "my-schema"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_remove_schema_from_index_on_delete()
        {
            var schemaGrain = A.Fake<ISchemaGrain>();
            var schemaState = A.Fake<ISchemaEntity>();

            A.CallTo(() => grainFactory.GetGrain<ISchemaGrain>(appId, null))
                .Returns(schemaGrain);

            A.CallTo(() => schemaGrain.GetStateAsync())
                .Returns(J.AsTask(schemaState));

            A.CallTo(() => schemaState.AppId)
                .Returns(BuildAppId());

            var context =
                new CommandContext(new DeleteSchema { SchemaId = appId }, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => index.RemoveSchemaAsync(appId))
                .MustHaveHappened();
        }

        private NamedId<Guid> BuildAppId()
        {
            return NamedId.Of(appId, "my-app");
        }
    }
}
