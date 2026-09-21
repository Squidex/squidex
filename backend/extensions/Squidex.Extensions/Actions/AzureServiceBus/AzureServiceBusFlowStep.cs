// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Azure.Messaging.ServiceBus;
using Squidex.Flows;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.AzureServiceBus;

[FlowStep(
    Title = "Azure Service Bus",
    IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 32 32'><path d='M.011 16L0 6.248l12-1.63V16zM14 4.328L29.996 2v14H14zM30 18l-.004 14L14 29.75V18zM12 29.495L.01 27.851.009 18H12z'/></svg>",
    IconColor = "#0d9bf9",
    Display = "Send to Azure Service Bus",
    Description = "Send an event to an azure service bus queue or topic.",
    ReadMore = "https://azure.microsoft.com/en-us/products/service-bus/")]
public sealed record AzureServiceBusFlowStep : FlowStep
{
    [LocalizedRequired]
    [Display(Name = "Connection", Description = "The connection string to the service bus namespace.")]
    [Editor(FlowStepEditor.Text)]
    [Expression]
    public string ConnectionString { get; set; }

    [LocalizedRequired]
    [Display(Name = "Queue or Topic", Description = "The name of the queue or topic.")]
    [Editor(FlowStepEditor.Text)]
    [Expression]
    public string QueueOrTopic { get; set; }

    [Display(Name = "Payload (Optional)", Description = "Leave it empty to use the full event as body.")]
    [Editor(FlowStepEditor.TextArea)]
    [Expression(ExpressionFallback.Envelope)]
    public string? Payload { get; set; }

    [Display(Name = "Subject (Optional)", Description = "The subject of the message, commonly used for filtering in subscriptions.")]
    [Editor(FlowStepEditor.Text)]
    [Expression]
    public string? Subject { get; set; }

    [Display(Name = "Session ID (Optional)", Description = "The session ID, required for session enabled queues and subscriptions.")]
    [Editor(FlowStepEditor.Text)]
    [Expression]
    public string? SessionId { get; set; }

    [Display(Name = "Message ID (Optional)", Description = "The message ID, used for duplicate detection.")]
    [Editor(FlowStepEditor.Text)]
    [Expression]
    public string? MessageId { get; set; }

    // The client owns the connection and is thread safe, senders are cheap and are created per message.
    private static readonly ClientPool<string, ServiceBusClient> Clients = new ClientPool<string, ServiceBusClient>(key =>
    {
        return new ServiceBusClient(key);
    });

    public async override ValueTask<FlowStepResult> ExecuteAsync(FlowExecutionContext executionContext,
        CancellationToken ct)
    {
        if (executionContext.IsSimulation)
        {
            executionContext.LogSkipSimulation();
            return Next();
        }

        var client = await Clients.GetClientAsync(ConnectionString);

        var message = new ServiceBusMessage(Payload);

        if (!string.IsNullOrWhiteSpace(Subject))
        {
            message.Subject = Subject;
        }

        if (!string.IsNullOrWhiteSpace(SessionId))
        {
            message.SessionId = SessionId;
        }

        if (!string.IsNullOrWhiteSpace(MessageId))
        {
            message.MessageId = MessageId;
        }

        await using var sender = client.CreateSender(QueueOrTopic);

        await sender.SendMessageAsync(message, ct);

        executionContext.Log($"Message sent to '{QueueOrTopic}'.");
        return Next();
    }
}
