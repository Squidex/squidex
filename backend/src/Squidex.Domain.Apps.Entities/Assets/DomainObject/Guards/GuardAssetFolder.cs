// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject.Guards
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

                await CheckPathAsync(command.AppId.Id, command.ParentId, assetQuery, DomainId.Empty, e);
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

        public static Task CanMove(MoveAssetFolder command, IAssetFolderEntity assetFolder, IAssetQueryService assetQuery)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(async e =>
            {
                if (command.ParentId != assetFolder.ParentId)
                {
                    await CheckPathAsync(command.AppId.Id, command.ParentId, assetQuery, assetFolder.Id, e);
                }
            });
        }

        private static async Task CheckPathAsync(DomainId appId, DomainId parentId, IAssetQueryService assetQuery, DomainId id, AddValidation e)
        {
            if (parentId != DomainId.Empty)
            {
                var path = await assetQuery.FindAssetFolderAsync(appId, parentId);

                if (path.Count == 0)
                {
                    e(T.Get("assets.folderNotFound"), nameof(MoveAssetFolder.ParentId));
                }
                else if (id != DomainId.Empty)
                {
                    var indexOfSelf = path.IndexOf(x => x.Id == id);
                    var indexOfParent = path.IndexOf(x => x.Id == parentId);

                    if (indexOfSelf >= 0 && indexOfParent > indexOfSelf)
                    {
                        e(T.Get("assets.folderRecursion"), nameof(MoveAssetFolder.ParentId));
                    }
                }
            }
        }
    }
}
