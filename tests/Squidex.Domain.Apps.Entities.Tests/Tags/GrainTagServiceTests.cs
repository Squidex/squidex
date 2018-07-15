// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Tags
{
    public class GrainTagServiceTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly ITagGrain grain = A.Fake<ITagGrain>();
        private readonly Guid appId = Guid.NewGuid();
        private readonly GrainTagService sut;

        public GrainTagServiceTests()
        {
            A.CallTo(() => grainFactory.GetGrain<ITagGrain>($"{appId}_Assets", null))
                .Returns(grain);

            sut = new GrainTagService(grainFactory);
        }

        [Fact]
        public async Task Should_call_grain_when_retrieving_tas()
        {
            await sut.GetTagsAsync(appId, TagGroups.Assets);

            A.CallTo(() => grain.GetTagsAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_grain_when_resolving_tag_ids()
        {
            var tagNames = new HashSet<string>();

            await sut.GetTagIdsAsync(appId, TagGroups.Assets, tagNames);

            A.CallTo(() => grain.GetTagIdsAsync(tagNames))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_grain_when_denormalizing_tags()
        {
            var tagIds = new HashSet<string>();

            await sut.DenormalizeTagsAsync(appId, TagGroups.Assets, tagIds);

            A.CallTo(() => grain.DenormalizeTagsAsync(tagIds))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_grain_when_normalizing_tags()
        {
            var tagIds = new HashSet<string>();
            var tagNames = new HashSet<string>();

            await sut.NormalizeTagsAsync(appId, TagGroups.Assets, tagNames, tagIds);

            A.CallTo(() => grain.NormalizeTagsAsync(tagNames, tagIds))
                .MustHaveHappened();
        }
    }
}
