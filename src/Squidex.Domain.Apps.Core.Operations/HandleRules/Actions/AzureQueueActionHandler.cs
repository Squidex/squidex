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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

#pragma warning disable SA1649 // File name must match first type name

namespace Squidex.Domain.Apps.Core.HandleRules.Actions
{
    public sealed class AzureQueueJob
    {
        public string QueueConnectionString { get; set; }
        public string QueueName { get; set; }

        public string MessageBodyV2 { get; set; }

        public JObject MessageBody { get; set; }

        public string Body
        {
            get
            {
                return MessageBodyV2 ?? MessageBody.ToString(Formatting.Indented);
            }
        }
    }

    public sealed class AzureQueueActionHandler : RuleActionHandler<AzureQueueAction, AzureQueueJob>
    {
        private readonly ClientPool<(string ConnectionString, string QueueName), CloudQueue> clients;
        private readonly RuleEventFormatter formatter;

        public AzureQueueActionHandler(RuleEventFormatter formatter)
        {
            Guard.NotNull(formatter, nameof(formatter));

            this.formatter = formatter;

            clients = new ClientPool<(string ConnectionString, string QueueName), CloudQueue>(key =>
            {
                var storageAccount = CloudStorageAccount.Parse(key.ConnectionString);

                var queueClient = storageAccount.CreateCloudQueueClient();
                var queueRef = queueClient.GetQueueReference(key.QueueName);

                return queueRef;
            });
        }

        protected override async Task<(string Description, AzureQueueJob Data)> CreateJobAsync(Envelope<AppEvent> @event, string eventName, AzureQueueAction action)
        {
            var body = formatter.ToRouteData(@event, eventName).ToString(Formatting.Indented);

            var queueName = await formatter.FormatStringAsync(action.Queue, @event);

            var ruleDescription = $"Send AzureQueueJob to azure queue '{action.Queue}'";
            var ruleJob = new AzureQueueJob
            {
                QueueConnectionString = action.ConnectionString,
                QueueName = queueName,
                MessageBodyV2 = body,
            };

            return (ruleDescription, ruleJob);
        }

        protected override async Task<(string Dump, Exception Exception)> ExecuteJobAsync(AzureQueueJob job)
        {
            var queue = clients.GetClient((job.QueueConnectionString, job.QueueName));

            await queue.AddMessageAsync(new CloudQueueMessage(job.Body));

            return ("Completed", null);
        }
    }
}
