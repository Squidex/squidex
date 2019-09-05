﻿// ==========================================================================
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
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Rules.Guards
{
    public static class GuardRule
    {
        public static Task CanCreate(CreateRule command, IAppProvider appProvider)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(() => "Cannot create rule.", async e =>
            {
                if (command.Trigger == null)
                {
                   e(Not.Defined("Trigger"), nameof(command.Trigger));
                }
                else
                {
                    var errors = await RuleTriggerValidator.ValidateAsync(command.AppId.Id, command.Trigger, appProvider);

                    errors.Foreach(x => x.AddTo(e));
                }

                if (command.Action == null)
                {
                   e(Not.Defined("Action"), nameof(command.Action));
                }
                else
                {
                    var errors = command.Action.Validate();

                    errors.Foreach(x => x.AddTo(e));
                }
            });
        }

        public static Task CanUpdate(UpdateRule command, Guid appId, IAppProvider appProvider)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(() => "Cannot update rule.", async e =>
            {
                if (command.Trigger == null && command.Action == null)
                {
                   e(Not.Defined("Either trigger or action"), nameof(command.Trigger), nameof(command.Action));
                }

                if (command.Trigger != null)
                {
                    var errors = await RuleTriggerValidator.ValidateAsync(appId, command.Trigger, appProvider);

                    errors.Foreach(x => x.AddTo(e));
                }

                if (command.Action != null)
                {
                    var errors = command.Action.Validate();

                    errors.Foreach(x => x.AddTo(e));
                }
            });
        }

        public static void CanEnable(EnableRule command, Rule rule)
        {
            Guard.NotNull(command, nameof(command));

            if (rule.IsEnabled)
            {
                throw new DomainException("Rule is already enabled.");
            }
        }

        public static void CanDisable(DisableRule command, Rule rule)
        {
            Guard.NotNull(command, nameof(command));

            if (!rule.IsEnabled)
            {
                throw new DomainException("Rule is already disabled.");
            }
        }

        public static void CanDelete(DeleteRule command)
        {
            Guard.NotNull(command, nameof(command));
        }
    }
}
