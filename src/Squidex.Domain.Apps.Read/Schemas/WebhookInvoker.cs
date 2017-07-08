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
using System.Threading.Tasks.Dataflow;
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
    public sealed class WebhookInvoker : DisposableObjectBase, IEventConsumer
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);

        private readonly ISchemaWebhookRepository webhookRepository;
        private readonly ISemanticLog log;
        private readonly TypeNameRegistry typeNameRegistry;
        private readonly JsonSerializer webhookSerializer;
        private readonly TransformBlock<InvocationRequest, InvocationResponse> invokeBlock;
        private readonly ActionBlock<InvocationResponse> dumpBlock;

        private class WebhookData
        {
            public ISchemaWebhookUrlEntity Webhook;
        }

        private sealed class InvocationRequest : WebhookData
        {
            public JObject Payload;
        }

        private sealed class InvocationResponse : WebhookData
        {
            public string Dump;
            public TimeSpan Elapsed;
            public WebhookResult Result;
        }

        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "^content-"; }
        }

        public WebhookInvoker(ISchemaWebhookRepository webhookRepository, JsonSerializer webhookSerializer, ISemanticLog log, TypeNameRegistry typeNameRegistry)
        {
            Guard.NotNull(webhookRepository, nameof(webhookRepository));
            Guard.NotNull(webhookSerializer, nameof(webhookSerializer));
            Guard.NotNull(typeNameRegistry, nameof(typeNameRegistry));
            Guard.NotNull(log, nameof(log));

            this.webhookRepository = webhookRepository;
            this.webhookSerializer = webhookSerializer;

            this.log = log;

            this.typeNameRegistry = typeNameRegistry;

            invokeBlock =
                new TransformBlock<InvocationRequest, InvocationResponse>(x => DispatchEventAsync(x),
                    new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1, BoundedCapacity = 8 });

            dumpBlock =
                new ActionBlock<InvocationResponse>(DumpAsync,
                    new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 32, BoundedCapacity = 64 });

            invokeBlock.LinkTo(dumpBlock,
                new DataflowLinkOptions { PropagateCompletion = true }, x => x != null);
        }

        protected override void DisposeObject(bool disposing)
        {
            invokeBlock.Complete();

            dumpBlock.Completion.Wait();
        }

        public Task ClearAsync()
        {
            return TaskHelper.Done;
        }

        public async Task On(Envelope<IEvent> @event)
        {
            if (@event.Payload is ContentEvent contentEvent)
            {
                var webhooks = await webhookRepository.QueryUrlsBySchemaAsync(contentEvent.AppId.Id, contentEvent.SchemaId.Id);
                
                if (webhooks.Count > 0)
                {
                    var payload = CreatePayload(@event);

                    foreach (var webhook in webhooks)
                    {
                        await invokeBlock.SendAsync(new InvocationRequest { Webhook = webhook, Payload = payload });
                    }
                }
            }
        }

        private JObject CreatePayload(Envelope<IEvent> @event)
        {
            return new JObject(
                new JProperty("type", typeNameRegistry.GetName(@event.Payload.GetType())),
                new JProperty("payload", JObject.FromObject(@event.Payload, webhookSerializer)),
                new JProperty("timestamp", @event.Headers.Timestamp().ToString()));
        }

        private async Task DumpAsync(InvocationResponse input)
        {
            try
            {
                await webhookRepository.AddInvokationAsync(input.Webhook.Id, input.Dump, input.Result, input.Elapsed);
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "DumpHook")
                    .WriteProperty("status", "Failed"));
            }
        }

        private async Task<InvocationResponse> DispatchEventAsync(InvocationRequest input)
        {
            try
            {
                var payload = SignPayload(input.Payload, input.Webhook);

                var requestString = payload.ToString(Formatting.Indented);
                var responseString = string.Empty;

                var request = BuildRequest(requestString, input.Webhook);
                var response = (HttpResponseMessage)null;

                var isTimeout = false;

                var watch = Stopwatch.StartNew();
                try
                {
                    using (log.MeasureInformation(w => w
                        .WriteProperty("action", "SendToHook")
                        .WriteProperty("status", "Invoked")
                        .WriteProperty("requestUrl", request.RequestUri.ToString())))
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

                return new InvocationResponse { Dump = dump, Result = result, Elapsed = watch.Elapsed, Webhook = input.Webhook };
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "SendToHook")
                    .WriteProperty("status", "Failed"));

                return null;
            }
        }

        private static JObject SignPayload(JObject payload, ISchemaWebhookUrlEntity webhook)
        {
            payload = new JObject(payload);

            var eventTimestamp = SystemClock.Instance.GetCurrentInstant().ToUnixTimeSeconds();
            var eventSignature = $"{eventTimestamp}{webhook.SharedSecret}".Sha256Base64();

            payload["signature"] = eventSignature;

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
