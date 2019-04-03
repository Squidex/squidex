// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.Guards
{
    public static class GuardAsset
    {
        public static void CanAnnotate(AnnotateAsset command, string oldFileName, string oldSlug)
        {
            Guard.NotNull(command, nameof(command));

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

        public static void CanCreate(CreateAsset command)
        {
            Guard.NotNull(command, nameof(command));
        }

        public static void CanUpdate(UpdateAsset command)
        {
            Guard.NotNull(command, nameof(command));
        }

        public static void CanDelete(DeleteAsset command)
        {
            Guard.NotNull(command, nameof(command));
        }
    }
}
