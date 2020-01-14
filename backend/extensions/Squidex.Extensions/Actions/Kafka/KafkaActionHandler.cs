// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;

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
                MessageKey = @event.Name,
                MessageValue = ToEnvelopeJson(@event)
            };

            return (Description, ruleJob);
        }

        protected override async Task<Result> ExecuteJobAsync(KafkaJob job, CancellationToken ct = default)
        {
            try
            {
                await kafkaProducer.Send(job.TopicName, job.MessageKey, job.MessageValue);

                return Result.Success($"Event pushed to {job.TopicName} kafka topic.");
            }
            catch (Exception ex)
            {
                return Result.Failed(ex, "Push to Kafka failed.");
            }
        }
    }

    public sealed class KafkaJob
    {
        public string TopicName { get; set; }

        public string MessageKey { get; set; }

        public string MessageValue { get; set; }
    }
}
