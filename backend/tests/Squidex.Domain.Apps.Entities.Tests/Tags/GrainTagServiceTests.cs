// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Tags
{
    public class GrainTagServiceTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly ITagGrain grain = A.Fake<ITagGrain>();
        private readonly DomainId appId = DomainId.NewGuid();
        private readonly GrainTagService sut;

        public GrainTagServiceTests()
        {
            A.CallTo(() => grainFactory.GetGrain<ITagGrain>($"{appId}_Assets", null))
                .Returns(grain);

            sut = new GrainTagService(grainFactory);
        }

        [Fact]
        public async Task Should_call_grain_if_clearing()
        {
            await sut.ClearAsync(appId, TagGroups.Assets);

            A.CallTo(() => grain.ClearAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_grain_if_rebuilding()
        {
            var tags = new TagsExport();

            await sut.RebuildTagsAsync(appId, TagGroups.Assets, tags);

            A.CallTo(() => grain.RebuildAsync(tags))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_grain_if_retrieving_raw_tags()
        {
            await sut.GetExportableTagsAsync(appId, TagGroups.Assets);

            A.CallTo(() => grain.GetExportableTagsAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_grain_if_retrieving_tags()
        {
            await sut.GetTagsAsync(appId, TagGroups.Assets);

            A.CallTo(() => grain.GetTagsAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_grain_if_resolving_tag_ids()
        {
            var tagNames = new HashSet<string>();

            await sut.GetTagIdsAsync(appId, TagGroups.Assets, tagNames);

            A.CallTo(() => grain.GetTagIdsAsync(tagNames))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_grain_if_denormalizing_tags()
        {
            var tagIds = new HashSet<string>();

            await sut.DenormalizeTagsAsync(appId, TagGroups.Assets, tagIds);

            A.CallTo(() => grain.DenormalizeTagsAsync(tagIds))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_grain_if_normalizing_tags()
        {
            var tagIds = new HashSet<string>();
            var tagNames = new HashSet<string>();

            await sut.NormalizeTagsAsync(appId, TagGroups.Assets, tagNames, tagIds);

            A.CallTo(() => grain.NormalizeTagsAsync(tagNames, tagIds))
                .MustHaveHappened();
        }
    }
}
