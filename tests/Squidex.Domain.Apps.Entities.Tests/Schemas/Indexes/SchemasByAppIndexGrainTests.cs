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
using Squidex.Infrastructure.States;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes
{
    public class SchemasByAppIndexGrainTests
    {
        private readonly IStore<Guid> store = A.Fake<IStore<Guid>>();
        private readonly IPersistence<SchemasByAppIndexGrain.State> persistence = A.Fake<IPersistence<SchemasByAppIndexGrain.State>>();
        private readonly Guid schemaId1 = Guid.NewGuid();
        private readonly Guid schemaId2 = Guid.NewGuid();
        private readonly string schemaName1 = "my-schema1";
        private readonly string schemaName2 = "my-schema2";
        private readonly SchemasByAppIndexGrain sut;

        public SchemasByAppIndexGrainTests()
        {
            A.CallTo(() => store.WithSnapshots(A<Type>.Ignored, A<Guid>.Ignored, A<Func<SchemasByAppIndexGrain.State, Task>>.Ignored))
                .Returns(persistence);

            sut = new SchemasByAppIndexGrain(store);
            sut.OnActivateAsync(Guid.NewGuid()).Wait();
        }

        [Fact]
        public async Task Should_add_schema_id_to_index()
        {
            await sut.AddSchemaAsync(schemaId1, schemaName1);

            var result = await sut.GetSchemaIdAsync(schemaName1);

            Assert.Equal(schemaId1, result);

            A.CallTo(() => persistence.WriteSnapshotAsync(A<SchemasByAppIndexGrain.State>.Ignored))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_delete_and_reset_state_when_cleaning()
        {
            await sut.AddSchemaAsync(schemaId1, schemaName1);
            await sut.ClearAsync();

            var id = await sut.GetSchemaIdAsync(schemaName1);

            Assert.Equal(id, Guid.Empty);

            A.CallTo(() => persistence.DeleteAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_remove_schema_id_from_index()
        {
            await sut.AddSchemaAsync(schemaId1, schemaName1);
            await sut.RemoveSchemaAsync(schemaId1);

            var result = await sut.GetSchemaIdAsync(schemaName1);

            Assert.Equal(Guid.Empty, result);

            A.CallTo(() => persistence.WriteSnapshotAsync(A<SchemasByAppIndexGrain.State>.Ignored))
                .MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task Should_replace_schema_ids_on_rebuild()
        {
            var state = new Dictionary<string, Guid>
            {
                [schemaName1] = schemaId1,
                [schemaName2] = schemaId2
            };

            await sut.RebuildAsync(state);

            Assert.Equal(schemaId1, await sut.GetSchemaIdAsync(schemaName1));
            Assert.Equal(schemaId2, await sut.GetSchemaIdAsync(schemaName2));

            Assert.Equal(new List<Guid> { schemaId1, schemaId2 }, await sut.GetSchemaIdsAsync());

            A.CallTo(() => persistence.WriteSnapshotAsync(A<SchemasByAppIndexGrain.State>.Ignored))
                .MustHaveHappened();
        }
    }
}
