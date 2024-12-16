// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Flows;

namespace Squidex.Domain.Apps.Core.HandleRules;

public abstract class RuleFlowStep<TEvent> : IFlowStep
{
    public virtual ValueTask ValidateAsync(FlowValidationContext validationContext, AddError addError,
        CancellationToken ct)
    {
        return default;
    }

    public ValueTask PrepareAsync(FlowContext context, FlowExecutionContext executionContext,
        CancellationToken ct)
    {
        var ctx = (RuleFlowContext)context;

        return PrepareAsync(ctx, ctx.Event, executionContext, ct);
    }

    protected virtual ValueTask PrepareAsync(RuleFlowContext context, EnrichedEvent @event, FlowExecutionContext executionContext,
        CancellationToken ct)
    {
        return default;
    }

    public async ValueTask<FlowStepResult> ExecuteAsync(FlowContext context, FlowExecutionContext executionContext,
        CancellationToken ct)
    {
        if (executionContext.IsSimulation)
        {
            return FlowStepResult.Next();
        }

        var ctx = (RuleFlowContext)context;

        if (ctx.Event is not TEvent @event)
        {
            executionContext.Log($"Unexpected event, expected {typeof(TEvent).Name}, got {ctx.Event.GetType().Name}");
            return FlowStepResult.Next();
        }

        return await ExecuteAsync(ctx, @event, executionContext, ct);
    }

    protected abstract ValueTask<FlowStepResult> ExecuteAsync(RuleFlowContext context, TEvent @event, FlowExecutionContext executionContext,
        CancellationToken ct);
}
