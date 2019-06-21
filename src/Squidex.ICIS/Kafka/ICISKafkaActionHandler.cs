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
        private readonly KafkaProducer<Commentary> kafkaCommentaryProducer;
        private readonly KafkaProducer<CommentaryType> kafkaCommentaryTypeProducer;

        public ICISKafkaActionHandler(RuleEventFormatter formatter, KafkaProducer<Commentary> kafkaCommentaryProducer, KafkaProducer<CommentaryType> kafkaCommentaryTypeProducer)
            : base(formatter)
        {
            this.kafkaCommentaryProducer = kafkaCommentaryProducer;
            this.kafkaCommentaryTypeProducer = kafkaCommentaryTypeProducer;
        }

        protected override (string Description, ICISKafkaJob Data) CreateJob(EnrichedEvent @event, ICISKafkaAction action)
        {
            if (@event is EnrichedContentEvent contentEvent)
            {
                var ruleJob = new ICISKafkaJob
                {
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
                switch (job.Message.SchemaId.Name)
                {
                    case "commentary":
                        var commentaryData = (Commentary)KafkaMessageFactory.GetKafkaMessage(job.Message.SchemaId.Name, job.Message);
                        await kafkaCommentaryProducer.Send(job.TopicName, commentaryData.Id, commentaryData);
                        break;
                    case "commentary-type":
                        var commentaryTypeData = (CommentaryType)KafkaMessageFactory.GetKafkaMessage(job.Message.SchemaId.Name, job.Message);
                        await kafkaCommentaryTypeProducer.Send(job.TopicName, commentaryTypeData.Id, commentaryTypeData);
                        break;
                    default:
                        throw new Exception($"Schema {job.Message.SchemaId.Name} not configured for Kafka Integration.");
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
        public string TopicName { get; set; }

        public EnrichedContentEvent Message { get; set; }
    }
}
