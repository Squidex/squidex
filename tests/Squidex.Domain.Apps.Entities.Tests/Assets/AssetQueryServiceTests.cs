// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetQueryServiceTests
    {
        private readonly ITagService tagService = A.Fake<ITagService>();
        private readonly IAssetRepository assetRepository = A.Fake<IAssetRepository>();
        private readonly IAppEntity app = A.Fake<IAppEntity>();
        private readonly Guid appId = Guid.NewGuid();
        private readonly string appName = "my-app";
        private readonly ClaimsIdentity identity = new ClaimsIdentity();
        private readonly QueryContext context;
        private readonly AssetQueryService sut;

        public AssetQueryServiceTests()
        {
            var user = new ClaimsPrincipal(identity);

            A.CallTo(() => app.Id).Returns(appId);
            A.CallTo(() => app.Name).Returns(appName);
            A.CallTo(() => app.LanguagesConfig).Returns(LanguagesConfig.English);

            context = QueryContext.Create(app, user);

            A.CallTo(() => tagService.DenormalizeTagsAsync(appId, TagGroups.Assets, A<HashSet<string>>.That.IsSameSequenceAs("id1", "id2", "id3")))
                .Returns(new Dictionary<string, string>
                {
                    ["id1"] = "name1",
                    ["id2"] = "name2",
                    ["id3"] = "name3"
                });

            sut = new AssetQueryService(tagService, assetRepository);
        }

        [Fact]
        public async Task Should_find_asset_by_id_and_resolve_tags()
        {
            var id = Guid.NewGuid();

            A.CallTo(() => assetRepository.FindAssetAsync(id))
                .Returns(CreateAsset(id, "id1", "id2", "id3"));

            var result = await sut.FindAssetAsync(context, id);

            Assert.Equal(HashSet.Of("name1", "name2", "name3"), result.Tags);
        }

        [Fact]
        public async Task Should_load_assets_from_ids_and_resolve_tags()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var ids = HashSet.Of(id1, id2);

            A.CallTo(() => assetRepository.QueryAsync(appId, A<HashSet<Guid>>.That.IsSameSequenceAs(ids)))
                .Returns(ResultList.Create(8,
                    CreateAsset(id1, "id1", "id2", "id3"),
                    CreateAsset(id2)));

            var result = await sut.QueryAsync(context, Q.Empty.WithIds(ids));

            Assert.Equal(8, result.Total);
            Assert.Equal(2, result.Count);

            Assert.Equal(HashSet.Of("name1", "name2", "name3"), result[0].Tags);
            Assert.Empty(result[1].Tags);
        }

        [Fact]
        public async Task Should_load_assets_with_query_and_resolve_tags()
        {
            A.CallTo(() => assetRepository.QueryAsync(appId, A<Query>.Ignored))
                .Returns(ResultList.Create(8,
                    CreateAsset(Guid.NewGuid(), "id1", "id2"),
                    CreateAsset(Guid.NewGuid(), "id2", "id3")));

            var result = await sut.QueryAsync(context, Q.Empty);

            Assert.Equal(8, result.Total);
            Assert.Equal(2, result.Count);

            Assert.Equal(HashSet.Of("name1", "name2"), result[0].Tags);
            Assert.Equal(HashSet.Of("name2", "name3"), result[1].Tags);
        }

        [Fact]
        public async Task Should_transform_odata_query()
        {
            await sut.QueryAsync(context, Q.Empty.WithODataQuery("$top=100&$orderby=fileName asc&$search=Hello World"));

            A.CallTo(() => assetRepository.QueryAsync(appId, A<Query>.That.Matches(x => x.ToString() == "FullText: 'Hello World'; Take: 100; Sort: fileName Ascending")))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_transform_odata_query_and_enrich_with_defaults()
        {
            await sut.QueryAsync(context, Q.Empty.WithODataQuery("$filter=fileName eq 'ABC'"));

            A.CallTo(() => assetRepository.QueryAsync(appId, A<Query>.That.Matches(x => x.ToString() == "Filter: fileName == 'ABC'; Take: 200; Sort: lastModified Descending")))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_limit_number_of_assets()
        {
            await sut.QueryAsync(context, Q.Empty.WithODataQuery("$top=300&$skip=20"));

            A.CallTo(() => assetRepository.QueryAsync(appId, A<Query>.That.Matches(x => x.ToString() == "Skip: 20; Take: 200; Sort: lastModified Descending")))
                .MustHaveHappened();
        }

        private static IAssetEntity CreateAsset(Guid id, params string[] tags)
        {
            var asset = A.Fake<IAssetEntity>();

            A.CallTo(() => asset.Id).Returns(id);
            A.CallTo(() => asset.Tags).Returns(HashSet.Of(tags));

            return asset;
        }
    }
}