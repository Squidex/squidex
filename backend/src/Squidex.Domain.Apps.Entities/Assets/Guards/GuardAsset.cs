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
    public static class GuardAsset
    {
        public static void CanAnnotate(AnnotateAsset command, string oldFileName, string oldSlug)
        {
            Guard.NotNull(command);

            Validate.It(() => "Cannot rename asset.", e =>
            {
                if (string.IsNullOrWhiteSpace(command.FileName) &&
                    string.IsNullOrWhiteSpace(command.Slug) &&
                    command.Tags == null)
                {
                   e("Either file name, slug or tags must be defined.", nameof(command.FileName), nameof(command.Slug), nameof(command.Tags));
                }

                if (!string.IsNullOrWhiteSpace(command.FileName) && string.Equals(command.FileName, oldFileName))
                {
                    e(Not.New("Asset", "name"), nameof(command.FileName));
                }

                if (!string.IsNullOrWhiteSpace(command.Slug) && string.Equals(command.Slug, oldSlug))
                {
                    e(Not.New("Asset", "slug"), nameof(command.Slug));
                }
            });
        }

        public static Task CanCreate(CreateAsset command, IAssetQueryService assetQuery)
        {
            Guard.NotNull(command);

            return Validate.It(() => "Cannot upload asset.", async e =>
            {
                await CheckPathAsync(command.ParentId, assetQuery, e);
            });
        }

        public static Task CanMove(MoveAsset command, IAssetQueryService assetQuery, Guid oldParentId)
        {
            Guard.NotNull(command);

            return Validate.It(() => "Cannot move asset.", async e =>
            {
                if (command.ParentId == oldParentId)
                {
                    e("Asset is already part of this folder.", nameof(command.ParentId));
                }
                else
                {
                    await CheckPathAsync(command.ParentId, assetQuery, e);
                }
            });
        }

        public static void CanUpdate(UpdateAsset command)
        {
            Guard.NotNull(command);
        }

        public static void CanDelete(DeleteAsset command)
        {
            Guard.NotNull(command);
        }

        private static async Task CheckPathAsync(Guid parentId, IAssetQueryService assetQuery, AddValidation e)
        {
            if (parentId != default)
            {
                var path = await assetQuery.FindAssetFolderAsync(parentId);

                if (path.Count == 0)
                {
                    e("Asset folder does not exist.", nameof(MoveAsset.ParentId));
                }
            }
        }
    }
}
