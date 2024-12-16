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
using Squidex.Flows.Execution;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Rules.DomainObject.Guards;

public static class GuardRule
{
    public static Task CanCreate(CreateRule command, IAppProvider appProvider, IFlowExecutor<RuleFlowContext> executor,
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
                await executor.ValidateAsync(command.Flow, FormatError(e), ct);
            }
        });
    }

    public static Task CanUpdate(UpdateRule command, Rule rule, IAppProvider appProvider, IFlowExecutor<RuleFlowContext> executor,
        CancellationToken ct)
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
                await executor.ValidateAsync(command.Flow, FormatError(e), ct);
            }
        });
    }

    private static AddError FormatError(AddValidation e)
    {
        return new AddError((path, type, message) =>
        {
            var property = "flow";
            if (!string.IsNullOrWhiteSpace(path))
            {
                property = $"{property}.{path}";
            }

            switch (type)
            {
                case ValidationErrorType.NoSteps:
                    e(T.Get("rules.noStep"), property);
                    break;
                case ValidationErrorType.NoStartStep:
                    e(T.Get("rules.noStartStep"), property);
                    break;
                case ValidationErrorType.InvalidNextStep:
                    e(T.Get("rules.invalidNextStep"), property);
                    break;
                case ValidationErrorType.InvalidStepId:
                    e(T.Get("rules.invalidNextStep"), property);
                    break;
                case ValidationErrorType.InvalidProperty when message != null:
                    e(message, property);
                    break;
            }
        });
    }
}
