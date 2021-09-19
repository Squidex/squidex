// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject.Guards
{
    public static class ValidationExtensions
    {
        public static void MustHaveName(this AssetFolderOperation operation, string folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName))
            {
                operation.AddError(Not.Defined(nameof(folderName)), "FolderName");
            }

            operation.ThrowOnErrors();
        }

        public static async Task MustMoveToValidFolder(this AssetOperation operation, DomainId parentId)
        {
            // If moved to root folder or not moved at all, we can just skip the validation.
            if (parentId == DomainId.Empty || parentId == operation.Snapshot.ParentId)
            {
                return;
            }

            var assetQuery = operation.Resolve<IAssetQueryService>();

            var path = await assetQuery.FindAssetFolderAsync(operation.App.Id, parentId);

            if (path.Count == 0)
            {
                operation.AddError(T.Get("assets.folderNotFound"), nameof(MoveAsset.ParentId)).ThrowOnErrors();
            }

            operation.ThrowOnErrors();
        }

        public static async Task MustMoveToValidFolder(this AssetFolderOperation operation, DomainId parentId)
        {
            // If moved to root folder or not moved at all, we can just skip the validation.
            if (parentId == DomainId.Empty || parentId == operation.Snapshot.ParentId)
            {
                return;
            }

            var assetQuery = operation.Resolve<IAssetQueryService>();

            var path = await assetQuery.FindAssetFolderAsync(operation.App.Id, parentId);

            if (path.Count == 0)
            {
                operation.AddError(T.Get("assets.folderNotFound"), nameof(MoveAssetFolder.ParentId));
            }
            else if (operation.CommandId != DomainId.Empty)
            {
                var indexOfSelf = path.IndexOf(x => x.Id == operation.CommandId);
                var indexOfParent = path.IndexOf(x => x.Id == parentId);

                // If we would move the folder to its own parent (the parent comes first in the path), we would create a recursion.
                if (indexOfSelf >= 0 && indexOfParent > indexOfSelf)
                {
                    operation.AddError(T.Get("assets.folderRecursion"), nameof(MoveAssetFolder.ParentId));
                }
            }

            operation.ThrowOnErrors();
        }

        public static async Task CheckReferrersAsync(this AssetOperation operation)
        {
            var contentRepository = operation.Resolve<IContentRepository>();

            var hasReferrer = await contentRepository.HasReferrersAsync(operation.App.Id, operation.CommandId, SearchScope.All, default);

            if (hasReferrer)
            {
                throw new DomainException(T.Get("assets.referenced"), "OBJECT_REFERENCED");
            }
        }
    }
}
