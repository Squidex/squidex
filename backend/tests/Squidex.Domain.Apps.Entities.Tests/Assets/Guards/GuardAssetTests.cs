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
    public class GuardAssetTests : IClassFixture<TranslationsFixture>
    {
        private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");

        [Fact]
        public async Task CanCreate_should_not_throw_exception_when_folder_found()
        {
            var command = new CreateAsset { AppId = appId, ParentId = DomainId.NewGuid() };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, command.ParentId))
                .Returns(new List<IAssetFolderEntity> { CreateFolder() });

            await GuardAsset.CanCreate(command, assetQuery);
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_when_folder_not_found()
        {
            var command = new CreateAsset { AppId = appId, ParentId = DomainId.NewGuid() };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, command.ParentId))
                .Returns(new List<IAssetFolderEntity>());

            await ValidationAssert.ThrowsAsync(() => GuardAsset.CanCreate(command, assetQuery),
                new ValidationError("Asset folder does not exist.", "ParentId"));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_when_added_to_root()
        {
            var command = new CreateAsset { AppId = appId };

            await GuardAsset.CanCreate(command, assetQuery);
        }

        [Fact]
        public async Task CanMove_should_throw_exception_when_folder_not_found()
        {
            var command = new MoveAsset { AppId = appId, ParentId = DomainId.NewGuid() };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, command.ParentId))
                .Returns(new List<IAssetFolderEntity>());

            await ValidationAssert.ThrowsAsync(() => GuardAsset.CanMove(command, assetQuery, DomainId.NewGuid()),
                new ValidationError("Asset folder does not exist.", "ParentId"));
        }

        [Fact]
        public async Task CanMove_should_not_throw_exception_when_folder_has_not_changed()
        {
            var command = new MoveAsset { AppId = appId, ParentId = DomainId.NewGuid() };

            await GuardAsset.CanMove(command, assetQuery, command.ParentId);
        }

        [Fact]
        public async Task CanMove_should_not_throw_exception_when_folder_found()
        {
            var command = new MoveAsset { AppId = appId, ParentId = DomainId.NewGuid() };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, command.ParentId))
                .Returns(new List<IAssetFolderEntity> { CreateFolder() });

            await GuardAsset.CanMove(command, assetQuery, DomainId.NewGuid());
        }

        [Fact]
        public async Task CanMove_should_not_throw_exception_when_added_to_root()
        {
            var command = new MoveAsset { AppId = appId };

            await GuardAsset.CanMove(command, assetQuery, DomainId.NewGuid());
        }

        [Fact]
        public void CanAnnotate_should_not_throw_exception_if_empty()
        {
            var command = new AnnotateAsset { AppId = appId };

            GuardAsset.CanAnnotate(command);
        }

        [Fact]
        public void CanAnnotate_should_not_throw_exception_if_a_value_is_passed()
        {
            var command = new AnnotateAsset { AppId = appId, FileName = "new-name", Slug = "new-slug" };

            GuardAsset.CanAnnotate(command);
        }

        [Fact]
        public void CanUpdate_should_not_throw_exception()
        {
            var command = new UpdateAsset { AppId = appId };

            GuardAsset.CanUpdate(command);
        }

        [Fact]
        public void CanDelete_should_not_throw_exception()
        {
            var command = new DeleteAsset { AppId = appId };

            GuardAsset.CanDelete(command);
        }

        private static IAssetFolderEntity CreateFolder(DomainId id = default)
        {
            var assetFolder = A.Fake<IAssetFolderEntity>();

            A.CallTo(() => assetFolder.Id).Returns(id);

            return assetFolder;
        }
    }
}
