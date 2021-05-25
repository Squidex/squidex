// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
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

        [Fact]
        public async Task CanCreate_should_throw_exception_if_folder_name_not_defined()
        {
            var command = new CreateAssetFolder { AppId = appId };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, command.ParentId, A<CancellationToken>._))
                .Returns(new List<IAssetFolderEntity>());

            await ValidationAssert.ThrowsAsync(() => GuardAssetFolder.CanCreate(command, assetQuery),
                new ValidationError("Folder name is required.", "FolderName"));
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_if_folder_not_found()
        {
            var command = new CreateAssetFolder { AppId = appId, FolderName = "My Folder", ParentId = DomainId.NewGuid() };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, command.ParentId, A<CancellationToken>._))
                .Returns(new List<IAssetFolderEntity>());

            await ValidationAssert.ThrowsAsync(() => GuardAssetFolder.CanCreate(command, assetQuery),
                new ValidationError("Asset folder does not exist.", "ParentId"));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_if_folder_found()
        {
            var command = new CreateAssetFolder { AppId = appId, FolderName = "My Folder", ParentId = DomainId.NewGuid() };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, command.ParentId, A<CancellationToken>._))
                .Returns(new List<IAssetFolderEntity> { AssetFolder() });

            await GuardAssetFolder.CanCreate(command, assetQuery);
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_if_added_to_root()
        {
            var command = new CreateAssetFolder { AppId = appId, FolderName = "My Folder" };

            await GuardAssetFolder.CanCreate(command, assetQuery);
        }

        [Fact]
        public async Task CanMove_should_throw_exception_if_adding_to_its_own_child()
        {
            var id = DomainId.NewGuid();

            var command = new MoveAssetFolder { AppId = appId, ParentId = DomainId.NewGuid() };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, command.ParentId, A<CancellationToken>._))
                .Returns(new List<IAssetFolderEntity>
                {
                    AssetFolder(id),
                    AssetFolder(command.ParentId)
                });

            await ValidationAssert.ThrowsAsync(() => GuardAssetFolder.CanMove(command, AssetFolder(id), assetQuery),
                new ValidationError("Cannot add folder to its own child.", "ParentId"));
        }

        [Fact]
        public async Task CanMove_should_throw_exception_if_folder_not_found()
        {
            var command = new MoveAssetFolder { AppId = appId, ParentId = DomainId.NewGuid() };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, command.ParentId, A<CancellationToken>._))
                .Returns(new List<IAssetFolderEntity>());

            await ValidationAssert.ThrowsAsync(() => GuardAssetFolder.CanMove(command, AssetFolder(), assetQuery),
                new ValidationError("Asset folder does not exist.", "ParentId"));
        }

        [Fact]
        public async Task CanMove_should_not_throw_exception_if_folder_found()
        {
            var command = new MoveAssetFolder { AppId = appId, ParentId = DomainId.NewGuid() };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, command.ParentId, A<CancellationToken>._))
                .Returns(new List<IAssetFolderEntity> { AssetFolder() });

            await GuardAssetFolder.CanMove(command, AssetFolder(), assetQuery);
        }

        [Fact]
        public async Task CanMove_should_not_throw_exception_if_folder_has_not_changed()
        {
            var command = new MoveAssetFolder { AppId = appId, ParentId = DomainId.NewGuid() };

            await GuardAssetFolder.CanMove(command, AssetFolder(parentId: command.ParentId), assetQuery);
        }

        [Fact]
        public async Task CanMove_should_not_throw_exception_if_added_to_root()
        {
            var command = new MoveAssetFolder { AppId = appId };

            await GuardAssetFolder.CanMove(command, AssetFolder(), assetQuery);
        }

        [Fact]
        public void CanRename_should_throw_exception_if_folder_name_is_empty()
        {
            var command = new RenameAssetFolder { AppId = appId };

            ValidationAssert.Throws(() => GuardAssetFolder.CanRename(command),
                new ValidationError("Folder name is required.", "FolderName"));
        }

        [Fact]
        public void CanRename_should_not_throw_exception_if_names_are_different()
        {
            var command = new RenameAssetFolder { AppId = appId, FolderName = "New Folder Name" };

            GuardAssetFolder.CanRename(command);
        }

        private IAssetFolderEntity AssetFolder(DomainId id = default, DomainId parentId = default)
        {
            var assetFolder = A.Fake<IAssetFolderEntity>();

            A.CallTo(() => assetFolder.Id)
                .Returns(id == default ? DomainId.NewGuid() : id);
            A.CallTo(() => assetFolder.AppId)
                .Returns(appId);
            A.CallTo(() => assetFolder.ParentId)
                .Returns(parentId == default ? DomainId.NewGuid() : parentId);

            return assetFolder;
        }
    }
}
