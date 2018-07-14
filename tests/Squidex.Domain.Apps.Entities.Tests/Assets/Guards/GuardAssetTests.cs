// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets.Guards
{
    public class GuardAssetTests
    {
        private readonly IAssetVerifier assetVerifier = A.Fake<IAssetVerifier>();

        [Fact]
        public void CanRename_should_throw_exception_if_name_not_defined()
        {
            var command = new RenameAsset();

            ValidationAssert.Throws(() => GuardAsset.CanRename(command, "asset-name"),
                new ValidationError("Name is required.", "Name"));
        }

        [Fact]
        public void CanRename_should_throw_exception_if_name_are_the_same()
        {
            var command = new RenameAsset { Name = "asset-name" };

            ValidationAssert.Throws(() => GuardAsset.CanRename(command, "asset-name"),
                new ValidationError("Asset has already this name.", "Name"));
        }

        [Fact]
        public void CanRename_should_not_throw_exception_if_name_are_different()
        {
            var command = new RenameAsset { Name = "new-name" };

            GuardAsset.CanRename(command, "asset-name");
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_if_folder_found()
        {
            var folderId = Guid.NewGuid();

            A.CallTo(() => assetVerifier.FolderExistsAsync(folderId))
                .Returns(false);

            var command = new CreateAsset { FolderId = folderId };

            await ValidationAssert.ThrowsAsync(() => GuardAsset.CanCreate(command, assetVerifier),
                new ValidationError("Folder not found.", "FolderId"));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_if_folder_found()
        {
            var folderId = Guid.NewGuid();

            A.CallTo(() => assetVerifier.FolderExistsAsync(folderId))
                .Returns(true);

            var command = new CreateAsset { FolderId = folderId };

            await GuardAsset.CanCreate(command, assetVerifier);
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception()
        {
            var command = new CreateAsset();

            await GuardAsset.CanCreate(command, assetVerifier);
        }

        [Fact]
        public async Task CanMove_should_throw_exception_if_folder_found()
        {
            var folderId = Guid.NewGuid();

            A.CallTo(() => assetVerifier.FolderExistsAsync(folderId))
                .Returns(false);

            var command = new MoveAsset { FolderId = folderId };

            await ValidationAssert.ThrowsAsync(() => GuardAsset.CanMove(command, assetVerifier, null),
                new ValidationError("Folder not found.", "FolderId"));
        }

        [Fact]
        public async Task CanMove_should_throw_exception_if_folder_is_same()
        {
            var folderId = Guid.NewGuid();

            var command = new MoveAsset { FolderId = folderId };

            await ValidationAssert.ThrowsAsync(() => GuardAsset.CanMove(command, assetVerifier, folderId),
                new ValidationError("Asset is already in this folder.", "FolderId"));
        }

        [Fact]
        public async Task CanMove_should_not_throw_exception_if_folder_found()
        {
            var folderId = Guid.NewGuid();

            A.CallTo(() => assetVerifier.FolderExistsAsync(folderId))
                .Returns(true);

            var command = new MoveAsset { FolderId = folderId };

            await GuardAsset.CanMove(command, assetVerifier, null);
        }

        [Fact]
        public async Task CanMove_should_not_throw_exception()
        {
            var command = new MoveAsset();

            await GuardAsset.CanMove(command, assetVerifier, Guid.NewGuid());
        }

        [Fact]
        public void CanUpdate_should_exception_if_folder()
        {
            var command = new UpdateAsset();

            Assert.Throws<DomainException>(() => GuardAsset.CanUpdate(command, true));
        }

        [Fact]
        public void CanUpdate_should_not_throw_exception()
        {
            var command = new UpdateAsset();

            GuardAsset.CanUpdate(command, false);
        }

        [Fact]
        public void CanDelete_should_not_throw_exception()
        {
            var command = new DeleteAsset();

            GuardAsset.CanDelete(command);
        }
    }
}
