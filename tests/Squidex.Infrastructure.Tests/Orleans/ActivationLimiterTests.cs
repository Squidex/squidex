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
    public class ActivationLimiterTests
    {
        private readonly IServiceProvider serviceProvider = A.Fake<IServiceProvider>();
        private readonly IGrainRuntime grainRuntime = A.Fake<IGrainRuntime>();
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly ActivationLimiter sut;

        public interface IGuidGrain : IGrainWithGuidKey, IDeactivatableGrain
        {
        }

        public interface IStringGrain : IGrainWithStringKey, IDeactivatableGrain
        {
        }

        private class GuidGrain : GrainOfGuid, IGuidGrain
        {
            public override IActivationLimit Limit => ActivationLimit.ForGuidKey<IGuidGrain>(3);

            public GuidGrain(IGrainRuntime runtime)
                : base(null, runtime)
            {
            }

            public override Task OnDeactivateAsync()
            {
                return base.OnDeactivateAsync();
            }
        }

        private class StringGrain : GrainOfString, IGuidGrain
        {
            public override IActivationLimit Limit => ActivationLimit.ForStringKey<IStringGrain>(3);

            public StringGrain(IGrainRuntime runtime)
                : base(null, runtime)
            {
            }

            public override Task OnDeactivateAsync()
            {
                return base.OnDeactivateAsync();
            }
        }

        public ActivationLimiterTests()
        {
            A.CallTo(() => serviceProvider.GetService(typeof(IActivationLimiter)))
                .ReturnsLazily(() => sut);

            A.CallTo(() => grainRuntime.ServiceProvider)
                .Returns(serviceProvider);

            sut = new ActivationLimiter(grainFactory);
        }

        [Fact]
        public void Should_deactivate_last_guid_grain()
        {
            var grain1 = CreateGuidGrain();
            var grain1Ref = A.Fake<IGuidGrain>();

            A.CallTo(() => grainFactory.GetGrain<IGuidGrain>(grain1.Key, null))
                .Returns(grain1Ref);

            CreateGuidGrain();
            CreateGuidGrain();
            CreateGuidGrain();

            sut.Dispose();

            A.CallTo(() => grain1Ref.DeactivateAsync())
                .MustHaveHappened();
        }

        [Fact]
        public void Should_not_deactivate_last_guid_grain_if_other_died()
        {
            var grain1 = CreateGuidGrain();
            var grain2 = CreateGuidGrain();
            var grain1Ref = A.Fake<IGuidGrain>();

            A.CallTo(() => grainFactory.GetGrain<IGuidGrain>(grain1.Key, null))
                .Returns(grain1Ref);

            grain2.ReportIamDead();

            CreateGuidGrain();
            CreateGuidGrain();

            sut.Dispose();

            A.CallTo(() => grain1Ref.DeactivateAsync())
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_deactivate_last_string_grain()
        {
            var grain1 = CreateStringGrain();
            var grain1Ref = A.Fake<IStringGrain>();

            A.CallTo(() => grainFactory.GetGrain<IStringGrain>(grain1.Key, null))
                .Returns(grain1Ref);

            CreateStringGrain();
            CreateStringGrain();
            CreateStringGrain();

            sut.Dispose();

            A.CallTo(() => grain1Ref.DeactivateAsync())
                .MustHaveHappened();
        }

        [Fact]
        public void Should_not_deactivate_last_string_grain_if_other_died()
        {
            var grain1 = CreateStringGrain();
            var grain2 = CreateStringGrain();
            var grain1Ref = A.Fake<IStringGrain>();

            A.CallTo(() => grainFactory.GetGrain<IStringGrain>(grain1.Key, null))
                .Returns(grain1Ref);

            grain2.ReportIamDead();

            CreateStringGrain();
            CreateStringGrain();

            sut.Dispose();

            A.CallTo(() => grain1Ref.DeactivateAsync())
                .MustNotHaveHappened();
        }

        private GuidGrain CreateGuidGrain()
        {
            var key = Guid.NewGuid();

            var grain = new GuidGrain(grainRuntime);

            grain.ActivateAsync(key).Wait();
            grain.Participate(null);
            grain.ReportIAmAlive();

            return grain;
        }

        private StringGrain CreateStringGrain()
        {
            var key = Guid.NewGuid().ToString();

            var grain = new StringGrain(grainRuntime);

            grain.ActivateAsync(key).Wait();
            grain.Participate(null);
            grain.ReportIAmAlive();

            return grain;
        }
    }
}
