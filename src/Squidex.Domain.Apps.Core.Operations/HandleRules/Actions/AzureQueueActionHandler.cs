// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Core.HandleRules.Actions
{
    public sealed class AzureQueueActionHandler : RuleActionHandler<AzureQueueAction>
    {
        private readonly ConcurrentDictionary<(string ConnectionString, string QueueName), CloudQueue> queues = new ConcurrentDictionary<(string ConnectionString, string QueueName), CloudQueue>();
        private readonly RuleEventFormatter formatter;

        public AzureQueueActionHandler(RuleEventFormatter formatter)
        {
            Guard.NotNull(formatter, nameof(formatter));

            this.formatter = formatter;
        }

        protected override (string Description, RuleJobData Data) CreateJob(Envelope<AppEvent> @event, string eventName, AzureQueueAction action)
        {
            var body = formatter.ToRouteData(@event, eventName);

            var ruleDescription = $"Send event to azure queue '{action.Queue}'";
            var ruleData = new RuleJobData
            {
                ["QueueConnectionString"] = action.ConnectionString,
                ["QueueName"] = action.Queue,
                ["MessageBody"] = body
            };

            return (ruleDescription, ruleData);
        }

        public override async Task<(string Dump, Exception Exception)> ExecuteJobAsync(RuleJobData job)
        {
            var queueConnectionString = job["QueueConnectionString"].Value<string>();
            var queueName = job["QueueName"].Value<string>();

            var queue = queues.GetOrAdd((queueConnectionString, queueName), s =>
            {
                var storageAccount = CloudStorageAccount.Parse(queueConnectionString);

                var queueClient = storageAccount.CreateCloudQueueClient();
                var queueRef = queueClient.GetQueueReference(queueName);

                return queueRef;
            });

            var messageBody = job["MessageBody"].ToString(Formatting.Indented);

            await queue.AddMessageAsync(new CloudQueueMessage(messageBody));

            return ("Completed", null);
        }
    }
}
