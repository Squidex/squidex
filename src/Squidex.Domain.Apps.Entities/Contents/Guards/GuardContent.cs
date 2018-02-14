// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Guards
{
    public static class GuardContent
    {
        public static void CanCreate(CreateContent command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot created content.", error =>
            {
                if (command.Data == null)
                {
                    error(new ValidationError("Data cannot be null.", nameof(command.Data)));
                }
            });
        }

        public static void CanUpdate(UpdateContent command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot update content.", error =>
            {
                if (command.Data == null)
                {
                    error(new ValidationError("Data cannot be null.", nameof(command.Data)));
                }
            });
        }

        public static void CanPatch(PatchContent command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot patch content.", error =>
            {
                if (command.Data == null)
                {
                    error(new ValidationError("Data cannot be null.", nameof(command.Data)));
                }
            });
        }

        public static void CanChangeContentStatus(Status status, ChangeContentStatus command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot change status.", error =>
            {
                if (!StatusFlow.Exists(command.Status) || !StatusFlow.CanChange(status, command.Status))
                {
                    error(new ValidationError($"Content cannot be changed from status {status} to {command.Status}.", nameof(command.Status)));
                }

                if (command.DueTime.HasValue && command.DueTime.Value < SystemClock.Instance.GetCurrentInstant())
                {
                    error(new ValidationError("DueTime must be in the future.", nameof(command.DueTime)));
                }
            });
        }

        public static void CanDelete(DeleteContent command)
        {
            Guard.NotNull(command, nameof(command));
        }
    }
}
