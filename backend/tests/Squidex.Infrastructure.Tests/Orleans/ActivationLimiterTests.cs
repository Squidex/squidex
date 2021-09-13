// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using FakeItEasy;
using Orleans;
using Orleans.Core;
using Orleans.Runtime;
using Xunit;

namespace Squidex.Infrastructure.Orleans
{
    public class ActivationLimiterTests
    {
        private readonly IGrainIdentity grainIdentity = A.Fake<IGrainIdentity>();
        private readonly IGrainRuntime grainRuntime = A.Fake<IGrainRuntime>();
        private readonly ActivationLimiter sut;

        private sealed class MyGrain : GrainBase
        {
            public MyGrain(IGrainIdentity identity, IGrainRuntime runtime, IActivationLimit limit)
                : base(identity, runtime)
            {
                limit.SetLimit(3, TimeSpan.FromMinutes(3));
            }
        }

        public ActivationLimiterTests()
        {
            sut = new ActivationLimiter();
        }

        [Fact]
        public void Should_deactivate_last_grain()
        {
            var grain1 = CreateGuidGrain();

            CreateGuidGrain();
            CreateGuidGrain();
            CreateGuidGrain();

            A.CallTo(() => grainRuntime.DeactivateOnIdle(grain1))
                .MustHaveHappened();
        }

        [Fact]
        public void Should_not_deactivate_last_grain_if_other_died()
        {
            CreateGuidGrain();
            CreateGuidGrain().ReportIAmDead();
            CreateGuidGrain();
            CreateGuidGrain();

            A.CallTo(() => grainRuntime.DeactivateOnIdle(A<Grain>._))
                .MustNotHaveHappened();
        }

        private MyGrain CreateGuidGrain()
        {
            var context = A.Fake<IGrainActivationContext>();

            var limit = new ActivationLimit(context, sut);

            var serviceProvider = A.Fake<IServiceProvider>();

            A.CallTo(() => grainRuntime.ServiceProvider)
                .Returns(serviceProvider);

            A.CallTo(() => context.ActivationServices)
                .Returns(serviceProvider);

            A.CallTo(() => serviceProvider.GetService(typeof(IActivationLimit)))
                .Returns(limit);

            A.CallTo(() => serviceProvider.GetService(typeof(IGrainRuntime)))
                .Returns(grainRuntime);

            var grain = new MyGrain(grainIdentity, grainRuntime, limit);

            A.CallTo(() => context.GrainInstance)
                .Returns(grain);

            A.CallTo(() => context.GrainType)
                .Returns(typeof(MyGrain));

            grain.ReportIAmAlive();

            return grain;
        }
    }
}
