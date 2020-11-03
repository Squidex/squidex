// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
        private readonly IAssetFolderRepository assetFolderRepository = A.Fake<IAssetFolderRepository>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly Context requestContext;
        private readonly AssetQueryParser queryParser = A.Fake<AssetQueryParser>();
        private readonly AssetQueryService sut;

        public AssetQueryServiceTests()
        {
            requestContext = new Context(Mocks.FrontendUser(), Mocks.App(appId));

            A.CallTo(() => queryParser.ParseQueryAsync(requestContext, A<Q>._))
                .Returns(new ClrQuery());

            sut = new AssetQueryService(assetEnricher, assetRepository, assetFolderRepository, queryParser);
        }

        [Fact]
        public async Task Should_find_asset_by_id_and_enrich_it()
        {
            var found = new AssetEntity { Id = DomainId.NewGuid() };

            var enriched = new AssetEntity();

            A.CallTo(() => assetRepository.FindAssetAsync(appId.Id, found.Id))
                .Returns(found);

            A.CallTo(() => assetEnricher.EnrichAsync(found, requestContext))
                .Returns(enriched);

            var result = await sut.FindAsync(requestContext, found.Id);

            Assert.Same(enriched, result);
        }

        [Fact]
        public async Task Should_find_assets_by_hash_and_and_enrich_it()
        {
            var found = new AssetEntity { Id = DomainId.NewGuid() };

            var enriched = new AssetEntity();

            A.CallTo(() => assetRepository.FindAssetAsync(appId.Id, "hash", "name", 123))
                .Returns(found);

            A.CallTo(() => assetEnricher.EnrichAsync(found, requestContext))
                .Returns(enriched);

            var result = await sut.FindByHashAsync(requestContext, "hash", "name", 123);

            Assert.Same(enriched, result);
        }

        [Fact]
        public async Task Should_load_assets_from_ids_and_resolve_tags()
        {
            var found1 = new AssetEntity { Id = DomainId.NewGuid() };
            var found2 = new AssetEntity { Id = DomainId.NewGuid() };

            var enriched1 = new AssetEntity();
            var enriched2 = new AssetEntity();

            var ids = HashSet.Of(found1.Id, found2.Id);

            A.CallTo(() => assetRepository.QueryAsync(appId.Id, A<HashSet<DomainId>>.That.Is(ids)))
                .Returns(ResultList.CreateFrom(8, found1, found2));

            A.CallTo(() => assetEnricher.EnrichAsync(A<IEnumerable<IAssetEntity>>.That.IsSameSequenceAs(found1, found2), requestContext))
                .Returns(new List<IEnrichedAssetEntity> { enriched1, enriched2 });

            var result = await sut.QueryAsync(requestContext, null, Q.Empty.WithIds(ids));

            Assert.Equal(8, result.Total);

            Assert.Equal(new[] { enriched1, enriched2 }, result.ToArray());
        }

        [Fact]
        public async Task Should_load_assets_with_query_and_resolve_tags()
        {
            var found1 = new AssetEntity { Id = DomainId.NewGuid() };
            var found2 = new AssetEntity { Id = DomainId.NewGuid() };

            var enriched1 = new AssetEntity();
            var enriched2 = new AssetEntity();

            var parentId = DomainId.NewGuid();

            A.CallTo(() => assetRepository.QueryAsync(appId.Id, parentId, A<ClrQuery>._))
                .Returns(ResultList.CreateFrom(8, found1, found2));

            A.CallTo(() => assetEnricher.EnrichAsync(A<IEnumerable<IAssetEntity>>.That.IsSameSequenceAs(found1, found2), requestContext))
                .Returns(new List<IEnrichedAssetEntity> { enriched1, enriched2 });

            var result = await sut.QueryAsync(requestContext, parentId, Q.Empty);

            Assert.Equal(8, result.Total);

            Assert.Equal(new[] { enriched1, enriched2 }, result.ToArray());
        }

        [Fact]
        public async Task Should_load_assets_folders_from_repository()
        {
            var parentId = DomainId.NewGuid();

            var assetFolders = ResultList.CreateFrom<IAssetFolderEntity>(10);

            A.CallTo(() => assetFolderRepository.QueryAsync(appId.Id, parentId))
                .Returns(assetFolders);

            var result = await sut.QueryAssetFoldersAsync(requestContext, parentId);

            Assert.Same(assetFolders, result);
        }

        [Fact]
        public async Task Should_resolve_folder_path_from_root()
        {
            var folderId1 = DomainId.NewGuid();
            var folder1 = CreateFolder(folderId1);

            A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(appId.Id, folderId1))
                .Returns(folder1);

            var result = await sut.FindAssetFolderAsync(appId.Id, folderId1);

            Assert.Equal(result, new[] { folder1 });
        }

        [Fact]
        public async Task Should_resolve_folder_path_from_child()
        {
            var folderId1 = DomainId.NewGuid();
            var folderId2 = DomainId.NewGuid();
            var folderId3 = DomainId.NewGuid();

            var folder1 = CreateFolder(folderId1);
            var folder2 = CreateFolder(folderId2, folderId1);
            var folder3 = CreateFolder(folderId3, folderId2);

            A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(appId.Id, folderId1))
                .Returns(folder1);

            A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(appId.Id, folderId2))
                .Returns(folder2);

            A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(appId.Id, folderId3))
                .Returns(folder3);

            var result = await sut.FindAssetFolderAsync(appId.Id, folderId3);

            Assert.Equal(result, new[] { folder1, folder2, folder3 });
        }

        [Fact]
        public async Task Should_not_resolve_folder_path_if_root_not_found()
        {
            var folderId1 = DomainId.NewGuid();

            A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(appId.Id, folderId1))
                .Returns(Task.FromResult<IAssetFolderEntity?>(null));

            var result = await sut.FindAssetFolderAsync(appId.Id, folderId1);

            Assert.Empty(result);
        }

        [Fact]
        public async Task Should_not_resolve_folder_path_if_parent_of_child_not_found()
        {
            var folderId1 = DomainId.NewGuid();
            var folderId2 = DomainId.NewGuid();

            var folder1 = CreateFolder(folderId1);
            var folder2 = CreateFolder(folderId2, folderId1);

            A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(appId.Id, folderId1))
                .Returns(Task.FromResult<IAssetFolderEntity?>(null));

            A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(appId.Id, folderId2))
                .Returns(folder2);

            var result = await sut.FindAssetFolderAsync(appId.Id, folderId2);

            Assert.Empty(result);
        }

        [Fact]
        public async Task Should_not_resolve_folder_path_if_recursion_detected()
        {
            var folderId1 = DomainId.NewGuid();
            var folderId2 = DomainId.NewGuid();

            var folder1 = CreateFolder(folderId1, folderId2);
            var folder2 = CreateFolder(folderId2, folderId1);

            A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(appId.Id, folderId1))
                .Returns(Task.FromResult<IAssetFolderEntity?>(null));

            A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(appId.Id, folderId2))
                .Returns(folder2);

            var result = await sut.FindAssetFolderAsync(appId.Id, folderId2);

            Assert.Empty(result);
        }

        private static IAssetFolderEntity CreateFolder(DomainId id, DomainId parentId = default)
        {
            var assetFolder = A.Fake<IAssetFolderEntity>();

            A.CallTo(() => assetFolder.Id).Returns(id);
            A.CallTo(() => assetFolder.ParentId).Returns(parentId);

            return assetFolder;
        }
    }
}