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
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Assets.Guards
{
    public static class GuardAssetFolder
    {
        public static Task CanCreate(CreateAssetFolder command, IAssetQueryService assetQuery)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(async e =>
            {
                if (string.IsNullOrWhiteSpace(command.FolderName))
                {
                    e(Not.Defined(nameof(command.FolderName)), nameof(command.FolderName));
                }

                await CheckPathAsync(command.ParentId, assetQuery, Guid.Empty, e);
            });
        }

        public static void CanRename(RenameAssetFolder command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(e =>
            {
                if (string.IsNullOrWhiteSpace(command.FolderName))
                {
                    e(Not.Defined(nameof(command.FolderName)), nameof(command.FolderName));
                }
            });
        }

        public static Task CanMove(MoveAssetFolder command, IAssetQueryService assetQuery, Guid id, Guid oldParentId)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(async e =>
            {
                if (command.ParentId != oldParentId)
                {
                    await CheckPathAsync(command.ParentId, assetQuery, id, e);
                }
            });
        }

        public static void CanDelete(DeleteAssetFolder command)
        {
            Guard.NotNull(command, nameof(command));
        }

        private static async Task CheckPathAsync(Guid parentId, IAssetQueryService assetQuery, Guid id, AddValidation e)
        {
            if (parentId != default)
            {
                var path = await assetQuery.FindAssetFolderAsync(parentId);

                if (path.Count == 0)
                {
                    e(T.Get("assets.folderNotFound"), nameof(MoveAssetFolder.ParentId));
                }
                else if (id != default)
                {
                    var indexOfThis = path.IndexOf(x => x.Id == id);
                    var indexOfParent = path.IndexOf(x => x.Id == parentId);

                    if (indexOfThis >= 0 && indexOfParent > indexOfThis)
                    {
                        e(T.Get("assets.folderRecursion"), nameof(MoveAssetFolder.ParentId));
                    }
                }
            }
        }
    }
}
