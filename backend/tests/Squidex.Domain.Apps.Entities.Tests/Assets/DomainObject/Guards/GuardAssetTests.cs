// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject.Guards
{
    public class GuardAssetTests : IClassFixture<TranslationsFixture>
    {
        private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");

        [Fact]
        public async Task CanMove_should_throw_exception_if_folder_not_found()
        {
            var parentId = DomainId.NewGuid();

            var command = new MoveAsset { AppId = appId, ParentId = parentId };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, parentId, default))
                .Returns(new List<IAssetFolderEntity>());

            await ValidationAssert.ThrowsAsync(() => GuardAsset.CanMove(command, Asset(), assetQuery),
                new ValidationError("Asset folder does not exist.", "ParentId"));
        }

        [Fact]
        public async Task CanMove_should_not_throw_exception_if_folder_not_found_but_optimized()
        {
            var parentId = DomainId.NewGuid();

            var command = new MoveAsset { AppId = appId, ParentId = parentId, OptimizeValidation = true };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, command.ParentId, default))
                .Returns(new List<IAssetFolderEntity>());

            await GuardAsset.CanMove(command, Asset(), assetQuery);
        }

        [Fact]
        public async Task CanMove_should_not_throw_exception_if_folder_found()
        {
            var parentId = DomainId.NewGuid();

            var command = new MoveAsset { AppId = appId, ParentId = parentId };

            A.CallTo(() => assetQuery.FindAssetFolderAsync(appId.Id, command.ParentId, default))
                .Returns(new List<IAssetFolderEntity> { AssetFolder() });

            await GuardAsset.CanMove(command, Asset(), assetQuery);
        }

        [Fact]
        public async Task CanMove_should_not_throw_exception_if_folder_has_not_changed()
        {
            var parentId = DomainId.NewGuid();

            var command = new MoveAsset { AppId = appId, ParentId = parentId };

            await GuardAsset.CanMove(command, Asset(parentId: parentId), assetQuery);
        }

        [Fact]
        public async Task CanMove_should_not_throw_exception_if_added_to_root()
        {
            var command = new MoveAsset { AppId = appId };

            await GuardAsset.CanMove(command, Asset(), assetQuery);
        }

        [Fact]
        public async Task CanDelete_should_throw_exception_if_referenced()
        {
            var asset = Asset();

            var command = new DeleteAsset { AppId = appId, CheckReferrers = true };

            A.CallTo(() => contentRepository.HasReferrersAsync(appId.Id, asset.Id, SearchScope.All, default))
                .Returns(true);

            await Assert.ThrowsAsync<DomainException>(() => GuardAsset.CanDelete(command, asset, contentRepository));
        }

        [Fact]
        public async Task CanDelete_should_not_throw_exception()
        {
            var command = new DeleteAsset { AppId = appId };

            await GuardAsset.CanDelete(command, Asset(), contentRepository);
        }

        private IAssetEntity Asset(DomainId id = default, DomainId parentId = default)
        {
            var asset = A.Fake<IAssetEntity>();

            A.CallTo(() => asset.Id)
                .Returns(id == default ? DomainId.NewGuid() : id);
            A.CallTo(() => asset.AppId)
                .Returns(appId);
            A.CallTo(() => asset.ParentId)
                .Returns(parentId == default ? DomainId.NewGuid() : parentId);

            return asset;
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
