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
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes
{
    public class SchemasByAppIndexGrainTests
    {
        private readonly IGrainState<SchemasByAppIndexGrain.GrainState> grainState = A.Fake<IGrainState<SchemasByAppIndexGrain.GrainState>>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId1 = NamedId.Of(Guid.NewGuid(), "my-schema1");
        private readonly NamedId<Guid> schemaId2 = NamedId.Of(Guid.NewGuid(), "my-schema2");
        private readonly SchemasByAppIndexGrain sut;

        public SchemasByAppIndexGrainTests()
        {
            A.CallTo(() => grainState.ClearAsync())
                .Invokes(() => grainState.Value = new SchemasByAppIndexGrain.GrainState());

            sut = new SchemasByAppIndexGrain(grainState);
            sut.ActivateAsync(appId.Id).Wait();
        }

        [Fact]
        public async Task Should_add_schema_id_to_index()
        {
            await sut.AddSchemaAsync(schemaId1.Id, schemaId1.Name);

            var result = await sut.GetSchemaIdAsync(schemaId1.Name);

            Assert.Equal(schemaId1.Id, result);

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_remove_schema_id_from_index()
        {
            await sut.AddSchemaAsync(schemaId1.Id, schemaId1.Name);
            await sut.RemoveSchemaAsync(schemaId1.Id);

            var result = await sut.GetSchemaIdAsync(schemaId1.Name);

            Assert.Equal(Guid.Empty, result);

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task Should_replace_schema_ids_on_rebuild()
        {
            var state = new Dictionary<string, Guid>
            {
                [schemaId1.Name] = schemaId1.Id,
                [schemaId2.Name] = schemaId2.Id
            };

            await sut.RebuildAsync(state);

            Assert.Equal(schemaId1.Id, await sut.GetSchemaIdAsync(schemaId1.Name));
            Assert.Equal(schemaId2.Id, await sut.GetSchemaIdAsync(schemaId2.Name));

            Assert.Equal(new List<Guid> { schemaId1.Id, schemaId2.Id }, await sut.GetSchemaIdsAsync());

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappened();
        }
    }
}
