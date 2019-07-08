using Microsoft.Extensions.Hosting;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.ICIS.Actions.Kafka;
using Squidex.ICIS.Actions.Kafka.Entities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.ICIS.Kafka.Consumer
{
    public sealed class CommodityConsumer : IHostedService
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly CommodityConsumerOptions options;
        private readonly IKafkaConsumer<Commodity> consumer;
        private readonly ICommandBus commandBus;
        private readonly IAppProvider appProvider;
        private readonly IContentQueryService contentQuery;
        private readonly ISemanticLog log;
        private readonly Dictionary<string, Guid> contentIds = new Dictionary<string, Guid>();
        private Task consumerTask;

        public CommodityConsumer(CommodityConsumerOptions options, IKafkaConsumer<Commodity> consumer, ICommandBus commandBus, IAppProvider appProvider, IContentQueryService contentQuery, ISemanticLog log)
        {
            this.options = options;
            this.consumer = consumer;
            this.commandBus = commandBus;
            this.appProvider = appProvider;
            this.contentQuery = contentQuery;
            this.log = log;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var app = await appProvider.GetAppAsync(options.AppName); // TODO;
            var actor = new RefToken(RefTokenType.Client, "client"); // TODO;
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            var schemaId = NamedId.Of(Guid.NewGuid(), "my-schema"); // TODO;

            var queryContext = QueryContext.Create(app, user, actor.Identifier);

            consumerTask = new Task(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        var commodity = consumer.Consume(cts.Token).Value;

                        if (!contentIds.TryGetValue(commodity.Id, out var contentId))
                        {
                            var contents = await contentQuery.QueryAsync(queryContext, schemaId.Name, Q.Empty.WithODataQuery($"data/id/iv eq '{commodity.Id}'"));
                            var contentFound = contents.FirstOrDefault();

                            if (contentFound != null)
                            {
                                contentId = contentFound.Id;
                                contentIds[commodity.Id] = contentFound.Id;
                            }
                        }

                        if (contentId != Guid.Empty)
                        {
                            await commandBus.PublishAsync(new UpdateContent
                            {
                                ContentId = contentId,
                                Actor = actor,
                                User = user,
                                Data = new NamedContentData()
                                    .AddField("id",
                                        new ContentFieldData()
                                            .AddValue(commodity.Id))
                                    .AddField("name",
                                        new ContentFieldData()
                                            .AddValue(commodity.Name))
                            });
                        }
                        else
                        {
                            var context = await commandBus.PublishAsync(new CreateContent
                            {
                                AppId = app.NamedId(),
                                SchemaId = schemaId,
                                Actor = actor,
                                User = user,
                                Publish = true,
                                Data = new NamedContentData()
                                    .AddField("id",
                                        new ContentFieldData()
                                            .AddValue(commodity.Id))
                                    .AddField("name",
                                        new ContentFieldData()
                                            .AddValue(commodity.Name))
                            });

                            var content = context.Result<IContentEntity>();

                            contentIds[commodity.Id] = content.Id;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Noop
                    }
                    catch (Exception ex)
                    { // TODO;
                    }
                }
            });

            consumerTask.Start();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            cts.Cancel();

            return consumerTask;
        }
    }
}
