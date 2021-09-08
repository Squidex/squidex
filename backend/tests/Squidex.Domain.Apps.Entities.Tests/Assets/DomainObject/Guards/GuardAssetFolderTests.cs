// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject.Guards
{
    public class GuardAssetFolderTests : IClassFixture<TranslationsFixture>
    {
        private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
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

            A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, parentId, A<CancellationToken>._))
                .Returns(new List<IAssetFolderEntity>());

            await ValidationAssert.ThrowsAsync(() => operation.MustMoveToValidFolder(parentId),
                new ValidationError("Asset folder does not exist.", "ParentId"));
        }

        [Fact]
        public async Task Should_not_throw_exception_if_moving_to_valid_folder()
        {
            var parentId = DomainId.NewGuid();

            var operation = Operation(CreateAssetFolder());

            A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, parentId, A<CancellationToken>._))
                .Returns(new List<IAssetFolderEntity> { CreateAssetFolder() });

            await operation.MustMoveToValidFolder(parentId);
        }

        [Fact]
        public async Task Should_not_throw_exception_if_moving_to_same_folder()
        {
            var parentId = DomainId.NewGuid();

            var operation = Operation(CreateAssetFolder(default, parentId));

            await operation.MustMoveToValidFolder(parentId);

            A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, parentId, default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_throw_exception_if_moving_to_root()
        {
            var parentId = DomainId.Empty;

            var operation = Operation(CreateAssetFolder());

            await operation.MustMoveToValidFolder(parentId);

            A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, A<DomainId>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_throw_exception_if_moving_its_own_child()
        {
            var parentId = DomainId.NewGuid();

            var operation = Operation(CreateAssetFolder());

            A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, parentId, A<CancellationToken>._))
                .Returns(new List<IAssetFolderEntity>
                {
                    CreateAssetFolder(operation.CommandId),
                    CreateAssetFolder(parentId, operation.CommandId)
                });

            await ValidationAssert.ThrowsAsync(() => operation.MustMoveToValidFolder(parentId),
                new ValidationError("Cannot add folder to its own child.", "ParentId"));
        }

        private AssetFolderOperation Operation(IAssetFolderEntity assetFolder)
        {
            return Operation(assetFolder, Mocks.FrontendUser());
        }

        private AssetFolderOperation Operation(IAssetFolderEntity assetFolder, ClaimsPrincipal? currentUser)
        {
            var serviceProvider =
                new ServiceCollection()
                    .AddSingleton(assetQuery)
                    .BuildServiceProvider();

            return new AssetFolderOperation(serviceProvider, () => assetFolder)
            {
                App = Mocks.App(appId),
                CommandId = assetFolder.Id,
                Command = new CreateAssetFolder { User = currentUser, Actor = actor }
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
}
