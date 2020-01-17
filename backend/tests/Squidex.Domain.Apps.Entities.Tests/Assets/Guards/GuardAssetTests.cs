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
    public class GuardAssetTests
    {
        private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();

        [Fact]
        public async Task CanCreate_should_not_throw_exception_when_folder_found()
        {
            var command = new CreateAsset { ParentId = Guid.NewGuid() };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(command.ParentId))
                .Returns(new List<IAssetFolderEntity> { CreateFolder() });

            await GuardAsset.CanCreate(command, assetQuery);
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_when_folder_not_found()
        {
            var command = new CreateAsset { ParentId = Guid.NewGuid() };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(command.ParentId))
                .Returns(new List<IAssetFolderEntity>());

            await ValidationAssert.ThrowsAsync(() => GuardAsset.CanCreate(command, assetQuery),
                new ValidationError("Asset folder does not exist.", "ParentId"));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_when_added_to_root()
        {
            var command = new CreateAsset();

            await GuardAsset.CanCreate(command, assetQuery);
        }

        [Fact]
        public async Task CanMove_should_throw_exception_when_folder_not_found()
        {
            var command = new MoveAsset { ParentId = Guid.NewGuid() };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(command.ParentId))
                .Returns(new List<IAssetFolderEntity>());

            await ValidationAssert.ThrowsAsync(() => GuardAsset.CanMove(command, assetQuery, Guid.NewGuid()),
                new ValidationError("Asset folder does not exist.", "ParentId"));
        }

        [Fact]
        public async Task CanMove_should_not_throw_exception_when_folder_has_not_changed()
        {
            var command = new MoveAsset { ParentId = Guid.NewGuid() };

            await GuardAsset.CanMove(command, assetQuery, command.ParentId);
        }

        [Fact]
        public async Task CanMove_should_not_throw_exception_when_folder_found()
        {
            var command = new MoveAsset { ParentId = Guid.NewGuid() };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(command.ParentId))
                .Returns(new List<IAssetFolderEntity> { CreateFolder() });

            await GuardAsset.CanMove(command, assetQuery, Guid.NewGuid());
        }

        [Fact]
        public async Task CanMove_should_not_throw_exception_when_added_to_root()
        {
            var command = new MoveAsset();

            await GuardAsset.CanMove(command, assetQuery, Guid.NewGuid());
        }

        [Fact]
        public void CanAnnotate_should_throw_exception_if_nothing_defined()
        {
            var command = new AnnotateAsset();

            ValidationAssert.Throws(() => GuardAsset.CanAnnotate(command),
                new ValidationError("At least one property must be defined.", "FileName", "IsProtected", "Metadata", "Slug", "Tags"));
        }

        [Fact]
        public void CanAnnotate_should_not_throw_exception_if_a_value_is_passed()
        {
            var command = new AnnotateAsset { FileName = "new-name", Slug = "new-slug" };

            GuardAsset.CanAnnotate(command);
        }

        [Fact]
        public void CanUpdate_should_not_throw_exception()
        {
            var command = new UpdateAsset();

            GuardAsset.CanUpdate(command);
        }

        [Fact]
        public void CanDelete_should_not_throw_exception()
        {
            var command = new DeleteAsset();

            GuardAsset.CanDelete(command);
        }

        private static IAssetFolderEntity CreateFolder(Guid id = default)
        {
            var assetFolder = A.Fake<IAssetFolderEntity>();

            A.CallTo(() => assetFolder.Id).Returns(id);

            return assetFolder;
        }
    }
}
