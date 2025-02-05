// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

#pragma warning disable xUnit1044 // Avoid using TheoryData type arguments that are not serializable
#pragma warning disable MA0040 // Forward the CancellationToken parameter to methods that take one

namespace Squidex.Shared;

public abstract class AssetFolderRepositoryTests : GivenContext
{
    private const int NumValues = 250;
    private static readonly DomainId ParentId = DomainId.Create("3b5ba909-e5a5-4858-9d0d-df4ff922d451");
    private static readonly NamedId<DomainId>[] AppIds =
    [
        NamedId.Of(DomainId.Create("3b5ba909-e5a5-4858-9d0d-df4ff922d452"), "my-app1"),
        NamedId.Of(DomainId.Create("3b5ba909-e5a5-4858-9d0d-df4ff922d453"), "my-app1"),
    ];

    private readonly DomainId appId;

    protected AssetFolderRepositoryTests()
    {
        appId = AppIds[Random.Shared.Next(AppIds.Length)].Id;
    }

    protected abstract Task<IAssetFolderRepository> CreateSutAsync();

    protected async Task<IAssetFolderRepository> CreateAndPrepareSutAsync()
    {
        var sut = await CreateSutAsync();

        if ((await sut.QueryAsync(AppIds[0].Id, null)).Count > 0)
        {
            return sut;
        }

        if (sut is not ISnapshotStore<AssetFolder> store)
        {
            return sut;
        }

        var batch = new List<SnapshotWriteJob<AssetFolder>>();

        async Task ExecuteBatchAsync(AssetFolder? entity)
        {
            if (entity != null)
            {
                batch.Add(new SnapshotWriteJob<AssetFolder>(entity.UniqueId, entity, 0));
            }

            if ((entity == null || batch.Count >= 1000) && batch.Count > 0)
            {
                await store.WriteManyAsync(batch);
                batch.Clear();
            }
        }

        foreach (var forAppId in AppIds)
        {
            for (var i = 0; i < NumValues; i++)
            {
                var fileName = i.ToString(CultureInfo.InvariantCulture);

                var assetFolder = CreateAssetFolder() with
                {
                    AppId = forAppId,
                    FolderName = fileName,
                };

                await ExecuteBatchAsync(assetFolder);
            }

            var byParent = CreateAssetFolder() with
            {
                AppId = forAppId,
                ParentId = ParentId,
                FolderName = "0",
            };

            await ExecuteBatchAsync(byParent);
        }

        await ExecuteBatchAsync(null);
        return sut;
    }

    public static readonly TheoryData<DomainId?> ParentIds = new TheoryData<DomainId?>
    {
        { null },
        { DomainId.Empty },
    };

    [Fact]
    public async Task Should_find_asset_folder_by_id()
    {
        var sut = await CreateAndPrepareSutAsync();

        var assetFolder1 = (await sut.QueryAsync(appId, null))[0];
        var assetFolder2 = await sut.FindAssetFolderAsync(appId, assetFolder1.Id);

        // The Slug is random here, as it does not really matter.
        Assert.NotNull(assetFolder2);
    }

    [Fact]
    public async Task Should_query_child_ids()
    {
        var sut = await CreateAndPrepareSutAsync();

        var assets = await sut.QueryChildIdsAsync(appId, ParentId);

        // No pagination is going on here.
        Assert.Single(assets);
    }

    [Theory]
    [MemberData(nameof(ParentIds))]
    public async Task Should_query_assets(DomainId? parentId)
    {
        var sut = await CreateAndPrepareSutAsync();

        var assets = await sut.QueryAsync(appId, parentId);

        // Default page size is unlimited.
        Assert.True(assets.Count >= NumValues);
    }
}
