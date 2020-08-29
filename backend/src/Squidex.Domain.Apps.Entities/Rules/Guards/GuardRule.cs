// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Rules.Guards
{
    public static class GuardRule
    {
        public static Task CanCreate(CreateRule command, IAppProvider appProvider)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(async e =>
            {
                if (command.Trigger == null)
                {
                    e(Not.Defined(nameof(command.Trigger)), nameof(command.Trigger));
                }
                else
                {
                    var errors = await RuleTriggerValidator.ValidateAsync(command.AppId.Id, command.Trigger, appProvider);

                    errors.Foreach((x, _) => x.AddTo(e));
                }

                if (command.Action == null)
                {
                    e(Not.Defined(nameof(command.Action)), nameof(command.Action));
                }
                else
                {
                    var errors = command.Action.Validate();

                    errors.Foreach((x, _) => x.AddTo(e));
                }
            });
        }

        public static Task CanUpdate(UpdateRule command, DomainId appId, IAppProvider appProvider)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(async e =>
            {
                if (command.Trigger != null)
                {
                    var errors = await RuleTriggerValidator.ValidateAsync(appId, command.Trigger, appProvider);

                    errors.Foreach((x, _) => x.AddTo(e));
                }

                if (command.Action != null)
                {
                    var errors = command.Action.Validate();

                    errors.Foreach((x, _) => x.AddTo(e));
                }
            });
        }

        public static void CanEnable(EnableRule command)
        {
            Guard.NotNull(command, nameof(command));
        }

        public static void CanDisable(DisableRule command)
        {
            Guard.NotNull(command, nameof(command));
        }

        public static void CanDelete(DeleteRule command)
        {
            Guard.NotNull(command, nameof(command));
        }
    }
}
