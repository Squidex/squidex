// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Assets.Queries;

public class AssetQueryServiceTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly IAssetEnricher assetEnricher = A.Fake<IAssetEnricher>();
    private readonly IAssetRepository assetRepository = A.Fake<IAssetRepository>();
    private readonly IAssetLoader assetLoader = A.Fake<IAssetLoader>();
    private readonly IAssetFolderRepository assetFolderRepository = A.Fake<IAssetFolderRepository>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly Context requestContext;
    private readonly AssetQueryParser queryParser = A.Fake<AssetQueryParser>();
    private readonly AssetQueryService sut;

    public AssetQueryServiceTests()
    {
        ct = cts.Token;

        requestContext = new Context(Mocks.FrontendUser(), Mocks.App(appId));

        SetupEnricher();

        A.CallTo(() => queryParser.ParseAsync(requestContext, A<Q>._, ct))
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
        var asset = CreateAsset(DomainId.NewGuid());

        A.CallTo(() => assetRepository.FindAssetBySlugAsync(appId.Id, "slug", A<CancellationToken>._))
            .Returns(asset);

        var actual = await sut.FindBySlugAsync(requestContext, "slug", ct);

        AssertAsset(asset, actual);
    }

    [Fact]
    public async Task Should_return_null_if_asset_by_slug_cannot_be_found()
    {
        var asset = CreateAsset(DomainId.NewGuid());

        A.CallTo(() => assetRepository.FindAssetBySlugAsync(appId.Id, "slug", A<CancellationToken>._))
            .Returns(Task.FromResult<IAssetEntity?>(null));

        var actual = await sut.FindBySlugAsync(requestContext, "slug", ct);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_find_asset_by_id_and_enrich_it()
    {
        var asset = CreateAsset(DomainId.NewGuid());

        A.CallTo(() => assetRepository.FindAssetAsync(appId.Id, asset.Id, A<CancellationToken>._))
            .Returns(asset);

        var actual = await sut.FindAsync(requestContext, asset.Id, ct: ct);

        AssertAsset(asset, actual);
    }

    [Fact]
    public async Task Should_return_null_if_asset_by_id_cannot_be_found()
    {
        var asset = CreateAsset(DomainId.NewGuid());

        A.CallTo(() => assetRepository.FindAssetAsync(appId.Id, asset.Id, A<CancellationToken>._))
            .Returns(Task.FromResult<IAssetEntity?>(null));

        var actual = await sut.FindAsync(requestContext, asset.Id, ct: ct);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_find_asset_by_id_and_version_and_enrich_it()
    {
        var asset = CreateAsset(DomainId.NewGuid());

        A.CallTo(() => assetLoader.GetAsync(appId.Id, asset.Id, 2, A<CancellationToken>._))
            .Returns(asset);

        var actual = await sut.FindAsync(requestContext, asset.Id, 2, ct);

        AssertAsset(asset, actual);
    }

    [Fact]
    public async Task Should_return_null_if_asset_by_id_and_version_cannot_be_found()
    {
        var asset = CreateAsset(DomainId.NewGuid());

        A.CallTo(() => assetLoader.GetAsync(appId.Id, asset.Id, 2, A<CancellationToken>._))
            .Returns(Task.FromResult<IAssetEntity?>(null));

        var actual = await sut.FindAsync(requestContext, asset.Id, 2, ct);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_find_global_asset_by_id_and_enrich_it()
    {
        var asset = CreateAsset(DomainId.NewGuid());

        A.CallTo(() => assetRepository.FindAssetAsync(asset.Id, A<CancellationToken>._))
            .Returns(asset);

        var actual = await sut.FindGlobalAsync(requestContext, asset.Id, ct);

        AssertAsset(asset, actual);
    }

    [Fact]
    public async Task Should_return_null_if_global_asset_by_id_cannot_be_found()
    {
        var asset = CreateAsset(DomainId.NewGuid());

        A.CallTo(() => assetRepository.FindAssetAsync(asset.Id, A<CancellationToken>._))
            .Returns(Task.FromResult<IAssetEntity?>(null));

        var actual = await sut.FindGlobalAsync(requestContext, asset.Id, ct);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_find_assets_by_hash_and_and_enrich_it()
    {
        var asset = CreateAsset(DomainId.NewGuid());

        A.CallTo(() => assetRepository.FindAssetByHashAsync(appId.Id, "hash", "name", 123, A<CancellationToken>._))
            .Returns(asset);

        var actual = await sut.FindByHashAsync(requestContext, "hash", "name", 123, ct);

        AssertAsset(asset, actual);
    }

    [Fact]
    public async Task Should_return_null_if_asset_by_hash_cannot_be_found()
    {
        var asset = CreateAsset(DomainId.NewGuid());

        A.CallTo(() => assetRepository.FindAssetByHashAsync(appId.Id, "hash", "name", 123, A<CancellationToken>._))
            .Returns(Task.FromResult<IAssetEntity?>(null));

        var actual = await sut.FindByHashAsync(requestContext, "hash", "name", 123, ct);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_query_assets_and_enrich_it()
    {
        var asset1 = CreateAsset(DomainId.NewGuid());
        var asset2 = CreateAsset(DomainId.NewGuid());

        var parentId = DomainId.NewGuid();

        var q = Q.Empty.WithODataQuery("fileName eq 'Name'");

        A.CallTo(() => assetRepository.QueryAsync(appId.Id, parentId, q, A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(8, asset1, asset2));

        var actual = await sut.QueryAsync(requestContext, parentId, q, ct);

        Assert.Equal(8, actual.Total);

        AssertAsset(asset1, actual[0]);
        AssertAsset(asset2, actual[1]);
    }

    [Fact]
    public async Task Should_query_asset_folders()
    {
        var parentId = DomainId.NewGuid();

        var assetFolders = ResultList.CreateFrom<IAssetFolderEntity>(10);

        A.CallTo(() => assetFolderRepository.QueryAsync(appId.Id, parentId, A<CancellationToken>._))
            .Returns(assetFolders);

        var actual = await sut.QueryAssetFoldersAsync(requestContext, parentId, ct);

        Assert.Same(assetFolders, actual);
    }

    [Fact]
    public async Task Should_query_asset_folders_with_appId()
    {
        var parentId = DomainId.NewGuid();

        var assetFolders = ResultList.CreateFrom<IAssetFolderEntity>(10);

        A.CallTo(() => assetFolderRepository.QueryAsync(appId.Id, parentId, A<CancellationToken>._))
            .Returns(assetFolders);

        var actual = await sut.QueryAssetFoldersAsync(appId.Id, parentId, ct);

        Assert.Same(assetFolders, actual);
    }

    [Fact]
    public async Task Should_find_asset_folder_with_path()
    {
        var folderId1 = DomainId.NewGuid();
        var folder1 = CreateFolder(folderId1);

        A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(appId.Id, folderId1, A<CancellationToken>._))
            .Returns(folder1);

        var actual = await sut.FindAssetFolderAsync(appId.Id, folderId1, ct);

        Assert.Equal(actual, new[] { folder1 });
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

        A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(appId.Id, folderId1, A<CancellationToken>._))
            .Returns(folder1);

        A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(appId.Id, folderId2, A<CancellationToken>._))
            .Returns(folder2);

        A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(appId.Id, folderId3, A<CancellationToken>._))
            .Returns(folder3);

        var actual = await sut.FindAssetFolderAsync(appId.Id, folderId3, ct);

        Assert.Equal(actual, new[] { folder1, folder2, folder3 });
    }

    [Fact]
    public async Task Should_not_resolve_folder_path_if_root_not_found()
    {
        var folderId1 = DomainId.NewGuid();

        A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(appId.Id, folderId1, A<CancellationToken>._))
            .Returns(Task.FromResult<IAssetFolderEntity?>(null));

        var actual = await sut.FindAssetFolderAsync(appId.Id, folderId1, ct);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_not_resolve_folder_path_if_parent_of_child_not_found()
    {
        var folderId1 = DomainId.NewGuid();
        var folderId2 = DomainId.NewGuid();

        var folder1 = CreateFolder(folderId1);
        var folder2 = CreateFolder(folderId2, folderId1);

        A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(appId.Id, folderId1, A<CancellationToken>._))
            .Returns(Task.FromResult<IAssetFolderEntity?>(null));

        A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(appId.Id, folderId2, A<CancellationToken>._))
            .Returns(folder2);

        var actual = await sut.FindAssetFolderAsync(appId.Id, folderId2, ct);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_not_resolve_folder_path_if_recursion_detected()
    {
        var folderId1 = DomainId.NewGuid();
        var folderId2 = DomainId.NewGuid();

        var folder1 = CreateFolder(folderId1, folderId2);
        var folder2 = CreateFolder(folderId2, folderId1);

        A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(appId.Id, folderId1, A<CancellationToken>._))
            .Returns(Task.FromResult<IAssetFolderEntity?>(null));

        A.CallTo(() => assetFolderRepository.FindAssetFolderAsync(appId.Id, folderId2, A<CancellationToken>._))
            .Returns(folder2);

        var actual = await sut.FindAssetFolderAsync(appId.Id, folderId2, ct);

        Assert.Empty(actual);
    }

    private static void AssertAsset(IAssetEntity source, IEnrichedAssetEntity? actual)
    {
        Assert.NotNull(actual);
        Assert.NotSame(source, actual);
        Assert.Equal(source.AssetId, actual?.AssetId);
    }

    private static IAssetFolderEntity CreateFolder(DomainId id, DomainId parentId = default)
    {
        var assetFolder = A.Fake<IAssetFolderEntity>();

        A.CallTo(() => assetFolder.Id).Returns(id);
        A.CallTo(() => assetFolder.ParentId).Returns(parentId);

        return assetFolder;
    }

    private static AssetEntity CreateAsset(DomainId id)
    {
        return new AssetEntity { Id = id };
    }

    private void SetupEnricher()
    {
        A.CallTo(() => assetEnricher.EnrichAsync(A<IEnumerable<IAssetEntity>>._, A<Context>._, ct))
            .ReturnsLazily(x =>
            {
                var input = x.GetArgument<IEnumerable<IAssetEntity>>(0)!;

                return Task.FromResult<IReadOnlyList<IEnrichedAssetEntity>>(input.Select(c => SimpleMapper.Map(c, new AssetEntity())).ToList());
            });
    }
}
