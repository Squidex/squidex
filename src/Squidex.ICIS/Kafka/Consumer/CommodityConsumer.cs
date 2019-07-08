using Microsoft.Extensions.Hosting;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.ICIS.Actions.Kafka;
using Squidex.ICIS.Actions.Kafka.Entities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.ICIS.Kafka.Consumer
{
    public sealed class CommodityConsumer : IHostedService
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly IKafkaConsumer<Commodity> consumer;
        private readonly ICommandBus commandBus;
        private readonly IAppProvider appProvider;
        private Task consumerTask;

        public CommodityConsumer(IKafkaConsumer<Commodity> consumer, ICommandBus commandBus, IAppProvider appProvider)
        {
            this.consumer = consumer;
            this.commandBus = commandBus;
            this.appProvider = appProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            consumerTask = new Task(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        var commodity = consumer.Consume(cts.Token);

                        await commandBus.PublishAsync(new CreateContent
                        {
                            AppId = NamedId.Of(Guid.NewGuid(), "my-app"), // TODO
                            SchemaId = NamedId.Of(Guid.NewGuid(), "my-schema"), // TODO
                            Actor = new RefToken(RefTokenType.Client, "client"),
                            User = new ClaimsPrincipal(new ClaimsIdentity()),
                            Publish = true,
                            Data = new NamedContentData()
                                .AddField("id", 
                                    new ContentFieldData()
                                        .AddValue(commodity.Value.Id))
                                .AddField("name",
                                    new ContentFieldData()
                                        .AddValue(commodity.Value.Name))
                        });
                    }
                    catch (OperationCanceledException)
                    {
                        // Noop
                    }
                }
            });

            consumerTask.Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            cts.Cancel();

            return consumerTask;
        }
    }
}
