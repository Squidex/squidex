using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Shared;
using Squidex.Shared.Identity;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.ICIS.Kafka.Consumer
{
    public sealed class ConsumerService<T> : IDisposable, IKafkaConsumerService
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly ConsumerOptions options;
        private readonly IContextProvider contextProvider;
        private readonly IKafkaConsumer<T> consumer;
        private readonly IKafkaHandler<T> handler;
        private readonly IAppProvider appProvider;
        private readonly ISemanticLog log;
        private readonly RefToken actor;
        private IAppEntity app;
        private Task consumerTask;

        public ConsumerService(
            IOptions<ConsumerOptions> options,
            IContextProvider contextProvider,
            IKafkaConsumer<T> consumer,
            IKafkaHandler<T> handler,
            IAppProvider appProvider,
            ISemanticLog log)
        {
            this.options = options.Value;
            this.contextProvider = contextProvider;
            this.consumer = consumer;
            this.handler = handler;
            this.appProvider = appProvider;
            this.log = log;

            actor = new RefToken(RefTokenType.Client, options.Value.ClientName);
        }

        public void Start()
        {
            consumerTask = new Task(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        var consumed = consumer.Consume(cts.Token);

                        await EnsureAppExistsAsync();

                        var context = contextProvider.Context;

                        AddPermissions(context.User);

                        //
                        // Assign the app here and not in a method, otherwise it would not land in the context.
                        // Read more:
                        // https://stackoverflow.com/a/37309427/1229622
                        //
                        context.App = app;
                        context.UpdatePermissions();

                        await handler.HandleAsync(actor, contextProvider.Context, consumed.Key, consumed.Value);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, w => w
                            .WriteProperty("action", "createContentConsumedByKafka")
                            .WriteProperty("status", "Failed"));
                    }
                }
            }, TaskCreationOptions.LongRunning);

            consumerTask.Start();
        }

        private void AddPermissions(ClaimsPrincipal principal)
        {
            var user = (ClaimsIdentity)principal.Identity;

            user.AddClaim(new Claim(SquidexClaimTypes.Permissions, Permissions.All));
        }

        private async Task EnsureAppExistsAsync()
        {
            if (app == null)
            {
                app = await appProvider.GetAppAsync(options.AppName);

                if (app == null)
                {
                    throw new InvalidOperationException($"Cannot find app with name '{options.AppName}'");
                }
            }
        }

        public void Dispose()
        {
            cts.Cancel();
            cts.Dispose();

            consumerTask.Wait();
        }
    }
}