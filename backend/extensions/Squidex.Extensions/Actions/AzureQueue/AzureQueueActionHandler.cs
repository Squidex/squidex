// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Extensions.Actions.AzureQueue;

public sealed class AzureQueueActionHandler : RuleActionHandler<AzureQueueAction, AzureQueueJob>
{
    private readonly ClientPool<(string ConnectionString, string QueueName), CloudQueue> clients;

    public AzureQueueActionHandler(RuleEventFormatter formatter)
        : base(formatter)
    {
        clients = new ClientPool<(string ConnectionString, string QueueName), CloudQueue>(key =>
        {
            var storageAccount = CloudStorageAccount.Parse(key.ConnectionString);

            var queueClient = storageAccount.CreateCloudQueueClient();
            var queueRef = queueClient.GetQueueReference(key.QueueName);

            return queueRef;
        });
    }

    protected override async Task<(string Description, AzureQueueJob Data)> CreateJobAsync(EnrichedEvent @event, AzureQueueAction action)
    {
        var queueName = await FormatAsync(action.Queue, @event);

        string requestBody;

        if (!string.IsNullOrEmpty(action.Payload))
        {
            requestBody = await FormatAsync(action.Payload, @event);
        }
        else
        {
            requestBody = ToEnvelopeJson(@event);
        }

        var ruleDescription = $"Send AzureQueueJob to azure queue '{queueName}'";
        var ruleJob = new AzureQueueJob
        {
            QueueConnectionString = action.ConnectionString,
            QueueName = queueName,
            MessageBodyV2 = requestBody
        };

        return (ruleDescription, ruleJob);
    }

    protected override async Task<Result> ExecuteJobAsync(AzureQueueJob job,
        CancellationToken ct = default)
    {
        var queue = await clients.GetClientAsync((job.QueueConnectionString, job.QueueName));

        await queue.AddMessageAsync(new CloudQueueMessage(job.MessageBodyV2), null, null, null, null, ct);

        return Result.Complete();
    }
}

public sealed class AzureQueueJob
{
    public string QueueConnectionString { get; set; }

    public string QueueName { get; set; }

    public string MessageBodyV2 { get; set; }
}
