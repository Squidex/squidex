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

public class GuardAssetTests : IClassFixture<TranslationsFixture>
{
    private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
    private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly RefToken actor = RefToken.User("123");

    [Fact]
    public async Task Should_throw_exception_if_moving_to_invalid_folder()
    {
        var parentId = DomainId.NewGuid();

        var operation = Operation(CreateAsset());

        A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, parentId, default))
            .Returns(new List<IAssetFolderEntity>());

        await ValidationAssert.ThrowsAsync(() => operation.MustMoveToValidFolder(parentId),
            new ValidationError("Asset folder does not exist.", "ParentId"));
    }

    [Fact]
    public async Task Should_not_throw_exception_if_moving_to_valid_folder()
    {
        var parentId = DomainId.NewGuid();

        var operation = Operation(CreateAsset());

        A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, parentId, default))
            .Returns(new List<IAssetFolderEntity> { CreateAssetFolder() });

        await operation.MustMoveToValidFolder(parentId);
    }

    [Fact]
    public async Task Should_not_throw_exception_if_moving_to_same_folder()
    {
        var parentId = DomainId.NewGuid();

        var operation = Operation(CreateAsset(default, parentId));

        await operation.MustMoveToValidFolder(parentId);

        A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, parentId, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_throw_exception_if_moving_to_root()
    {
        var parentId = DomainId.Empty;

        var operation = Operation(CreateAsset(parentId));

        await operation.MustMoveToValidFolder(parentId);

        A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, parentId, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_throw_exception_if_referenced()
    {
        var operation = Operation(CreateAsset());

        A.CallTo(() => contentRepository.HasReferrersAsync(appId.Id, operation.CommandId, SearchScope.All, default))
            .Returns(true);

        await Assert.ThrowsAsync<DomainException>(() => operation.CheckReferrersAsync());
    }

    [Fact]
    public async Task Should_not_throw_exception_if_not_referenced()
    {
        var operation = Operation(CreateAsset());

        A.CallTo(() => contentRepository.HasReferrersAsync(appId.Id, operation.CommandId, SearchScope.All, default))
            .Returns(true);

        await Assert.ThrowsAsync<DomainException>(() => operation.CheckReferrersAsync());
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
            App = Mocks.App(appId),
            CommandId = asset.Id,
            Command = new CreateAsset { User = currentUser, Actor = actor }
        };
    }

    private AssetEntity CreateAsset(DomainId id = default, DomainId parentId = default)
    {
        return new AssetEntity
        {
            Id = OrNew(id),
            AppId = appId,
            Created = default,
            CreatedBy = actor,
            ParentId = OrNew(parentId)
        };
    }

    private IAssetFolderEntity CreateAssetFolder(DomainId id = default, DomainId parentId = default)
    {
        var assetFolder = A.Fake<IAssetFolderEntity>();

        A.CallTo(() => assetFolder.Id)
            .Returns(OrNew(id));
        A.CallTo(() => assetFolder.AppId)
            .Returns(appId);
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
