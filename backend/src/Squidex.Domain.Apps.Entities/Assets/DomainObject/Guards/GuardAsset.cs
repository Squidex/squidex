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
    public static class GuardAsset
    {
        public static Task CanMove(MoveAsset command, IAssetEntity asset, IAssetQueryService assetQuery)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(async e =>
            {
                var parentId = command.ParentId;

                if (parentId != asset.ParentId && parentId != DomainId.Empty && !command.OptimizeValidation)
                {
                    var path = await assetQuery.FindAssetFolderAsync(command.AppId.Id, parentId);

                    if (path.Count == 0)
                    {
                        e(T.Get("assets.folderNotFound"), nameof(MoveAsset.ParentId));
                    }
                }
            });
        }

        public static async Task CanDelete(DeleteAsset command, IAssetEntity asset, IContentRepository contentRepository)
        {
            Guard.NotNull(command, nameof(command));

            if (command.CheckReferrers)
            {
                var hasReferrer = await contentRepository.HasReferrersAsync(asset.AppId.Id, asset.Id, SearchScope.All, default);

                if (hasReferrer)
                {
                    throw new DomainException(T.Get("assets.referenced"), "OBJECT_REFERENCED");
                }
            }
        }
    }
}
