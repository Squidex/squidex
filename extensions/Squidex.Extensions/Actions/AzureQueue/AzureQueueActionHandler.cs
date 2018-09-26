// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;

namespace Squidex.Extensions.Actions.AzureQueue
{
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

        protected override (string Description, AzureQueueJob Data) CreateJob(EnrichedEvent @event, AzureQueueAction action)
        {
            var queueName = Format(action.Queue, @event);

            var ruleDescription = $"Send AzureQueueJob to azure queue '{queueName}'";
            var ruleJob = new AzureQueueJob
            {
                QueueConnectionString = action.ConnectionString,
                QueueName = queueName,
                MessageBodyV2 = ToEnvelopeJson(@event)
            };

            return (ruleDescription, ruleJob);
        }

        protected override async Task<(string Dump, Exception Exception)> ExecuteJobAsync(AzureQueueJob job)
        {
            var queue = clients.GetClient((job.QueueConnectionString, job.QueueName));

            await queue.AddMessageAsync(new CloudQueueMessage(job.MessageBodyV2));

            return ("Completed", null);
        }
    }

    public sealed class AzureQueueJob
    {
        public string QueueConnectionString { get; set; }

        public string QueueName { get; set; }

        public string MessageBodyV2 { get; set; }
    }
}
