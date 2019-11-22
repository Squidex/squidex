// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets.Guards
{
    public class GuardAssetFolderTests
    {
        private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();

        [Fact]
        public async Task CanCreate_should_throw_exception_when_folder_name_not_defined()
        {
            var command = new CreateAssetFolder();

            A.CallTo(() => assetQuery.FindAssetFolderAsync(command.ParentId))
                .Returns(new List<IAssetFolderEntity>());

            await ValidationAssert.ThrowsAsync(() => GuardAssetFolder.CanCreate(command, assetQuery),
                new ValidationError("Folder name is required.", "FolderName"));
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_when_folder_not_found()
        {
            var command = new CreateAssetFolder { FolderName = "My Folder", ParentId = Guid.NewGuid() };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(command.ParentId))
                .Returns(new List<IAssetFolderEntity>());

            await ValidationAssert.ThrowsAsync(() => GuardAssetFolder.CanCreate(command, assetQuery),
                new ValidationError("Asset folder does not exist.", "ParentId"));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_when_folder_found()
        {
            var command = new CreateAssetFolder { FolderName = "My Folder", ParentId = Guid.NewGuid() };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(command.ParentId))
                .Returns(new List<IAssetFolderEntity> { CreateFolder() });

            await GuardAssetFolder.CanCreate(command, assetQuery);
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_when_added_to_root()
        {
            var command = new CreateAssetFolder { FolderName = "My Folder" };

            await GuardAssetFolder.CanCreate(command, assetQuery);
        }

        [Fact]
        public async Task CanMove_should_throw_exception_when_adding_to_its_own_child()
        {
            var id = Guid.NewGuid();

            var command = new MoveAssetFolder { ParentId = Guid.NewGuid() };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(command.ParentId))
                .Returns(new List<IAssetFolderEntity>
                {
                    CreateFolder(id),
                    CreateFolder(command.ParentId)
                });

            await ValidationAssert.ThrowsAsync(() => GuardAssetFolder.CanMove(command, assetQuery, id, Guid.NewGuid()),
                new ValidationError("Cannot add folder to its own child.", "ParentId"));
        }

        [Fact]
        public async Task CanMove_should_throw_exception_when_folder_not_found()
        {
            var command = new MoveAssetFolder { ParentId = Guid.NewGuid() };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(command.ParentId))
                .Returns(new List<IAssetFolderEntity>());

            await ValidationAssert.ThrowsAsync(() => GuardAssetFolder.CanMove(command, assetQuery, Guid.NewGuid(), Guid.NewGuid()),
                new ValidationError("Asset folder does not exist.", "ParentId"));
        }

        [Fact]
        public async Task CanMove_should_throw_exception_when_folder_has_not_changed()
        {
            var command = new MoveAssetFolder { ParentId = Guid.NewGuid() };

            await ValidationAssert.ThrowsAsync(() => GuardAssetFolder.CanMove(command, assetQuery, Guid.NewGuid(), command.ParentId),
                new ValidationError("Asset folder is already part of this folder.", "ParentId"));
        }

        [Fact]
        public async Task CanMove_should_not_throw_exception_when_folder_found()
        {
            var command = new MoveAssetFolder { ParentId = Guid.NewGuid() };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(command.ParentId))
                .Returns(new List<IAssetFolderEntity> { CreateFolder() });

            await GuardAssetFolder.CanMove(command, assetQuery, Guid.NewGuid(), Guid.NewGuid());
        }

        [Fact]
        public async Task CanMove_should_not_throw_exception_when_added_to_root()
        {
            var command = new MoveAssetFolder();

            await GuardAssetFolder.CanMove(command, assetQuery, Guid.NewGuid(), Guid.NewGuid());
        }

        [Fact]
        public void CanRename_should_throw_exception_if_folder_name_is_empty()
        {
            var command = new RenameAssetFolder();

            ValidationAssert.Throws(() => GuardAssetFolder.CanRename(command, "My Folder"),
                new ValidationError("Folder name is required.", "FolderName"));
        }

        [Fact]
        public void CanRename_should_throw_exception_if_names_are_the_same()
        {
            var command = new RenameAssetFolder { FolderName = "My Folder" };

            ValidationAssert.Throws(() => GuardAssetFolder.CanRename(command, "My Folder"),
                new ValidationError("Asset folder has already this name.", "FolderName"));
        }

        [Fact]
        public void CanRename_should_not_throw_exception_if_names_are_different()
        {
            var command = new RenameAssetFolder { FolderName = "New Folder Name" };

            GuardAssetFolder.CanRename(command, "My Folder");
        }

        [Fact]
        public void CanDelete_should_not_throw_exception()
        {
            var command = new DeleteAssetFolder();

            GuardAssetFolder.CanDelete(command);
        }

        private IAssetFolderEntity CreateFolder(Guid id = default)
        {
            var assetFolder = A.Fake<IAssetFolderEntity>();

            A.CallTo(() => assetFolder.Id).Returns(id);

            return assetFolder;
        }
    }
}
