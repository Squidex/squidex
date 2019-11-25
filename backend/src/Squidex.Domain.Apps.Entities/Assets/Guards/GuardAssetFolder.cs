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
            Guard.NotNull(command);

            return Validate.It(() => "Cannot upload asset.", async e =>
            {
                if (string.IsNullOrWhiteSpace(command.FolderName))
                {
                    e(Not.Defined("Folder name"), nameof(command.FolderName));
                }

                await CheckPathAsync(command.ParentId, assetQuery, Guid.Empty, e);
            });
        }

        public static void CanRename(RenameAssetFolder command, string olderFolderName)
        {
            Guard.NotNull(command);

            Validate.It(() => "Cannot rename asset.", e =>
            {
                if (string.IsNullOrWhiteSpace(command.FolderName))
                {
                    e(Not.Defined("Folder name"), nameof(command.FolderName));
                }
                else if (string.Equals(command.FolderName, olderFolderName))
                {
                    e(Not.New("Asset folder", "name"), nameof(command.FolderName));
                }
            });
        }

        public static Task CanMove(MoveAssetFolder command, IAssetQueryService assetQuery, Guid id, Guid oldParentId)
        {
            Guard.NotNull(command);

            return Validate.It(() => "Cannot move asset.", async e =>
            {
                if (command.ParentId == oldParentId)
                {
                    e("Asset folder is already part of this folder.", nameof(command.ParentId));
                }
                else
                {
                    await CheckPathAsync(command.ParentId, assetQuery, id, e);
                }
            });
        }

        public static void CanDelete(DeleteAssetFolder command)
        {
            Guard.NotNull(command);
        }

        private static async Task CheckPathAsync(Guid parentId, IAssetQueryService assetQuery, Guid id, AddValidation e)
        {
            if (parentId != default)
            {
                var path = await assetQuery.FindAssetFolderAsync(parentId);

                if (path.Count == 0)
                {
                    e("Asset folder does not exist.", nameof(MoveAssetFolder.ParentId));
                }
                else if (id != default)
                {
                    var indexOfThis = path.IndexOf(x => x.Id == id);
                    var indexOfParent = path.IndexOf(x => x.Id == parentId);

                    if (indexOfThis >= 0 && indexOfParent > indexOfThis)
                    {
                        e("Cannot add folder to its own child.", nameof(MoveAssetFolder.ParentId));
                    }
                }
            }
        }
    }
}
