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
using Squidex.Domain.Apps.Entities.Apps;
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
        private readonly KafkaProducer<Commodity> kafkaCommodityProducer;
        private readonly KafkaProducer<Region> kafkaRegionProducer;
        private readonly IAppProvider appProvider;
        private readonly Dictionary<string, IAppEntity> commentaryAppDic = new Dictionary<string, IAppEntity>();
        private readonly IContentRepository contentRepository;

        public ICISKafkaActionHandler(RuleEventFormatter formatter, 
            KafkaProducer<Commentary> kafkaCommentaryProducer, 
            KafkaProducer<CommentaryType> kafkaCommentaryTypeProducer, 
            KafkaProducer<Commodity> kafkaCommodityProducer,
            KafkaProducer<Region> kafkaRegionProducer,
            IAppProvider appProvider, 
            IContentRepository contentRepository)
            : base(formatter)
        {
            this.kafkaCommentaryProducer = kafkaCommentaryProducer;
            this.kafkaCommentaryTypeProducer = kafkaCommentaryTypeProducer;
            this.kafkaCommodityProducer = kafkaCommodityProducer;
            this.kafkaRegionProducer = kafkaRegionProducer;
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
                if (!commentaryAppDic.ContainsKey("commentary"))
                {
                    var app = appProvider.GetAppAsync("commentary");
                    app.Wait();
                    commentaryAppDic.Add("commentary", app.Result);
                }

                switch (job.Message.SchemaId.Name)
                {
                    case "commentary":
                        var commentaryData = (Commentary)KafkaMessageFactory.GetKafkaMessage(job.Message.SchemaId.Name, job.Message, commentaryAppDic["commentary"], contentRepository);
                        await kafkaCommentaryProducer.Send(job.TopicName, commentaryData.Id, commentaryData);
                        break;
                    case "commentary-type":
                        var commentaryTypeData = (CommentaryType)KafkaMessageFactory.GetKafkaMessage(job.Message.SchemaId.Name, job.Message, commentaryAppDic["commentary"], contentRepository);
                        await kafkaCommentaryTypeProducer.Send(job.TopicName, commentaryTypeData.Id, commentaryTypeData);
                        break;
                    case "commodity":
                        var commodityData = (Commodity)KafkaMessageFactory.GetKafkaMessage(job.Message.SchemaId.Name, job.Message, commentaryAppDic["commentary"], contentRepository);
                        await kafkaCommodityProducer.Send(job.TopicName, commodityData.Id, commodityData);
                        break;
                    case "region":
                        var regionData = (Region)KafkaMessageFactory.GetKafkaMessage(job.Message.SchemaId.Name, job.Message, commentaryAppDic["commentary"], contentRepository);
                        await kafkaRegionProducer.Send(job.TopicName, regionData.Id, regionData);
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
