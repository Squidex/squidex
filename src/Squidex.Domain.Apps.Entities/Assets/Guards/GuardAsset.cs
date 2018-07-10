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

namespace Squidex.Domain.Apps.Entities.Assets.Guards
{
    public static class GuardAsset
    {
        public static void CanRename(RenameAsset command, string oldName)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot rename asset.", e =>
            {
                if (string.IsNullOrWhiteSpace(command.Name))
                {
                    e("Name is required.", nameof(command.Name));
                }

                if (string.Equals(command.Name, oldName))
                {
                    e("Asset has already this name.", nameof(command.Name));
                }
            });
        }

        public static void CanCreateFolder(CreateAssetFolder command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot rename asset.", e =>
            {
                if (string.IsNullOrWhiteSpace(command.Name))
                {
                    e("Name is required.", nameof(command.Name));
                }
            });
        }

        public static Task CanMove(MoveAsset command, IAssetVerifier assetVerifier, Guid? oldFolderId)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(() => "Cannot rename asset.", async e =>
            {
                if (oldFolderId == command.FolderId)
                {
                    e("Asset is already in this folder.", nameof(command.FolderId));
                }
                else if (command.FolderId.HasValue && !await assetVerifier.FolderExistsAsync(command.FolderId.Value))
                {
                    e("Folder not found.", nameof(command.FolderId));
                }
            });
        }

        public static Task CanCreate(CreateAsset command, IAssetVerifier assetVerifier)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(() => "Cannot rename asset.", async e =>
            {
                if (command.FolderId.HasValue && !await assetVerifier.FolderExistsAsync(command.FolderId.Value))
                {
                    e("Folder not found.", nameof(command.FolderId));
                }
            });
        }

        public static void CanUpdate(UpdateAsset command, bool isFolder)
        {
            Guard.NotNull(command, nameof(command));

            if (isFolder)
            {
                throw new DomainException("Asset is a folder and cannot be updated.");
            }
        }

        public static void CanDelete(DeleteAsset command)
        {
            Guard.NotNull(command, nameof(command));
        }
    }
}
