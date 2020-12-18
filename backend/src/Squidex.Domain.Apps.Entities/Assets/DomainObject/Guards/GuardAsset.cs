// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
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
        public static void CanAnnotate(AnnotateAsset command)
        {
            Guard.NotNull(command, nameof(command));
        }

        public static Task CanCreate(CreateAsset command, IAssetQueryService assetQuery)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(async e =>
            {
                await CheckPathAsync(command.AppId.Id, command.ParentId, assetQuery, e);
            });
        }

        public static Task CanMove(MoveAsset command, IAssetEntity asset, IAssetQueryService assetQuery)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(async e =>
            {
                if (command.ParentId != asset.ParentId)
                {
                    await CheckPathAsync(command.AppId.Id, command.ParentId, assetQuery, e);
                }
            });
        }

        public static void CanUpdate(UpdateAsset command)
        {
            Guard.NotNull(command, nameof(command));
        }

        public static async Task CanDelete(DeleteAsset command, IAssetEntity asset, IContentRepository contentRepository)
        {
            Guard.NotNull(command, nameof(command));

            if (command.CheckReferrers)
            {
                var hasReferrer = await contentRepository.HasReferrersAsync(asset.AppId.Id, asset.Id, SearchScope.All);

                if (hasReferrer)
                {
                    throw new DomainException(T.Get("assets.referenced"));
                }
            }
        }

        private static async Task CheckPathAsync(DomainId appId, DomainId parentId, IAssetQueryService assetQuery, AddValidation e)
        {
            if (parentId != DomainId.Empty)
            {
                var path = await assetQuery.FindAssetFolderAsync(appId, parentId);

                if (path.Count == 0)
                {
                    e(T.Get("assets.folderNotFound"), nameof(MoveAsset.ParentId));
                }
            }
        }
    }
}
