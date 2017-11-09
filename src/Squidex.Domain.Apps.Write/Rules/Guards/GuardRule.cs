﻿// ==========================================================================
//  GuardRule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Domain.Apps.Write.Rules.Commands;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Rules.Guards
{
    public static class GuardRule
    {
        public static Task CanCreate(CreateRule command, ISchemaProvider schemas)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(() => "Cannot create rule.", async error =>
            {
                if (command.Trigger == null)
                {
                    error(new ValidationError("Trigger must be defined.", nameof(command.Trigger)));
                }
                else
                {
                    var errors = await RuleTriggerValidator.ValidateAsync(command.Trigger, schemas);

                    errors.Foreach(error);
                }

                if (command.Action == null)
                {
                    error(new ValidationError("Trigger must be defined.", nameof(command.Action)));
                }
                else
                {
                    var errors = await RuleActionValidator.ValidateAsync(command.Action);

                    errors.Foreach(error);
                }
            });
        }

        public static Task CanUpdate(UpdateRule command, ISchemaProvider schemas)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(() => "Cannot update rule.", async error =>
            {
                if (command.Trigger == null && command.Action == null)
                {
                    error(new ValidationError("Either trigger or action must be defined.", nameof(command.Trigger), nameof(command.Action)));
                }

                if (command.Trigger != null)
                {
                    var errors = await RuleTriggerValidator.ValidateAsync(command.Trigger, schemas);

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
