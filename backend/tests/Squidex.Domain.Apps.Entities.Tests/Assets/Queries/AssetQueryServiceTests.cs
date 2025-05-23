﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Assets.Queries;

public class AssetQueryServiceTests : GivenContext
{
    private readonly IAssetEnricher assetEnricher = A.Fake<IAssetEnricher>();
    private readonly IAssetRepository assetRepository = A.Fake<IAssetRepository>();
    private readonly IAssetLoader assetLoader = A.Fake<IAssetLoader>();
    private readonly IAssetFolderRepository assetFolderRepository = A.Fake<IAssetFolderRepository>();
    private readonly AssetQueryParser queryParser = A.Fake<AssetQueryParser>();
    private readonly AssetQueryService sut;

    public AssetQueryServiceTests()
    {
        SetupEnricher();

        A.CallTo(() => queryParser.ParseAsync(ApiContext, A<Q>._, CancellationToken))
            .ReturnsLazily(c => Task.FromResult(c.GetArgument<Q>(1)!));

        var options = Options.Create(new AssetOptions());

        sut = new AssetQueryService(
            assetEnricher,
            assetRepository,
            assetLoader,
            assetFolderRepository,
            options,
            queryParser);
    }

    [Fact]
    public async Task Should_find_asset_by_slug_and_enrich_it()
    {
        var asset = CreateAsset();

        A.CallTo(() => assetRepository.FindAssetBySlugAsync(AppId.Id, "slug", true, A<CancellationToken>._))
            .Returns(asset);

        var actual = await sut.FindBySlugAsync(ApiContext, "slug", true, CancellationToken);

        AssertAsset(asset, actual);
    }

    [Fact]
    public async Task Should_return_null_if_asset_by_slug_cannot_be_found()
    {
        var asset = CreateAsset();

        A.CallTo(() => assetRepository.FindAssetBySlugAsync(AppId.Id, "slug", false, A<CancellationToken>._))
            .Returns(Task.FromResult<Asset?>(null));

        var actual = await sut.FindBySlugAsync(ApiContext, "slug", false, CancellationToken);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_find_asset_by_id_and_enrich_it()
    {
        var asset = CreateAsset();

        A.CallTo(() => assetRepository.FindAssetAsync(AppId.Id, asset.Id, false, A<CancellationToken>._))
            .Returns(asset);

        var actual = await sut.FindAsync(ApiContext, asset.Id, ct: CancellationToken);

        AssertAsset(asset, actual);
    }

    [Fact]
    public async Task Should_return_null_if_asset_by_id_cannot_be_found()
    {
        var asset = CreateAsset();

        A.CallTo(() => assetRepository.FindAssetAsync(AppId.Id, asset.Id, false, A<CancellationToken>._))
            .Returns(Task.FromResult<Asset?>(null));

        var actual = await sut.FindAsync(ApiContext, asset.Id, ct: CancellationToken);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_find_asset_by_id_and_version_and_enrich_it()
    {
        var asset = CreateAsset();

        A.CallTo(() => assetLoader.GetAsync(AppId.Id, asset.Id, 2, A<CancellationToken>._))
            .Returns(asset);

        var actual = await sut.FindAsync(ApiContext, asset.Id, false, 2, CancellationToken);

        AssertAsset(asset, actual);
    }

    [Fact]
    public async Task Should_return_null_if_asset_by_id_and_version_cannot_be_found()
    {
        var asset = CreateAsset();

        A.CallTo(() => assetLoader.GetAsync(AppId.Id, asset.Id, 2, A<CancellationToken>._))
            .Returns(Task.FromResult<Asset?>(null));

        var actual = await sut.FindAsync(ApiContext, asset.Id, false, 2, CancellationToken);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_find_global_asset_by_id_and_enrich_it()
    {
        var asset = CreateAsset();

        A.CallTo(() => assetRepository.FindAssetAsync(asset.Id, A<CancellationToken>._))
            .Returns(asset);

        var actual = await sut.FindGlobalAsync(ApiContext, asset.Id, CancellationToken);

        AssertAsset(asset, actual);
    }

    [Fact]
    public async Task Should_return_null_if_global_asset_by_id_cannot_be_found()
    {
        var asset = CreateAsset();

        A.CallTo(() => assetRepository.FindAssetAsync(asset.Id, A<CancellationToken>._))
            .Returns(Task.FromResult<Asset?>(null));

        var actual = await sut.FindGlobalAsync(ApiContext, asset.Id, CancellationToken);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_find_assets_by_hash_and_and_enrich_it()
    {
        var asset = CreateAsset();

        A.CallTo(() => assetRepository.FindAssetByHashAsync(AppId.Id, "hash", "name", 123, A<CancellationToken>._))
            .Returns(asset);

        var actual = await sut.FindByHashAsync(ApiContext, "hash", "name", 123, CancellationToken);

        AssertAsset(asset, actual);
    }

    [Fact]
    public async Task Should_return_null_if_asset_by_hash_cannot_be_found()
    {
        var asset = CreateAsset();

        A.CallTo(() => assetRepository.FindAssetByHashAsync(AppId.Id, "hash", "name", 123, A<CancellationToken>._))
            .Returns(Task.FromResult<Asset?>(null));

        var actual = await sut.FindByHashAsync(ApiContext, "hash", "name", 123, CancellationToken);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_query_assets_and_enrich_it()
    {
        var asset1 = CreateAsset();
        var asset2 = CreateAsset();

        var parentId = DomainId.NewGuid();

        var q = Q.Empty.WithODataQuery("fileName eq 'Name'");

        A.CallTo(() => assetRepository.QueryAsync(AppId.Id, parentId, q, A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(8, asset1, asset2));

        var actual = await sut.QueryAsync(ApiContext, parentId, q, CancellationToken);

        Assert.Equal(8, actual.Total);

        AssertAsset(asset1, actual[0]);
        AssertAsset(asset2, actual[1]);
    }

    [Fact]
    public async Task Should_query_asset_folders()
    {
        var parentId = DomainId.NewGuid();

        var assetFolders = ResultList.CreateFrom<AssetFolder>(10);

        A.CallTo(() => assetFolderRepository.QueryAsync(AppId.Id, parentId, A<CancellationToken>._))
            .Returns(assetFolders);

        var actual = await sut.QueryAssetFoldersAsync(ApiContext, parentId, CancellationToken);

        Assert.Same(assetFolders, actual);
    }

    [Fact]
    public async Task Should_find_asset_folder_with_path()
    {
        var folder1 = CreateAssetFolder();

        A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(AppId.Id, folder1.Id, A<CancellationToken>._))
            .Returns(folder1);

        var actual = await sut.FindAssetFolderAsync(AppId.Id, folder1.Id, CancellationToken);

        Assert.Equal(actual, [folder1]);
    }

    [Fact]
    public async Task Should_resolve_folder_path_from_child()
    {
        var folder1 = CreateAssetFolder();
        var folder2 = CreateAssetFolder() with { ParentId = folder1.Id };
        var folder3 = CreateAssetFolder() with { ParentId = folder2.Id };

        A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(AppId.Id, folder1.Id, A<CancellationToken>._))
            .Returns(folder1);

        A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(AppId.Id, folder2.Id, A<CancellationToken>._))
            .Returns(folder2);

        A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(AppId.Id, folder3.Id, A<CancellationToken>._))
            .Returns(folder3);

        var actual = await sut.FindAssetFolderAsync(AppId.Id, folder3.Id, CancellationToken);

        Assert.Equal(actual, [folder1, folder2, folder3]);
    }

    [Fact]
    public async Task Should_not_resolve_folder_path_if_root_not_found()
    {
        var folderId1 = DomainId.NewGuid();

        A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(AppId.Id, folderId1, A<CancellationToken>._))
            .Returns(Task.FromResult<AssetFolder?>(null));

        var actual = await sut.FindAssetFolderAsync(AppId.Id, folderId1, CancellationToken);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_not_resolve_folder_path_if_parent_of_child_not_found()
    {
        var folder1 = CreateAssetFolder();
        var folder2 = CreateAssetFolder() with { ParentId = folder1.Id };

        A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(AppId.Id, folder1.Id, A<CancellationToken>._))
            .Returns(Task.FromResult<AssetFolder?>(null));

        A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(AppId.Id, folder2.Id, A<CancellationToken>._))
            .Returns(folder2);

        var actual = await sut.FindAssetFolderAsync(AppId.Id, folder2.Id, CancellationToken);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_not_resolve_folder_path_if_recursion_detected()
    {
        var folderId1 = DomainId.NewGuid();
        var folderId2 = DomainId.NewGuid();

        var folder1 = CreateAssetFolder().WithId(folderId1) with { ParentId = folderId2 };
        var folder2 = CreateAssetFolder().WithId(folderId2) with { ParentId = folderId1 };

        A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(AppId.Id, folderId1, A<CancellationToken>._))
            .Returns(Task.FromResult<AssetFolder?>(null));

        A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(AppId.Id, folderId2, A<CancellationToken>._))
            .Returns(folder2);

        var actual = await sut.FindAssetFolderAsync(AppId.Id, folderId2, CancellationToken);

        Assert.Empty(actual);
    }

    private static void AssertAsset(Asset source, EnrichedAsset? actual)
    {
        Assert.NotNull(actual);
        Assert.NotSame(source, actual);
        Assert.Equal(source.Id, actual?.Id);
    }

    private void SetupEnricher()
    {
        A.CallTo(() => assetEnricher.EnrichAsync(A<IEnumerable<Asset>>._, A<Context>._, CancellationToken))
            .ReturnsLazily(x =>
            {
                var input = x.GetArgument<IEnumerable<Asset>>(0)!;

                return Task.FromResult<IReadOnlyList<EnrichedAsset>>(input.Select(c => SimpleMapper.Map(c, new EnrichedAsset())).ToList());
            });
    }
}
