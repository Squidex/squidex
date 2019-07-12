// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetQueryServiceTests
    {
        private readonly ITagService tagService = A.Fake<ITagService>();
        private readonly IAssetEnricher assetEnricher = A.Fake<IAssetEnricher>();
        private readonly IAssetRepository assetRepository = A.Fake<IAssetRepository>();
        private readonly IAppEntity app = A.Fake<IAppEntity>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly Context context;
        private readonly AssetQueryService sut;

        public AssetQueryServiceTests()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());

            A.CallTo(() => app.Id).Returns(appId.Id);
            A.CallTo(() => app.Name).Returns(appId.Name);
            A.CallTo(() => app.LanguagesConfig).Returns(LanguagesConfig.English);

            context = new Context(user, app);

            var options = Options.Create(new AssetOptions { DefaultPageSize = 30 });

            sut = new AssetQueryService(tagService, assetEnricher, assetRepository, options);
        }

        [Fact]
        public void Should_provide_default_page_size()
        {
            var result = sut.DefaultPageSizeGraphQl;

            Assert.Equal(20, result);
        }

        [Fact]
        public async Task Should_find_asset_by_id_and_enrich_it()
        {
            var found = new AssetEntity { Id = Guid.NewGuid() };

            var enriched = new AssetEntity();

            A.CallTo(() => assetRepository.FindAssetAsync(found.Id, false))
                .Returns(found);

            A.CallTo(() => assetEnricher.EnrichAsync(found))
                .Returns(enriched);

            var result = await sut.FindAssetAsync(found.Id);

            Assert.Same(enriched, result);
        }

        [Fact]
        public async Task Should_find_assets_by_hash_and_and_enrich_it()
        {
            var found = new AssetEntity { Id = Guid.NewGuid() };

            var enriched = new AssetEntity();

            A.CallTo(() => assetRepository.QueryByHashAsync(appId.Id, "hash"))
                .Returns(new List<IAssetEntity> { found });

            A.CallTo(() => assetEnricher.EnrichAsync(A<IEnumerable<IAssetEntity>>.That.IsSameSequenceAs(found)))
                .Returns(new List<IEnrichedAssetEntity> { enriched });

            var result = await sut.QueryByHashAsync(appId.Id, "hash");

            Assert.Same(enriched, result.Single());
        }

        [Fact]
        public async Task Should_load_assets_from_ids_and_resolve_tags()
        {
            var found1 = new AssetEntity { Id = Guid.NewGuid() };
            var found2 = new AssetEntity { Id = Guid.NewGuid() };

            var enriched1 = new AssetEntity();
            var enriched2 = new AssetEntity();

            var ids = HashSet.Of(found1.Id, found2.Id);

            A.CallTo(() => assetRepository.QueryAsync(appId.Id, A<HashSet<Guid>>.That.IsSameSequenceAs(ids)))
                .Returns(ResultList.CreateFrom(8, found1, found2));

            A.CallTo(() => assetEnricher.EnrichAsync(A<IEnumerable<IAssetEntity>>.That.IsSameSequenceAs(found1, found2)))
                .Returns(new List<IEnrichedAssetEntity> { enriched1, enriched2 });

            var result = await sut.QueryAsync(context, Q.Empty.WithIds(ids));

            Assert.Equal(8, result.Total);

            Assert.Equal(new[] { enriched1, enriched2 }, result.ToArray());
        }

        [Fact]
        public async Task Should_load_assets_with_query_and_resolve_tags()
        {
            var found1 = new AssetEntity { Id = Guid.NewGuid() };
            var found2 = new AssetEntity { Id = Guid.NewGuid() };

            var enriched1 = new AssetEntity();
            var enriched2 = new AssetEntity();

            A.CallTo(() => assetRepository.QueryAsync(appId.Id, A<Query>.Ignored))
                .Returns(ResultList.CreateFrom(8, found1, found2));

            A.CallTo(() => assetEnricher.EnrichAsync(A<IEnumerable<IAssetEntity>>.That.IsSameSequenceAs(found1, found2)))
                .Returns(new List<IEnrichedAssetEntity> { enriched1, enriched2 });

            var result = await sut.QueryAsync(context, Q.Empty);

            Assert.Equal(8, result.Total);

            Assert.Equal(new[] { enriched1, enriched2 }, result.ToArray());
        }

        [Fact]
        public async Task Should_transform_odata_query()
        {
            var query = Q.Empty.WithODataQuery("$top=100&$orderby=fileName asc&$search=Hello World");

            await sut.QueryAsync(context, query);

            A.CallTo(() => assetRepository.QueryAsync(appId.Id, A<Query>.That.Is("FullText: 'Hello World'; Take: 100; Sort: fileName Ascending")))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_transform_odata_query_and_enrich_with_defaults()
        {
            var query = Q.Empty.WithODataQuery("$top=200&$filter=fileName eq 'ABC'");

            await sut.QueryAsync(context, query);

            A.CallTo(() => assetRepository.QueryAsync(appId.Id, A<Query>.That.Is("Filter: fileName == 'ABC'; Take: 200; Sort: lastModified Descending")))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_apply_default_page_size()
        {
            var query = Q.Empty;

            await sut.QueryAsync(context, query);

            A.CallTo(() => assetRepository.QueryAsync(appId.Id, A<Query>.That.Is("Take: 30; Sort: lastModified Descending")))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_limit_number_of_assets()
        {
            var query = Q.Empty.WithODataQuery("$top=300&$skip=20");

            await sut.QueryAsync(context, query);

            A.CallTo(() => assetRepository.QueryAsync(appId.Id, A<Query>.That.Is("Skip: 20; Take: 200; Sort: lastModified Descending")))
                .MustHaveHappened();
        }
    }
}