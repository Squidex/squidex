// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avro;
using Avro.Specific;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.ICIS.Actions.Kafka.Entities;
using Squidex.Infrastructure;

namespace Squidex.ICIS.Actions.Kafka
{
    public sealed class ICISKafkaActionHandler : RuleActionHandler<ICISKafkaAction, ICISKafkaJob>
    {
        private const string DescriptionIgnore = "Ignore";
        private readonly ICISKafkaOptions kafkaOptions;

        public ICISKafkaActionHandler(RuleEventFormatter formatter, IOptions<ICISKafkaOptions> kafkaOptions)
            : base(formatter)
        {
            Guard.NotNull(kafkaOptions, nameof(kafkaOptions));
            this.kafkaOptions = kafkaOptions.Value;
        }

        protected override (string Description, ICISKafkaJob Data) CreateJob(EnrichedEvent @event, ICISKafkaAction action)
        {
            if (@event is EnrichedContentEvent contentEvent)
            {
                var ruleJob = new ICISKafkaJob
                {
                    Broker = kafkaOptions.Broker,
                    SchemaRegistry = kafkaOptions.SchemaRegistry,
                    TopicName = action.TopicName,
                    Message = contentEvent
                };

                return ("Push to Kafka", ruleJob);
            }

            return (DescriptionIgnore, new ICISKafkaJob());
        }

        protected override async Task<Result> ExecuteJobAsync(ICISKafkaJob job, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(job.TopicName))
            {
                return Result.Ignored();
            }

            try
            {
                switch (job.TopicName)
                {
                    case "Commentary":
                        using (var commentaryProducer = new KafkaProducer<Commentary>(job.TopicName, job.Broker, job.SchemaRegistry))
                        {
                            var commentaryData = (Commentary)KafkaMessageFactory.GetKafkaMessage(job.TopicName, job.Message);
                            await commentaryProducer.Send(commentaryData.Id, commentaryData);
                        }

                        break;
                    case "CommentaryType":
                        using (var commentaryTypeProducer = new KafkaProducer<CommentaryType>(job.TopicName, job.Broker, job.SchemaRegistry))
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

    public sealed class ICISKafkaJob
    {
        public string Broker { get; set; }

        public string SchemaRegistry { get; set; }

        public string TopicName { get; set; }

        public EnrichedContentEvent Message { get; set; }
    }
}
