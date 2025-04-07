// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Flows;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Rules.DomainObject.Guards;

public static class GuardRule
{
    public static Task CanCreate(CreateRule command, IAppProvider appProvider, IFlowManager<FlowEventContext> flowManager,
        CancellationToken ct)
    {
        Guard.NotNull(command);

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

            if (command.Flow == null)
            {
                e(Not.Defined(nameof(command.Flow)), nameof(command.Flow));
            }
            else
            {
                await flowManager.ValidateAsync(command.Flow, (path, type, message) =>
                {
                }, ct);
                var errors = command.Action.Validate();

                errors.Foreach((x, _) => x.AddTo(e));
            }
        });
    }

    public static Task CanUpdate(UpdateRule command, Rule rule, IAppProvider appProvider, IFlowManager<FlowEventContext> flowManager)
    {
        Guard.NotNull(command);

        return Validate.It(async e =>
        {
            if (command.Trigger != null)
            {
                var errors = await RuleTriggerValidator.ValidateAsync(rule.AppId.Id, command.Trigger, appProvider);

                errors.Foreach((x, _) => x.AddTo(e));
            }

            if (command.Flow != null)
            {
                var errors = command.Action.Validate();

                errors.Foreach((x, _) => x.AddTo(e));
            }
        });
    }

    private AddValidation 
}
