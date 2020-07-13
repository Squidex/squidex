// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Xunit;

namespace Squidex.Infrastructure.Orleans.Indexes
{
    public class IdsIndexGrainTests
    {
        private readonly IGrainState<IdsIndexState<DomainId>> grainState = A.Fake<IGrainState<IdsIndexState<DomainId>>>();
        private readonly DomainId id1 = DomainId.NewGuid();
        private readonly DomainId id2 = DomainId.NewGuid();
        private readonly IdsIndexGrain<IdsIndexState<DomainId>, DomainId> sut;

        public IdsIndexGrainTests()
        {
            A.CallTo(() => grainState.ClearAsync())
                .Invokes(() => grainState.Value = new IdsIndexState<DomainId>());

            sut = new IdsIndexGrain<IdsIndexState<DomainId>, DomainId>(grainState);
        }

        [Fact]
        public async Task Should_add_id_to_index()
        {
            await sut.AddAsync(id1);
            await sut.AddAsync(id2);

            var result = await sut.GetIdsAsync();

            Assert.Equal(new List<DomainId> { id1, id2 }, result);

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task Should_provide_number_of_entries()
        {
            await sut.AddAsync(id1);
            await sut.AddAsync(id2);

            var count = await sut.CountAsync();

            Assert.Equal(2, count);
        }

        [Fact]
        public async Task Should_clear_all_entries()
        {
            await sut.AddAsync(id1);
            await sut.AddAsync(id2);

            await sut.ClearAsync();

            var count = await sut.CountAsync();

            Assert.Equal(0, count);
        }

        [Fact]
        public async Task Should_remove_id_from_index()
        {
            await sut.AddAsync(id1);
            await sut.AddAsync(id2);
            await sut.RemoveAsync(id1);

            var result = await sut.GetIdsAsync();

            Assert.Equal(new List<DomainId> { id2 }, result);

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappenedTwiceOrMore();
        }

        [Fact]
        public async Task Should_replace__ids_on_rebuild()
        {
            var state = new HashSet<DomainId>
            {
                id1,
                id2
            };

            await sut.RebuildAsync(state);

            var result = await sut.GetIdsAsync();

            Assert.Equal(new List<DomainId> { id1, id2 }, result);

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappened();
        }
    }
}
