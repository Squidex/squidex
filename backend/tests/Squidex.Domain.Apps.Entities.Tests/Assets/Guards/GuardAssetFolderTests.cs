// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets.Guards
{
    public class GuardAssetFolderTests : IClassFixture<TranslationsFixture>
    {
        private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");

        [Fact]
        public async Task CanCreate_should_throw_exception_when_folder_name_not_defined()
        {
            var command = new CreateAssetFolder { AppId = appId };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, command.ParentId))
                .Returns(new List<IAssetFolderEntity>());

            await ValidationAssert.ThrowsAsync(() => GuardAssetFolder.CanCreate(command, assetQuery),
                new ValidationError("Folder name is required.", "FolderName"));
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_when_folder_not_found()
        {
            var command = new CreateAssetFolder { AppId = appId, FolderName = "My Folder", ParentId = DomainId.NewGuid() };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, command.ParentId))
                .Returns(new List<IAssetFolderEntity>());

            await ValidationAssert.ThrowsAsync(() => GuardAssetFolder.CanCreate(command, assetQuery),
                new ValidationError("Asset folder does not exist.", "ParentId"));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_when_folder_found()
        {
            var command = new CreateAssetFolder { AppId = appId, FolderName = "My Folder", ParentId = DomainId.NewGuid() };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, command.ParentId))
                .Returns(new List<IAssetFolderEntity> { CreateFolder() });

            await GuardAssetFolder.CanCreate(command, assetQuery);
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_when_added_to_root()
        {
            var command = new CreateAssetFolder { AppId = appId, FolderName = "My Folder" };

            await GuardAssetFolder.CanCreate(command, assetQuery);
        }

        [Fact]
        public async Task CanMove_should_throw_exception_when_adding_to_its_own_child()
        {
            var id = DomainId.NewGuid();

            var command = new MoveAssetFolder { AppId = appId, ParentId = DomainId.NewGuid() };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, command.ParentId))
                .Returns(new List<IAssetFolderEntity>
                {
                    CreateFolder(id),
                    CreateFolder(command.ParentId)
                });

            await ValidationAssert.ThrowsAsync(() => GuardAssetFolder.CanMove(command, assetQuery, id, DomainId.NewGuid()),
                new ValidationError("Cannot add folder to its own child.", "ParentId"));
        }

        [Fact]
        public async Task CanMove_should_throw_exception_when_folder_not_found()
        {
            var command = new MoveAssetFolder { AppId = appId, ParentId = DomainId.NewGuid() };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, command.ParentId))
                .Returns(new List<IAssetFolderEntity>());

            await ValidationAssert.ThrowsAsync(() => GuardAssetFolder.CanMove(command, assetQuery, DomainId.NewGuid(), DomainId.NewGuid()),
                new ValidationError("Asset folder does not exist.", "ParentId"));
        }

        [Fact]
        public async Task CanMove_should_not_throw_exception_when_folder_found()
        {
            var command = new MoveAssetFolder { AppId = appId, ParentId = DomainId.NewGuid() };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, command.ParentId))
                .Returns(new List<IAssetFolderEntity> { CreateFolder() });

            await GuardAssetFolder.CanMove(command, assetQuery, DomainId.NewGuid(), DomainId.NewGuid());
        }

        [Fact]
        public async Task CanMove_should_not_throw_exception_when_folder_has_not_changed()
        {
            var command = new MoveAssetFolder { AppId = appId, ParentId = DomainId.NewGuid() };

            await GuardAssetFolder.CanMove(command, assetQuery, DomainId.NewGuid(), command.ParentId);
        }

        [Fact]
        public async Task CanMove_should_not_throw_exception_when_added_to_root()
        {
            var command = new MoveAssetFolder { AppId = appId };

            await GuardAssetFolder.CanMove(command, assetQuery, DomainId.NewGuid(), DomainId.NewGuid());
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

        [Fact]
        public void CanDelete_should_not_throw_exception()
        {
            var command = new DeleteAssetFolder { AppId = appId };

            GuardAssetFolder.CanDelete(command);
        }

        private static IAssetFolderEntity CreateFolder(DomainId id = default)
        {
            var assetFolder = A.Fake<IAssetFolderEntity>();

            A.CallTo(() => assetFolder.Id).Returns(id);

            return assetFolder;
        }
    }
}
