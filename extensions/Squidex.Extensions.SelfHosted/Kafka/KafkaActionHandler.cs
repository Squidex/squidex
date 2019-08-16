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
using Squidex.Extensions.SelfHosted.Kafka;

namespace Squidex.Extensions.Actions.Kafka
{
    public sealed class KafkaActionHandler : RuleActionHandler<KafkaAction, KafkaJob>
    {
        private const string Description = "Push to Kafka";
        private readonly KafkaProducer kafkaProducer;

        public KafkaActionHandler(RuleEventFormatter formatter, KafkaProducer kafkaProducer)
            : base(formatter)
        {
            this.kafkaProducer = kafkaProducer;
        }

        protected override (string Description, KafkaJob Data) CreateJob(EnrichedEvent @event, KafkaAction action)
        {
            var ruleJob = new KafkaJob
            {
                TopicName = action.TopicName,
                Key = @event.Name,
                Message = ToEnvelopeJson(@event)
            };

            return (Description, ruleJob);
        }

        protected override async Task<Result> ExecuteJobAsync(KafkaJob job, CancellationToken ct)
        {
            try
            {
                await kafkaProducer.Send(job.TopicName, job.Key, job.Message);
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
        public string TopicName { get; set; }

        public string Key { get; set; }

        public string Message { get; set; }
    }
}
