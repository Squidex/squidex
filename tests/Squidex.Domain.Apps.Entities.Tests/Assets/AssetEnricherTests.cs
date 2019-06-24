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
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetEnricherTests
    {
        private readonly ITagService tagService = A.Fake<ITagService>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly AssetEnricher sut;

        public AssetEnricherTests()
        {
            sut = new AssetEnricher(tagService);
        }

        [Fact]
        public async Task Should_not_enrich_if_asset_contains_null_tags()
        {
            var source = new AssetEntity { AppId = appId };

            var result = await sut.EnrichAsync(source);

            Assert.Empty(result.TagNames);
        }

        [Fact]
        public async Task Should_enrich_asset_with_tag_names()
        {
            var source = new AssetEntity
            {
                Tags = new HashSet<string>
                {
                    "id1",
                    "id2"
                },
                AppId = appId
            };

            A.CallTo(() => tagService.DenormalizeTagsAsync(appId.Id, TagGroups.Assets, A<HashSet<string>>.That.IsSameSequenceAs("id1", "id2")))
                .Returns(new Dictionary<string, string>
                {
                    ["id1"] = "name1",
                    ["id2"] = "name2"
                });

            var result = await sut.EnrichAsync(source);

            Assert.Equal(new HashSet<string> { "name1", "name2" }, result.TagNames);
        }

        [Fact]
        public async Task Should_enrich_multiple_assets_with_tag_names()
        {
            var source1 = new AssetEntity
            {
                Tags = new HashSet<string>
                {
                    "id1",
                    "id2"
                },
                AppId = appId
            };

            var source2 = new AssetEntity
            {
                Tags = new HashSet<string>
                {
                    "id2",
                    "id3"
                },
                AppId = appId
            };

            A.CallTo(() => tagService.DenormalizeTagsAsync(appId.Id, TagGroups.Assets, A<HashSet<string>>.That.IsSameSequenceAs("id1", "id2", "id3")))
                .Returns(new Dictionary<string, string>
                {
                    ["id1"] = "name1",
                    ["id2"] = "name2",
                    ["id3"] = "name3"
                });

            var result = await sut.EnrichAsync(new[] { source1, source2 });

            Assert.Equal(new HashSet<string> { "name1", "name2" }, result[0].TagNames);
            Assert.Equal(new HashSet<string> { "name2", "name3" }, result[1].TagNames);
        }
    }
}
