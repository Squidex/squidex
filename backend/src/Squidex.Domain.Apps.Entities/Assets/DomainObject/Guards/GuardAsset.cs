// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.Folders;
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

        public static void CanCreate(CreateAsset command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(e =>
            {
                if (command.ParentId == FolderId.NotFound)
                {
                    e(T.Get("assets.folderNotFound"), nameof(MoveAsset.ParentId));
                }
            });
        }

        public static void CanMove(MoveAsset command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(e =>
            {
                if (command.ParentId == FolderId.NotFound)
                {
                    e(T.Get("assets.folderNotFound"), nameof(MoveAsset.ParentId));
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
    }
}
