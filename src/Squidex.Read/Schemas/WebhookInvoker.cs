// ==========================================================================
//  WebhookInvoker.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;
using Squidex.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Http;
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
                var hooks = await webhookRepository.QueryUrlsBySchemaAsync(contentEvent.AppId.Id, contentEvent.SchemaId.Id);
                
                if (hooks.Count > 0)
                {
                    var payload = CreatePayload(@event);

                    foreach (var hook in hooks)
                    {
                        DispatchEventAsync(payload, hook, @event.Headers.Timestamp()).Forget();
                    }
                }
            }
        }

        private JObject CreatePayload(Envelope<IEvent> @event)
        {
            return new JObject(
                new JProperty("type", @event.Payload.GetType().Name),
                new JProperty("payload", JObject.FromObject(@event.Payload, webhookSerializer)),
                new JProperty("timestamp", @event.Headers.Timestamp().ToString()));
        }

        private async Task DispatchEventAsync(JObject payload, ISchemaWebhookUrlEntity webhook, Instant instant)
        {
            try
            {
                payload = SignPayload(payload, webhook, instant);

                var requestString = payload.ToString(Formatting.Indented);
                var responseString = string.Empty;

                var request = BuildRequest(requestString, webhook);
                var response = (HttpResponseMessage)null;

                var isTimeout = false;

                var watch = Stopwatch.StartNew();
                try
                {
                    using (log.MeasureInformation(w => w
                        .WriteProperty("Action", "SendToHook")
                        .WriteProperty("Status", "Invoked")
                        .WriteProperty("RequestUrl", request.RequestUri.ToString())))
                    {
                        using (var client = new HttpClient { Timeout = Timeout })
                        {
                            response = await client.SendAsync(request);
                        }
                    }
                }
                catch (TimeoutException)
                {
                    isTimeout = true;
                }
                catch (OperationCanceledException)
                {
                    isTimeout = true;
                }
                finally
                {
                    watch.Stop();
                }

                if (response != null)
                {
                    responseString = await response.Content.ReadAsStringAsync();
                }

                var dump = DumpFormatter.BuildDump(request, response, requestString, responseString, watch.Elapsed);

                var result = WebhookResult.Fail;

                if (isTimeout)
                {
                    result = WebhookResult.Timeout;
                }
                else if (response?.IsSuccessStatusCode == true)
                {
                    result = WebhookResult.Success;
                }

                await webhookRepository.AddInvokationAsync(webhook.Id, dump, result, watch.Elapsed);
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("Action", "SendToHook")
                    .WriteProperty("Status", "Failed"));
            }
        }

        private static JObject SignPayload(JObject payload, ISchemaWebhookUrlEntity webhook, Instant instant)
        {
            payload["signature"] = $"{instant.ToUnixTimeSeconds()}{webhook.SharedSecret}".Sha256Base64();

            return payload;
        }

        private static HttpRequestMessage BuildRequest(string requestBody, ISchemaWebhookUrlEntity webhook)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, webhook.Url)
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };

            return request;
        }
    }
}
