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
        public static void CanRename(RenameAsset command, string oldName)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot rename asset.", e =>
            {
                if (string.IsNullOrWhiteSpace(command.FileName))
                {
                    e("Name is required.", nameof(command.FileName));
                }

                if (string.Equals(command.FileName, oldName))
                {
                    e("Asset has already this name.", nameof(command.FileName));
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
