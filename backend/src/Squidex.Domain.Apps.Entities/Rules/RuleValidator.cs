// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Flows;
using Squidex.Flows.CronJobs;
using Squidex.Flows.Internal;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Rules;

public sealed class RuleValidator(
    IFlowManager<FlowEventContext> flowManager,
    IFlowCronJobManager<CronJobContext> flowCronJobs,
    IAppProvider appProvider)
    : IRuleValidator
{
    public async Task ValidateTriggerAsync(RuleTrigger trigger, DomainId appId, AddValidation addError,
        CancellationToken ct = default)
    {
        Guard.NotNull(trigger);
        Guard.NotNull(addError);

        var context = new TriggerValidationContext(appId, addError, appProvider, flowCronJobs, ct);

        await trigger.Accept(RuleTriggerValidator.Instance, context);
    }

    public async Task ValidateFlowAsync(FlowDefinition flow, AddValidation addError,
        CancellationToken ct = default)
    {
        Guard.NotNull(flow);
        Guard.NotNull(addError);

        await flowManager.ValidateAsync(flow, ConvertError(addError), ct);
    }

    public async Task ValidateStepAsync(FlowStep step, AddValidation addError,
        CancellationToken ct = default)
    {
        Guard.NotNull(step);
        Guard.NotNull(addError);

        await flowManager.ValidateAsync(step, ConvertError(addError), ct);
    }

    private static AddError ConvertError(AddValidation addError)
    {
        return new AddError((path, type, message) =>
        {
            switch (type)
            {
                case ValidationErrorType.NoSteps:
                    addError(T.Get("rules.validation.noSteps"), path);
                    break;
                case ValidationErrorType.NoStartStep:
                    addError(T.Get("rules.validation.noStartStep"), path);
                    break;
                case ValidationErrorType.InvalidNextStepId:
                    addError(T.Get("rules.validation.invalidNextStepId"), path);
                    break;
                case ValidationErrorType.InvalidStepId:
                    addError(T.Get("rules.validation.invalidStepId"), path);
                    break;
                case ValidationErrorType.InvalidProperty when !string.IsNullOrWhiteSpace(message):
                    addError(message, path);
                    break;
            }
        });
    }

    private sealed class RuleTriggerValidator : IRuleTriggerVisitor<ValueTask<None>, TriggerValidationContext>
    {
        public static readonly RuleTriggerValidator Instance = new RuleTriggerValidator();

        public ValueTask<None> Visit(CommentTrigger trigger, TriggerValidationContext args)
        {
            return default;
        }

        public ValueTask<None> Visit(AssetChangedTriggerV2 trigger, TriggerValidationContext args)
        {
            return default;
        }

        public ValueTask<None> Visit(ManualTrigger trigger, TriggerValidationContext args)
        {
            return default;
        }

        public ValueTask<None> Visit(SchemaChangedTrigger trigger, TriggerValidationContext args)
        {
            return default;
        }

        public ValueTask<None> Visit(UsageTrigger trigger, TriggerValidationContext args)
        {
            if (trigger.NumDays is < 1 or > 30)
            {
                args.AddError(Not.Between(nameof(trigger.NumDays), 1, 30), nameof(trigger.NumDays));
            }

            return default;
        }

        public ValueTask<None> Visit(CronJobTrigger trigger, TriggerValidationContext args)
        {
            if (!args.CronJobs.IsValidCronExpression(trigger.CronExpression))
            {
                args.AddError(T.Get("rules.validation.invalidCronExpression"), nameof(trigger.CronExpression));
            }

            if (!args.CronJobs.IsValidTimezone(trigger.CronTimezone))
            {
                args.AddError(T.Get("rules.validation.invalidCronTimezone"), nameof(trigger.CronTimezone));
            }

            return default;
        }

        public async ValueTask<None> Visit(ContentChangedTriggerV2 trigger, TriggerValidationContext args)
        {
            if (trigger.Schemas == null)
            {
                return None.Value;
            }

            foreach (var schema in trigger.Schemas)
            {
                if (schema.SchemaId == DomainId.Empty)
                {
                    args.AddError(Not.Defined("SchemaId"), nameof(trigger.Schemas));
                }
                else if (await args.AppProvider.GetSchemaAsync(args.AppId, schema.SchemaId, false, args.CancellationToken) == null)
                {
                    args.AddError(T.Get("rules.validation.schemaNotFound", new { id = schema.SchemaId }), nameof(trigger.Schemas));
                }
            }

            return None.Value;
        }
    }

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it
    private record struct TriggerValidationContext(
        DomainId AppId,
        AddValidation AddError,
        IAppProvider AppProvider,
        IFlowCronJobManager<CronJobContext> CronJobs,
        CancellationToken CancellationToken);
#pragma warning restore RECS0082 // Parameter has the same name as a member and hides it
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
}
