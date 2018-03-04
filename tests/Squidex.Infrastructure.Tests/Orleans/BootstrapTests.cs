// ==========================================================================
//  EventConsumerBootstrapTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Xunit;

namespace Squidex.Infrastructure.Orleans
{
    public sealed class BootstrapTests
    {
        private readonly IBackgroundGrain grain = A.Fake<IBackgroundGrain>();
        private readonly Bootstrap<IBackgroundGrain> sut;

        public BootstrapTests()
        {
            var factory = A.Fake<IGrainFactory>();

            sut = new Bootstrap<IBackgroundGrain>(factory);

            A.CallTo(() => factory.GetGrain<IBackgroundGrain>("Default", null))
                .Returns(grain);
        }

        [Fact]
        public async Task Should_activate_grain_on_run()
        {
            await sut.Execute(CancellationToken.None);

            A.CallTo(() => grain.ActivateAsync())
                .MustHaveHappened();
        }
    }
}
