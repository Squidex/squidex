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
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets.Guards
{
    public class GuardAssetTests
    {
        private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();

        [Fact]
        public void CanAnnotate_should_throw_exception_if_nothing_defined()
        {
            var command = new AnnotateAsset();

            ValidationAssert.Throws(() => GuardAsset.CanAnnotate(command, "asset-name", "asset-slug"),
                new ValidationError("Either file name, slug or tags must be defined.", "FileName", "Slug", "Tags"));
        }

        [Fact]
        public void CanAnnotate_should_throw_exception_if_names_are_the_same()
        {
            var command = new AnnotateAsset { FileName = "asset-name" };

            ValidationAssert.Throws(() => GuardAsset.CanAnnotate(command, "asset-name", "asset-slug"),
                new ValidationError("Asset has already this name.", "FileName"));
        }

        [Fact]
        public void CanAnnotate_should_throw_exception_if_slugs_are_the_same()
        {
            var command = new AnnotateAsset { Slug = "asset-slug" };

            ValidationAssert.Throws(() => GuardAsset.CanAnnotate(command, "asset-name", "asset-slug"),
                new ValidationError("Asset has already this slug.", "Slug"));
        }

        [Fact]
        public void CanAnnotate_should_not_throw_exception_if_names_are_different()
        {
            var command = new AnnotateAsset { FileName = "new-name", Slug = "new-slug" };

            GuardAsset.CanAnnotate(command, "asset-name", "asset-slug");
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_when_folder_not_found()
        {
            var command = new CreateAsset { ParentId = Guid.NewGuid() };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(command.ParentId))
                .Returns(Task.FromResult<IAssetFolderEntity?>(null));

            await ValidationAssert.ThrowsAsync(() => GuardAsset.CanCreate(command, assetQuery),
                new ValidationError("Asset folder does not exist.", "ParentId"));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_when_folder_found()
        {
            var command = new CreateAsset { ParentId = Guid.NewGuid() };

            await GuardAsset.CanCreate(command, assetQuery);
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
                .Returns(Task.FromResult<IAssetFolderEntity?>(null));

            await ValidationAssert.ThrowsAsync(() => GuardAsset.CanMove(command, assetQuery, Guid.NewGuid()),
                new ValidationError("Asset folder does not exist.", "ParentId"));
        }

        [Fact]
        public async Task CanMove_should_throw_exception_when_folder_has_not_changed()
        {
            var command = new MoveAsset { ParentId = Guid.NewGuid() };

            await ValidationAssert.ThrowsAsync(() => GuardAsset.CanMove(command, assetQuery, command.ParentId),
                new ValidationError("Asset is already part of this folder.", "ParentId"));
        }

        [Fact]
        public async Task CanMove_should_not_throw_exception_when_folder_found()
        {
            var command = new MoveAsset { ParentId = Guid.NewGuid() };

            await GuardAsset.CanMove(command, assetQuery, Guid.NewGuid());
        }

        [Fact]
        public async Task CanMove_should_not_throw_exception_when_added_to_root()
        {
            var command = new MoveAsset();

            await GuardAsset.CanMove(command, assetQuery, Guid.NewGuid());
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
    }
}
