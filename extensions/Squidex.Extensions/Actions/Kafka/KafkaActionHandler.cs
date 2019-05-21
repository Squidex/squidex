// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Avro;
using Avro.Specific;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;

namespace Squidex.Extensions.Actions.Kafka
{
    public sealed class KafkaActionHandler : RuleActionHandler<KafkaAction, KafkaJob>
    {
        private const string DescriptionIgnore = "Ignore";

        public KafkaActionHandler(RuleEventFormatter formatter)
            : base(formatter)
        {
        }

        protected override (string Description, KafkaJob Data) CreateJob(EnrichedEvent @event, KafkaAction action)
        {
            if (@event is EnrichedContentEvent contentEvent)
            {
                var ruleJob = new KafkaJob
                {
                    Broker = action.Broker,
                    SchemaRegistry = action.SchemaRegistry,
                    TopicName = action.TopicName,
                    Key = contentEvent.Id.ToString(),
                    Message = KafkaMessageFactory.GetKafkaMessage(action.TopicName, contentEvent.Data)
                };
                return ("Push to Kafka", ruleJob);
            }

            return (DescriptionIgnore, new KafkaJob());
        }

        protected override async Task<Result> ExecuteJobAsync(KafkaJob job, CancellationToken ct)
        {
            try
            {
                using (var kafkaProducer = KafkaProducerFactory.GetKafkaProducer(job.TopicName, job.Broker.AbsoluteUri, job.SchemaRegistry.AbsoluteUri))
                {
                    await kafkaProducer.Send(job.Key, job.Message);
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

        public Uri SchemaRegistry { get; set; }

        public string TopicName { get; set; }

        public string Key { get; set; }

        public ISpecificRecord Message { get; set; }
    }
}
