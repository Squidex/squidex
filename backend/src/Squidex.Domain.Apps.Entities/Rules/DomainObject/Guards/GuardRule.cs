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
using Squidex.Infrastructure.Translations;
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
                var errors = await RuleTriggerValidator.ValidateAsync(command.AppId.Id, command.Trigger, appProvider, ct);

                errors.Foreach((x, _) => x.AddTo(e));
            }

            if (command.Flow == null)
            {
                e(Not.Defined(nameof(command.Flow)), nameof(command.Flow));
            }
            else
            {
                await flowManager.ValidateAsync(command.Flow, ConvertError(e), ct);
            }
        });
    }

    public static Task CanUpdate(UpdateRule command, Rule rule, IAppProvider appProvider, IFlowManager<FlowEventContext> flowManager,
        CancellationToken ct)
    {
        Guard.NotNull(command);

        return Validate.It(async e =>
        {
            if (command.Trigger != null)
            {
                var errors = await RuleTriggerValidator.ValidateAsync(rule.AppId.Id, command.Trigger, appProvider, ct);

                errors.Foreach((x, _) => x.AddTo(e));
            }

            if (command.Flow != null)
            {
                await flowManager.ValidateAsync(command.Flow, ConvertError(e), ct);
            }
        });
    }

    private static AddError ConvertError(AddValidation addError)
    {
        return new AddError((path, type, message) =>
        {
            var actualPath = nameof(CreateRule.Flow);
            if (!string.IsNullOrWhiteSpace(path))
            {
                actualPath = $"{actualPath}.{path}";
            }

            switch (type)
            {
                case ValidationErrorType.NoSteps:
                    addError(T.Get("rules.validation.noSteps"), actualPath);
                    break;
                case ValidationErrorType.NoStartStep:
                    addError(T.Get("rules.validation.noStartStep"), actualPath);
                    break;
                case ValidationErrorType.InvalidNextStepId:
                    addError(T.Get("rules.validation.invalidNextStepId"), actualPath);
                    break;
                case ValidationErrorType.InvalidStepId:
                    addError(T.Get("rules.validation.invalidStepId"), actualPath);
                    break;
                case ValidationErrorType.InvalidProperty when !string.IsNullOrWhiteSpace(message):
                    addError(message, actualPath);
                    break;
            }
        });
    }
}
