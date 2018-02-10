// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules.Guards
{
    public static class GuardRule
    {
        public static Task CanCreate(CreateRule command, IAppProvider appProvider)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(() => "Cannot create rule.", async error =>
            {
                if (command.Trigger == null)
                {
                    error(new ValidationError("Trigger is required.", nameof(command.Trigger)));
                }
                else
                {
                    var errors = await RuleTriggerValidator.ValidateAsync(command.AppId.Id, command.Trigger, appProvider);

                    errors.Foreach(error);
                }

                if (command.Action == null)
                {
                    error(new ValidationError("Trigger is required.", nameof(command.Action)));
                }
                else
                {
                    var errors = await RuleActionValidator.ValidateAsync(command.Action);

                    errors.Foreach(error);
                }
            });
        }

        public static Task CanUpdate(UpdateRule command, Guid appId, IAppProvider appProvider)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(() => "Cannot update rule.", async error =>
            {
                if (command.Trigger == null && command.Action == null)
                {
                    error(new ValidationError("Either trigger or action is required.", nameof(command.Trigger), nameof(command.Action)));
                }

                if (command.Trigger != null)
                {
                    var errors = await RuleTriggerValidator.ValidateAsync(appId, command.Trigger, appProvider);

                    errors.Foreach(error);
                }

                if (command.Action != null)
                {
                    var errors = await RuleActionValidator.ValidateAsync(command.Action);

                    errors.Foreach(error);
                }
            });
        }

        public static void CanEnable(EnableRule command, Rule rule)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot enable rule.", error =>
            {
                if (rule.IsEnabled)
                {
                    error(new ValidationError("Rule is already enabled."));
                }
            });
        }

        public static void CanDisable(DisableRule command, Rule rule)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot disable rule.", error =>
            {
                if (!rule.IsEnabled)
                {
                    error(new ValidationError("Rule is already disabled."));
                }
            });
        }

        public static void CanDelete(DeleteRule command)
        {
            Guard.NotNull(command, nameof(command));
        }
    }
}
