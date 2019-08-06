// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets.Queries
{
    public class AssetQueryServiceTests
    {
        private readonly IAssetEnricher assetEnricher = A.Fake<IAssetEnricher>();
        private readonly IAssetRepository assetRepository = A.Fake<IAssetRepository>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly Context requestContext;
        private readonly AssetQueryParser queryParser = A.Fake<AssetQueryParser>();
        private readonly AssetQueryService sut;

        public AssetQueryServiceTests()
        {
            requestContext = new Context(Mocks.FrontendUser(), Mocks.App(appId));

            A.CallTo(() => queryParser.ParseQuery(requestContext, A<Q>.Ignored))
                .Returns(new ClrQuery());

            sut = new AssetQueryService(assetEnricher, assetRepository, queryParser);
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

            var result = await sut.QueryAsync(requestContext, Q.Empty.WithIds(ids));

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

            A.CallTo(() => assetRepository.QueryAsync(appId.Id, A<ClrQuery>.Ignored))
                .Returns(ResultList.CreateFrom(8, found1, found2));

            A.CallTo(() => assetEnricher.EnrichAsync(A<IEnumerable<IAssetEntity>>.That.IsSameSequenceAs(found1, found2)))
                .Returns(new List<IEnrichedAssetEntity> { enriched1, enriched2 });

            var result = await sut.QueryAsync(requestContext, Q.Empty);

            Assert.Equal(8, result.Total);

            Assert.Equal(new[] { enriched1, enriched2 }, result.ToArray());
        }
    }
}