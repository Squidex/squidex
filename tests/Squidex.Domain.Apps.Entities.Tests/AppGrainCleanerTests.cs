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
using Xunit;

namespace Squidex.Domain.Apps.Entities
{
    public class AppGrainCleanerTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly ICleanableAppGrain index = A.Fake<ICleanableAppGrain>();
        private readonly Guid appId = Guid.NewGuid();
        private readonly AppGrainCleaner<ICleanableAppGrain> sut;

        public AppGrainCleanerTests()
        {
            A.CallTo(() => grainFactory.GetGrain<ICleanableAppGrain>(appId, null))
                .Returns(index);

            sut = new AppGrainCleaner<ICleanableAppGrain>(grainFactory);
        }

        [Fact]
        public async Task Should_forward_to_index()
        {
            await sut.ClearAsync(appId);

            A.CallTo(() => index.ClearAsync())
                .MustHaveHappened();
        }
    }
}
