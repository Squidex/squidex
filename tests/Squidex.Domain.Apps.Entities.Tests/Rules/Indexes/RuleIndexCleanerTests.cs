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

namespace Squidex.Domain.Apps.Entities.Rules.Indexes
{
    public class RuleIndexCleanerTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly IRulesByAppIndex index = A.Fake<IRulesByAppIndex>();
        private readonly Guid appId = Guid.NewGuid();
        private readonly RuleIndexCleaner sut;

        public RuleIndexCleanerTests()
        {
            A.CallTo(() => grainFactory.GetGrain<IRulesByAppIndex>(appId, null))
                .Returns(index);

            sut = new RuleIndexCleaner(grainFactory);
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
