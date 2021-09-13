// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Counter
{
    public class CounterDeleterTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly CounterDeleter sut;

        public CounterDeleterTests()
        {
            sut = new CounterDeleter(grainFactory);
        }

        [Fact]
        public void Should_run_with_default_order()
        {
            var order = ((IDeleter)sut).Order;

            Assert.Equal(0, order);
        }

        [Fact]
        public async Task Should_remove_events_from_streams()
        {
            var app = Mocks.App(NamedId.Of(DomainId.NewGuid(), "my-app"));

            var grain = A.Fake<ICounterGrain>();

            A.CallTo(() => grainFactory.GetGrain<ICounterGrain>(app.Id.ToString(), null))
                .Returns(grain);

            await sut.DeleteAppAsync(app, default);

            A.CallTo(() => grain.ClearAsync())
                .MustHaveHappened();
        }
    }
}
