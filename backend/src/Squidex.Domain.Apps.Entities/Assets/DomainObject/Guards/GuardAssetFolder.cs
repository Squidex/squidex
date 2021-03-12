// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.Folders;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject.Guards
{
    public static class GuardAssetFolder
    {
        public static void CanCreate(CreateAssetFolder command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(e =>
            {
                if (string.IsNullOrWhiteSpace(command.FolderName))
                {
                    e(Not.Defined(nameof(command.FolderName)), nameof(command.FolderName));
                }

                if (command.ParentId == FolderId.NotFound)
                {
                    e(T.Get("assets.folderNotFound"), nameof(CreateAssetFolder.ParentId));
                }

                if (command.ParentId == FolderId.NotValid)
                {
                    e(T.Get("assets.folderRecursion"), nameof(MoveAssetFolder.ParentId));
                }
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

        public static void CanMove(MoveAssetFolder command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(e =>
            {
                if (command.ParentId == FolderId.NotFound)
                {
                    e(T.Get("assets.folderNotFound"), nameof(CreateAssetFolder.ParentId));
                }

                if (command.ParentId == FolderId.NotValid)
                {
                    e(T.Get("assets.folderRecursion"), nameof(MoveAssetFolder.ParentId));
                }
            });
        }

        public static void CanDelete(DeleteAssetFolder command)
        {
            Guard.NotNull(command, nameof(command));
        }
    }
}
