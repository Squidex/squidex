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
    public class ActivationLimiterFilterTests
    {
        private readonly IIncomingGrainCallContext context = A.Fake<IIncomingGrainCallContext>();
        private readonly ActivationLimiterFilter sut;

        public ActivationLimiterFilterTests()
        {
            sut = new ActivationLimiterFilter();
        }

        public sealed class MyGrain : GrainBase
        {
            public MyGrain(IActivationLimit limit)
                : base(null, CreateRuntime(limit))
            {
            }

            private static IGrainRuntime CreateRuntime(IActivationLimit limit)
            {
                var serviceProvider = A.Fake<IServiceProvider>();

                var grainRuntime = A.Fake<IGrainRuntime>();

                A.CallTo(() => grainRuntime.ServiceProvider)
                    .Returns(serviceProvider);

                A.CallTo(() => serviceProvider.GetService(typeof(IActivationLimit)))
                    .Returns(limit);

                return grainRuntime;
            }
        }

        [Fact]
        public async Task Should_update_iam_alive_for_grain_base()
        {
            var limit = A.Fake<IActivationLimit>();

            var grain = new MyGrain(limit);

            A.CallTo(() => context.Grain)
                .Returns(grain);

            await sut.Invoke(context);

            A.CallTo(() => limit.ReportIAmAlive())
                .MustHaveHappened();

            A.CallTo(() => context.Invoke())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_also_handle_other_grains()
        {
            var grain = A.Fake<Grain>();

            A.CallTo(() => context.Grain)
                .Returns(grain);

            await sut.Invoke(context);

            A.CallTo(() => context.Invoke())
                .MustHaveHappened();
        }
    }
}
