// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;

namespace Squidex.Extensions.Actions.Kafka
{
    public sealed class KafkaActionHandler : RuleActionHandler<KafkaAction, KafkaJob>
    {
        private const string Description = "Push to Kafka";

        public KafkaActionHandler(RuleEventFormatter formatter)
            : base(formatter)
        {
        }

        protected override (string Description, KafkaJob Data) CreateJob(EnrichedEvent @event, KafkaAction action)
        {
            var ruleJob = new KafkaJob
            {
                Broker = action.Broker,
                TopicName = action.TopicName,
                Key = @event.Name,
                Message = ToEnvelopeJson(@event)
            };

            return (Description, ruleJob);
        }

        protected override async Task<Result> ExecuteJobAsync(KafkaJob job, CancellationToken ct = default)
        {
            try
            {
                var config = new ProducerConfig { BootstrapServers = job.Broker.AbsoluteUri };
                using (var producer = new ProducerBuilder<string, string>(config).Build())
                {
                    await producer.ProduceAsync(job.TopicName, new Message<string, string> { Key = job.Key, Value = job.Message });
                }

                return Result.Success("Event pushed to Kafka");
            }
            catch (Exception ex)
            {
                return Result.Failed(ex, "Push to Kafka failed");
            }
        }
    }

    public sealed class KafkaJob
    {
        public Uri Broker { get; set; }

        public string TopicName { get; set; }

        public string Key { get; set; }

        public string Message { get; set; }
    }
}
