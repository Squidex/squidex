// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Xunit;

namespace Squidex.Infrastructure.Orleans.Indexes
{
    public class UniqueNameGrainTests
    {
        private readonly UniqueNameGrain<string> sut;

        public UniqueNameGrainTests()
        {
            sut = new UniqueNameGrain<string>();
        }

        [Fact]
        public async Task Should_acquire_token_if_not_reserved()
        {
            var token = await sut.ReserveAsync("1", "name1");

            Assert.NotNull(token);
        }

        [Fact]
        public async Task Should_reserve_again_if_reservation_removed()
        {
            var token1 = await sut.ReserveAsync("1", "name1");

            await sut.RemoveReservationAsync(token1);

            var token = await sut.ReserveAsync("2", "name1");

            Assert.NotNull(token);
        }

        [Fact]
        public async Task Should_not_make_reservation_if_name_already_reserved()
        {
            await sut.ReserveAsync("1", "name1");

            var token = await sut.ReserveAsync("2", "name1");

            Assert.Null(token);
        }

        [Fact]
        public async Task Should_acquire_token_again_if_reserved_with_same_id()
        {
            var token1 = await sut.ReserveAsync("1", "name1");
            var token2 = await sut.ReserveAsync("1", "name1");

            Assert.NotNull(token1);
            Assert.NotNull(token2);
            Assert.Equal(token2, token1);
        }
    }
}
