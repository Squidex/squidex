// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Microsoft.WindowsAzure.Storage.Queue;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Flows;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.AzureQueue;

public sealed class AzureQueueStep : RuleFlowStep<EnrichedEvent>
{
    [LocalizedRequired]
    [Display(Name = "Connection", Description = "The connection string to the storage account.")]
    [Editor(FlowStepEditor.Text)]
    [Expression]
    public string ConnectionString { get; set; }

    [LocalizedRequired]
    [Display(Name = "Queue", Description = "The name of the queue.")]
    [Editor(FlowStepEditor.Text)]
    [Expression]
    public string Queue { get; set; }

    [Display(Name = "Payload (Optional)", Description = "Leave it empty to use the full event as body.")]
    [Editor(FlowStepEditor.Text)]
    [Expression(ExpressionFallback.Envelope)]
    public string? Payload { get; set; }

    protected override async ValueTask<FlowStepResult> ExecuteAsync(RuleFlowContext context, EnrichedEvent @event, FlowExecutionContext executionContext,
        CancellationToken ct)
    {
        var queuePool = executionContext.Resolve<AzureQueuePool>();
        var queueClient = await queuePool.GetClientAsync((ConnectionString, Queue));

        await queueClient.AddMessageAsync(new CloudQueueMessage(Payload), null, null, null, null, ct);

        return FlowStepResult.Next();
    }
}
