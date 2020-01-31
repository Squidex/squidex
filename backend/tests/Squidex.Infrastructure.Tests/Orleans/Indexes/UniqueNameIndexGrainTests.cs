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
using Xunit;

namespace Squidex.Infrastructure.Orleans.Indexes
{
    public class UniqueNameIndexGrainTests
    {
        private readonly IGrainState<UniqueNameIndexState<Guid>> grainState = A.Fake<IGrainState<UniqueNameIndexState<Guid>>>();
        private readonly NamedId<Guid> id1 = NamedId.Of(Guid.NewGuid(), "my-name1");
        private readonly NamedId<Guid> id2 = NamedId.Of(Guid.NewGuid(), "my-name2");
        private readonly UniqueNameIndexGrain<UniqueNameIndexState<Guid>, Guid> sut;

        public UniqueNameIndexGrainTests()
        {
            A.CallTo(() => grainState.ClearAsync())
                .Invokes(() => grainState.Value = new UniqueNameIndexState<Guid>());

            sut = new UniqueNameIndexGrain<UniqueNameIndexState<Guid>, Guid>(grainState);
        }

        [Fact]
        public async Task Should_not_write_to_state_for_reservation()
        {
            await sut.ReserveAsync(id1.Id, id1.Name);

            A.CallTo(() => grainState.WriteAsync())
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_add_to_index_if_reservation_token_acquired()
        {
            await AddAsync(id1);

            var result = await sut.GetIdAsync(id1.Name);

            Assert.Equal(id1.Id, result);

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_make_reservation_if_name_already_reserved()
        {
            await sut.ReserveAsync(id1.Id, id1.Name);

            var newToken = await sut.ReserveAsync(id1.Id, id1.Name);

            Assert.Null(newToken);
        }

        [Fact]
        public async Task Should_not_make_reservation_if_name_taken()
        {
            await AddAsync(id1);

            var newToken = await sut.ReserveAsync(id1.Id, id1.Name);

            Assert.Null(newToken);
        }

        [Fact]
        public async Task Should_provide_number_of_entries()
        {
            await AddAsync(id1);
            await AddAsync(id2);

            var count = await sut.CountAsync();

            Assert.Equal(2, count);
        }

        [Fact]
        public async Task Should_clear_all_entries()
        {
            await AddAsync(id1);
            await AddAsync(id2);

            await sut.ClearAsync();

            var count = await sut.CountAsync();

            Assert.Equal(0, count);
        }

        [Fact]
        public async Task Should_make_reservation_after_reservation_removed()
        {
            var token = await sut.ReserveAsync(id1.Id, id1.Name);

            await sut.RemoveReservationAsync(token!);

            var newToken = await sut.ReserveAsync(id1.Id, id1.Name);

            Assert.NotNull(newToken);
        }

        [Fact]
        public async Task Should_make_reservation_after_id_removed()
        {
            await AddAsync(id1);

            await sut.RemoveAsync(id1.Id);

            var newToken = await sut.ReserveAsync(id1.Id, id1.Name);

            Assert.NotNull(newToken);
        }

        [Fact]
        public async Task Should_remove_id_from_index()
        {
            await AddAsync(id1);

            await sut.RemoveAsync(id1.Id);

            var result = await sut.GetIdAsync(id1.Name);

            Assert.Equal(Guid.Empty, result);

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task Should_not_write_to_state_if_nothing_removed()
        {
            await sut.RemoveAsync(id1.Id);

            A.CallTo(() => grainState.WriteAsync())
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_ignore_error_if_removing_reservation_with_Invalid_token()
        {
            await sut.RemoveReservationAsync(null);
        }

        [Fact]
        public async Task Should_ignore_error_if_completing_reservation_with_Invalid_token()
        {
            await sut.AddAsync(null!);
        }

        [Fact]
        public async Task Should_replace_ids_on_rebuild()
        {
            var state = new Dictionary<string, Guid>
            {
                [id1.Name] = id1.Id,
                [id2.Name] = id2.Id
            };

            await sut.RebuildAsync(state);

            Assert.Equal(id1.Id, await sut.GetIdAsync(id1.Name));
            Assert.Equal(id2.Id, await sut.GetIdAsync(id2.Name));

            var result = await sut.GetIdsAsync();

            Assert.Equal(new List<Guid> { id1.Id, id2.Id }, result);

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_provide_multiple_ids_by_names()
        {
            await AddAsync(id1);
            await AddAsync(id2);

            var result = await sut.GetIdsAsync(new[] { id1.Name, id2.Name, "not-found" });

            Assert.Equal(new List<Guid> { id1.Id, id2.Id }, result);
        }

        private async Task AddAsync(NamedId<Guid> id)
        {
            var token = await sut.ReserveAsync(id.Id, id.Name);

            await sut.AddAsync(token!);
        }
    }
}
