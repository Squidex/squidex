// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject.Guards;

public class GuardAssetFolderTests : GivenContext, IClassFixture<TranslationsFixture>
{
    private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
    private readonly RefToken actor = RefToken.User("123");

    [Fact]
    public void Should_throw_exception_if_folder_name_not_defined()
    {
        var operation = Operation(CreateAssetFolder());

        ValidationAssert.Throws(() => operation.MustHaveName(null!),
            new ValidationError("Folder name is required.", "FolderName"));
    }

    [Fact]
    public void Should_not_throw_exception_if_folder_name_defined()
    {
        var operation = Operation(CreateAssetFolder());

        operation.MustHaveName("Folder");
    }

    [Fact]
    public async Task Should_throw_exception_if_moving_to_invalid_folder()
    {
        var parentId = DomainId.NewGuid();

        var operation = Operation(CreateAssetFolder());

        A.CallTo(() => assetQuery.FindAssetFolderAsync(AppId.Id, parentId, CancellationToken))
            .Returns(new List<AssetFolder>());

        await ValidationAssert.ThrowsAsync(() => operation.MustMoveToValidFolder(parentId, CancellationToken),
            new ValidationError("Asset folder does not exist.", "ParentId"));
    }

    [Fact]
    public async Task Should_not_throw_exception_if_moving_to_valid_folder()
    {
        var parentId = DomainId.NewGuid();

        var operation = Operation(CreateAssetFolder());

        A.CallTo(() => assetQuery.FindAssetFolderAsync(AppId.Id, parentId, CancellationToken))
            .Returns(new List<AssetFolder> { CreateAssetFolder() });

        await operation.MustMoveToValidFolder(parentId, CancellationToken);
    }

    [Fact]
    public async Task Should_not_throw_exception_if_moving_to_same_folder()
    {
        var parentId = DomainId.NewGuid();

        var operation = Operation(CreateAssetFolder() with { ParentId = parentId });

        await operation.MustMoveToValidFolder(parentId, CancellationToken);

        A.CallTo(() => assetQuery.FindAssetFolderAsync(AppId.Id, parentId, CancellationToken))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_throw_exception_if_moving_to_root()
    {
        var parentId = DomainId.Empty;

        var operation = Operation(CreateAssetFolder());

        await operation.MustMoveToValidFolder(parentId, CancellationToken);

        A.CallTo(() => assetQuery.FindAssetFolderAsync(AppId.Id, A<DomainId>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_throw_exception_if_moving_its_own_child()
    {
        var parentId = DomainId.NewGuid();

        var operation = Operation(CreateAssetFolder());

        A.CallTo(() => assetQuery.FindAssetFolderAsync(AppId.Id, parentId, CancellationToken))
            .Returns(new List<AssetFolder>
            {
                CreateAssetFolder().WithId(operation.CommandId),
                CreateAssetFolder().WithId(parentId) with { ParentId = operation.CommandId }
            });

        await ValidationAssert.ThrowsAsync(() => operation.MustMoveToValidFolder(parentId, CancellationToken),
            new ValidationError("Cannot add folder to its own child.", "ParentId"));
    }

    private AssetFolderOperation Operation(AssetFolder assetFolder)
    {
        return Operation(assetFolder, Mocks.FrontendUser());
    }

    private AssetFolderOperation Operation(AssetFolder assetFolder, ClaimsPrincipal? currentUser)
    {
        var serviceProvider =
            new ServiceCollection()
                .AddSingleton(assetQuery)
                .BuildServiceProvider();

        return new AssetFolderOperation(serviceProvider, () => assetFolder)
        {
            App = App,
            CommandId = assetFolder.Id,
            Command = new CreateAssetFolder { User = currentUser, Actor = actor }
        };
    }
}
