// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Flows;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.AzureQueue;

[FlowStep(
    Title = "Azure Queue",
    IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 32 32'><path d='M.011 16L0 6.248l12-1.63V16zM14 4.328L29.996 2v14H14zM30 18l-.004 14L14 29.75V18zM12 29.495L.01 27.851.009 18H12z'/></svg>",
    IconColor = "#0d9bf9",
    Display = "Send to Azure Queue",
    Description = "Send an event to azure queue storage.",
    ReadMore = "https://azure.microsoft.com/en-us/services/storage/queues/")]
#pragma warning disable CS0618 // Type or member is obsolete
public sealed partial record AzureQueueFlowStep : FlowStep, IConvertibleToAction
#pragma warning restore CS0618 // Type or member is obsolete
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
    [Editor(FlowStepEditor.TextArea)]
    [Expression(ExpressionFallback.Envelope)]
    public string? Payload { get; set; }

    private static readonly ClientPool<(string ConnectionString, string QueueName), CloudQueue> Clients = new ClientPool<(string ConnectionString, string QueueName), CloudQueue>(key =>
    {
        var storageAccount = CloudStorageAccount.Parse(key.ConnectionString);

        var queueClient = storageAccount.CreateCloudQueueClient();
        var queueRef = queueClient.GetQueueReference(key.QueueName);

        return queueRef;
    });

    public override ValueTask ValidateAsync(FlowValidationContext validationContext, AddStepError addError,
        CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(Queue) && !QueueNameRegex().IsMatch(Queue))
        {
            addError(nameof(Queue), "Queue must be valid azure queue name.");
        }

        return base.ValidateAsync(validationContext, addError, ct);
    }

    public async override ValueTask<FlowStepResult> ExecuteAsync(FlowExecutionContext executionContext,
        CancellationToken ct)
    {
        if (executionContext.IsSimulation)
        {
            executionContext.LogSkipSimulation();
            return Next();
        }

        var queue = await Clients.GetClientAsync((ConnectionString, Queue));

        await queue.AddMessageAsync(new CloudQueueMessage(Payload), null, null, null, null, ct);
        return Next();
    }

#pragma warning disable CS0618 // Type or member is obsolete
    public RuleAction ToAction()
    {
        return SimpleMapper.Map(this, new AzureQueueAction());
    }
#pragma warning restore CS0618 // Type or member is obsolete

    [GeneratedRegex("^[a-z][a-z0-9]{2,}(\\-[a-z0-9]+)*$")]
    private static partial Regex QueueNameRegex();
}
