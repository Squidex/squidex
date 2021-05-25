// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets.Queries
{
    public class AssetEnricherTests
    {
        private readonly ITagService tagService = A.Fake<ITagService>();
        private readonly IRequestCache requestCache = A.Fake<IRequestCache>();
        private readonly IAssetMetadataSource assetMetadataSource1 = A.Fake<IAssetMetadataSource>();
        private readonly IAssetMetadataSource assetMetadataSource2 = A.Fake<IAssetMetadataSource>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly Context requestContext;
        private readonly AssetEnricher sut;

        public AssetEnricherTests()
        {
            var assetMetadataSources = new[]
            {
                assetMetadataSource1,
                assetMetadataSource2
            };

            requestContext = Context.Anonymous(Mocks.App(appId));

            sut = new AssetEnricher(tagService, assetMetadataSources, requestCache);
        }

        [Fact]
        public async Task Should_not_enrich_if_asset_contains_null_tags()
        {
            var source = new AssetEntity { AppId = appId };

            var result = await sut.EnrichAsync(source, requestContext, default);

            Assert.Empty(result.TagNames);
        }

        [Fact]
        public async Task Should_enrich_with_cache_dependencies()
        {
            var source = new AssetEntity { AppId = appId, Id = DomainId.NewGuid(), Version = 13 };

            var result = await sut.EnrichAsync(source, requestContext, default);

            A.CallTo(() => requestCache.AddDependency(result.UniqueId, result.Version))
                .MustHaveHappened();
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

            A.CallTo(() => tagService.DenormalizeTagsAsync(appId.Id, TagGroups.Assets, A<HashSet<string>>.That.Is("id1", "id2")))
                .Returns(new Dictionary<string, string>
                {
                    ["id1"] = "name1",
                    ["id2"] = "name2"
                });

            var result = await sut.EnrichAsync(source, requestContext, default);

            Assert.Equal(new HashSet<string> { "name1", "name2" }, result.TagNames);
        }

        [Fact]
        public async Task Should_not_enrich_asset_with_tag_names_if_disabled()
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

            var result = await sut.EnrichAsync(source, requestContext.Clone(b => b.WithoutAssetEnrichment()), default);

            Assert.Null(result.TagNames);
        }

        [Fact]
        public async Task Should_enrich_asset_with_metadata()
        {
            var source = new AssetEntity
            {
                FileSize = 2 * 1024,
                Tags = new HashSet<string>
                {
                    "id1",
                    "id2"
                },
                AppId = appId
            };

            A.CallTo(() => assetMetadataSource1.Format(A<IAssetEntity>._))
                .Returns(new[] { "metadata1" });

            A.CallTo(() => assetMetadataSource2.Format(A<IAssetEntity>._))
                .Returns(new[] { "metadata2", "metadata3" });

            var result = await sut.EnrichAsync(source, requestContext, default);

            Assert.Equal("metadata1, metadata2, metadata3, 2 kB", result.MetadataText);
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

            A.CallTo(() => tagService.DenormalizeTagsAsync(appId.Id, TagGroups.Assets, A<HashSet<string>>.That.Is("id1", "id2", "id3")))
                .Returns(new Dictionary<string, string>
                {
                    ["id1"] = "name1",
                    ["id2"] = "name2",
                    ["id3"] = "name3"
                });

            var result = await sut.EnrichAsync(new[] { source1, source2 }, requestContext, default);

            Assert.Equal(new HashSet<string> { "name1", "name2" }, result[0].TagNames);
            Assert.Equal(new HashSet<string> { "name2", "name3" }, result[1].TagNames);
        }
    }
}
