// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Assets.Guards
{
    public static class GuardAssetFolder
    {
        public static Task CanCreate(CreateAssetFolder command, IAssetQueryService assetQuery)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(() => "Cannot upload asset.", async e =>
            {
                if (string.IsNullOrWhiteSpace(command.FolderName))
                {
                    e(Not.Defined("Folder name"), nameof(command.FolderName));
                }

                await CheckPathAsync(command.AppId.Id, command.ParentId, assetQuery, DomainId.Empty, e);
            });
        }

        public static void CanRename(RenameAssetFolder command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot rename asset.", e =>
            {
                if (string.IsNullOrWhiteSpace(command.FolderName))
                {
                    e(Not.Defined("Folder name"), nameof(command.FolderName));
                }
            });
        }

        public static Task CanMove(MoveAssetFolder command, IAssetQueryService assetQuery, DomainId id, DomainId oldParentId)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(() => "Cannot move asset.", async e =>
            {
                if (command.ParentId != oldParentId)
                {
                    await CheckPathAsync(command.AppId.Id, command.ParentId, assetQuery, id, e);
                }
            });
        }

        public static void CanDelete(DeleteAssetFolder command)
        {
            Guard.NotNull(command, nameof(command));
        }

        private static async Task CheckPathAsync(DomainId appId, DomainId parentId, IAssetQueryService assetQuery, DomainId id, AddValidation e)
        {
            if (parentId != DomainId.Empty)
            {
                var path = await assetQuery.FindAssetFolderAsync(appId, parentId);

                if (path.Count == 0)
                {
                    e("Asset folder does not exist.", nameof(MoveAssetFolder.ParentId));
                }
                else if (id != DomainId.Empty)
                {
                    var indexOfSelf = path.IndexOf(x => x.Id == id);
                    var indexOfParent = path.IndexOf(x => x.Id == parentId);

                    if (indexOfSelf >= 0 && indexOfParent > indexOfSelf)
                    {
                        e("Cannot add folder to its own child.", nameof(MoveAssetFolder.ParentId));
                    }
                }
            }
        }
    }
}
