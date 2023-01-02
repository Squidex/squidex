// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject.Guards;

public class GuardAssetTests : GivenContext, IClassFixture<TranslationsFixture>
{
    private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
    private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();

    [Fact]
    public async Task Should_throw_exception_if_moving_to_invalid_folder()
    {
        var parentId = DomainId.NewGuid();

        var operation = Operation(CreateAsset());

        A.CallTo(() => assetQuery.FindAssetFolderAsync(AppId.Id, parentId, CancellationToken))
            .Returns(new List<IAssetFolderEntity>());

        await ValidationAssert.ThrowsAsync(() => operation.MustMoveToValidFolder(parentId, CancellationToken),
            new ValidationError("Asset folder does not exist.", "ParentId"));
    }

    [Fact]
    public async Task Should_not_throw_exception_if_moving_to_valid_folder()
    {
        var parentId = DomainId.NewGuid();

        var operation = Operation(CreateAsset());

        A.CallTo(() => assetQuery.FindAssetFolderAsync(AppId.Id, parentId, CancellationToken))
            .Returns(new List<IAssetFolderEntity> { CreateAssetFolder() });

        await operation.MustMoveToValidFolder(parentId, CancellationToken);
    }

    [Fact]
    public async Task Should_not_throw_exception_if_moving_to_same_folder()
    {
        var parentId = DomainId.NewGuid();

        var operation = Operation(CreateAsset(default, parentId));

        await operation.MustMoveToValidFolder(parentId, CancellationToken);

        A.CallTo(() => assetQuery.FindAssetFolderAsync(AppId.Id, parentId, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_throw_exception_if_moving_to_root()
    {
        var parentId = DomainId.Empty;

        var operation = Operation(CreateAsset(parentId));

        await operation.MustMoveToValidFolder(parentId, CancellationToken);

        A.CallTo(() => assetQuery.FindAssetFolderAsync(AppId.Id, parentId, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_throw_exception_if_referenced()
    {
        var operation = Operation(CreateAsset());

        A.CallTo(() => contentRepository.HasReferrersAsync(AppId.Id, operation.CommandId, SearchScope.All, CancellationToken))
            .Returns(true);

        await Assert.ThrowsAsync<DomainException>(() => operation.CheckReferrersAsync(CancellationToken));
    }

    [Fact]
    public async Task Should_not_throw_exception_if_not_referenced()
    {
        var operation = Operation(CreateAsset());

        A.CallTo(() => contentRepository.HasReferrersAsync(AppId.Id, operation.CommandId, SearchScope.All, CancellationToken))
            .Returns(true);

        await Assert.ThrowsAsync<DomainException>(() => operation.CheckReferrersAsync(CancellationToken));
    }

    private AssetOperation Operation(AssetEntity asset)
    {
        return Operation(asset, Mocks.FrontendUser());
    }

    private AssetOperation Operation(AssetEntity asset, ClaimsPrincipal? currentUser)
    {
        var serviceProvider =
            new ServiceCollection()
                .AddSingleton(contentRepository)
                .AddSingleton(assetQuery)
                .BuildServiceProvider();

        return new AssetOperation(serviceProvider, () => asset)
        {
            App = App,
            CommandId = asset.Id,
            Command = new CreateAsset { User = currentUser, Actor = User }
        };
    }

    private AssetEntity CreateAsset(DomainId id = default, DomainId parentId = default)
    {
        return new AssetEntity
        {
            Id = OrNew(id),
            AppId = AppId,
            Created = default,
            CreatedBy = User,
            ParentId = OrNew(parentId)
        };
    }

    private IAssetFolderEntity CreateAssetFolder(DomainId id = default, DomainId parentId = default)
    {
        var assetFolder = A.Fake<IAssetFolderEntity>();

        A.CallTo(() => assetFolder.Id)
            .Returns(OrNew(id));
        A.CallTo(() => assetFolder.AppId)
            .Returns(AppId);
        A.CallTo(() => assetFolder.ParentId)
            .Returns(OrNew(parentId));

        return assetFolder;
    }

    private static DomainId OrNew(DomainId parentId)
    {
        if (parentId == default)
        {
            parentId = DomainId.NewGuid();
        }

        return parentId;
    }
}
