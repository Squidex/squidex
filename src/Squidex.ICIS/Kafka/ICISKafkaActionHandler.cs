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
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.ICIS.Kafka.Entities;
using Squidex.ICIS.Kafka.Producer;
using Squidex.ICIS.Kafka.Services;

namespace Squidex.ICIS.Kafka
{
    public sealed class ICISKafkaActionHandler : RuleActionHandler<ICISKafkaAction, ICISKafkaJob>
    {
        private const string DescriptionIgnore = "Ignore";
        private readonly IKafkaProducer<Commentary> kafkaCommentaryProducer;
        private readonly IKafkaProducer<CommentaryType> kafkaCommentaryTypeProducer;
        private readonly IAppProvider appProvider;
        private readonly IContentRepository contentRepository;

        public ICISKafkaActionHandler(RuleEventFormatter formatter,
            IKafkaProducer<Commentary> kafkaCommentaryProducer,
            IKafkaProducer<CommentaryType> kafkaCommentaryTypeProducer,
            IAppProvider appProvider, 
            IContentRepository contentRepository)
            : base(formatter)
        {
            this.kafkaCommentaryProducer = kafkaCommentaryProducer;
            this.kafkaCommentaryTypeProducer = kafkaCommentaryTypeProducer;
            this.appProvider = appProvider;
            this.contentRepository = contentRepository;
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
                var app = await appProvider.GetAppAsync(job.Message.AppId.Id);

                switch (job.Message.SchemaId.Name)
                {
                    case "commentary":
                        var commentaryData = await CommentaryMapper.ToAvroAsync(job.Message, app, contentRepository);
                        await kafkaCommentaryProducer.Send(job.TopicName, commentaryData.Id, commentaryData);
                        break;
                    case "commentary-type":
                        var commentaryTypeData = CommentaryTypeMapper.ToAvro(job.Message);
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
