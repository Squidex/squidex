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
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Extensions.Actions.Kafka.Entities;

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
                    Message = contentEvent.Data
                };
                return ("Push to Kafka", ruleJob);
            }

            return (DescriptionIgnore, new KafkaJob());
        }

        protected override async Task<Result> ExecuteJobAsync(KafkaJob job, CancellationToken ct)
        {
            try
            {
                switch (job.TopicName)
                {
                    case "Commentary":
                        using (var commentaryProducer = new KafkaProducer<Commentary>(job.TopicName, job.Broker.AbsoluteUri, job.SchemaRegistry.AbsoluteUri))
                        {
                            var commentaryData = (Commentary)KafkaMessageFactory.GetKafkaMessage(job.TopicName, job.Message);
                            commentaryData.Id = job.Key;
                            await commentaryProducer.Send(commentaryData.Id, commentaryData);
                        }

                        break;
                    case "CommentaryType":
                        using (var commentaryTypeProducer = new KafkaProducer<CommentaryType>(job.TopicName, job.Broker.AbsoluteUri, job.SchemaRegistry.AbsoluteUri))
                        {
                            var commentaryTypeData = (CommentaryType)KafkaMessageFactory.GetKafkaMessage(job.TopicName, job.Message);
                            await commentaryTypeProducer.Send(commentaryTypeData.Id, commentaryTypeData);
                        }

                        break;
                    default:
                        throw new Exception("kafka Topic not configured.");
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

        public NamedContentData Message { get; set; }
    }
}
