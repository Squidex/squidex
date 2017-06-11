// ==========================================================================
//  WebhookInvoker.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;
using Squidex.Read.Schemas.Repositories;

namespace Squidex.Read.Schemas
{
    public sealed class WebhookInvoker : IEventConsumer
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);

        private readonly ISchemaWebhookRepository webhookRepository;
        private readonly ISemanticLog log;
        private readonly JsonSerializer webhookSerializer;

        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "^content-"; }
        }

        public WebhookInvoker(ISchemaWebhookRepository webhookRepository, JsonSerializer webhookSerializer, ISemanticLog log)
        {
            Guard.NotNull(webhookRepository, nameof(webhookRepository));
            Guard.NotNull(webhookSerializer, nameof(webhookSerializer));
            Guard.NotNull(log, nameof(log));

            this.webhookRepository = webhookRepository;
            this.webhookSerializer = webhookSerializer;

            this.log = log;
        }

        public Task ClearAsync()
        {
            return TaskHelper.Done;
        }

        public async Task On(Envelope<IEvent> @event)
        {
            if (@event.Payload is ContentEvent contentEvent)
            {
                var hooks = await webhookRepository.QueryBySchemaAsync(contentEvent.SchemaId.Id);

                if (hooks.Count > 0)
                {
                    var payload = CreatePayload(@event);

                    foreach (var hook in hooks)
                    {
                        DispatchEventAsync(payload, hook).Forget();
                    }
                }
            }
        }

        private JObject CreatePayload(Envelope<IEvent> @event)
        {
            return new JObject(
                new JProperty("type", @event.Payload.GetType().Name),
                new JProperty("meta", JObject.FromObject(@event.Headers, webhookSerializer)),
                new JProperty("data", JObject.FromObject(@event.Headers, webhookSerializer)));
        }

        private async Task DispatchEventAsync(JObject payload, ISchemaWebhookEntity webhook)
        {
            try
            {
                using (log.MeasureInformation(w => w
                    .WriteProperty("Action", "SendToHook")
                    .WriteProperty("Status", "Invoked")))
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = Timeout;

                        var message = new HttpRequestMessage(HttpMethod.Post, webhook.Url)
                        {
                            Content = new StringContent(payload.ToString(), Encoding.UTF8, "application/json")
                        };

                        message.Headers.TryAddWithoutValidation("X-SecurityToken", webhook.SecurityToken);
                        message.Headers.Add("User-Agent", "Squidex");

                        var response = await client.SendAsync(message);

                        response.EnsureSuccessStatusCode();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("Action", "SendToHook")
                    .WriteProperty("Status", "Failed"));
            }
        }
    }
}
