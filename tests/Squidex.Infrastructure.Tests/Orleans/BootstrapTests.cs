// ==========================================================================
//  EventConsumerBootstrapTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

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
        public void Should_activate_grain_on_run()
        {
            sut.Run();

            A.CallTo(() => grain.ActivateAsync())
                .MustHaveHappened();
        }
    }
}
