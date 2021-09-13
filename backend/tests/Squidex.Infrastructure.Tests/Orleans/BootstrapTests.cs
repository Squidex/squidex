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
using Orleans.Runtime;
using Xunit;

namespace Squidex.Infrastructure.Orleans
{
    public class BootstrapTests
    {
        private readonly IBackgroundGrain grain = A.Fake<IBackgroundGrain>();
        private readonly GrainBootstrap<IBackgroundGrain> sut;

        public BootstrapTests()
        {
            var factory = A.Fake<IGrainFactory>();

            sut = new GrainBootstrap<IBackgroundGrain>(factory);

            A.CallTo(() => factory.GetGrain<IBackgroundGrain>("Default", null))
                .Returns(grain);
        }

        [Fact]
        public async Task Should_activate_grain_on_run()
        {
            await sut.StartAsync(default);

            A.CallTo(() => grain.ActivateAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_fail_on_non_rejection_exception()
        {
            A.CallTo(() => grain.ActivateAsync())
                .Throws(new InvalidOperationException());

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.StartAsync(default));
        }

        [Fact]
        public async Task Should_retry_after_rejection_exception()
        {
            A.CallTo(() => grain.ActivateAsync())
                .Throws(new OrleansException()).Once();

            await sut.StartAsync(default);

            A.CallTo(() => grain.ActivateAsync())
                .MustHaveHappened(2, Times.Exactly);
        }

        [Fact]
        public async Task Should_fail_after_10_rejection_exception()
        {
            A.CallTo(() => grain.ActivateAsync())
                .Throws(new OrleansException());

            await Assert.ThrowsAsync<OrleansException>(() => sut.StartAsync(default));

            A.CallTo(() => grain.ActivateAsync())
                .MustHaveHappened(10, Times.Exactly);
        }
    }
}
