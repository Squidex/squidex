// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
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
            private readonly IActivationLimit limit = A.Fake<IActivationLimit>();

            public override IActivationLimit Limit => limit;
        }

        [Fact]
        public async Task Should_update_iam_alive_for_grain_base()
        {
            var grain = new MyGrain();

            A.CallTo(() => context.Grain)
                .Returns(grain);

            await sut.Invoke(context);

            A.CallTo(() => grain.Limit.Register(A<IActivationLimiter>.Ignored, grain))
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
