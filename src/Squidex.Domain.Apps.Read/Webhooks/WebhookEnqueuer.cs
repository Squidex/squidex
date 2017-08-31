// ==========================================================================
//  WebhookEnqueuer.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;
using Squidex.Domain.Apps.Core.Webhooks;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Apps.Read.Webhooks.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Read.Webhooks
{
    public sealed class WebhookEnqueuer : IEventConsumer
    {
        private const string ContentPrefix = "Content";
        private static readonly Duration ExpirationTime = Duration.FromDays(2);
        private readonly IWebhookEventRepository webhookEventRepository;
        private readonly IWebhookRepository webhookRepository;
        private readonly IClock clock;
        private readonly TypeNameRegistry typeNameRegistry;
        private readonly JsonSerializer webhookSerializer;

        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "^content-"; }
        }

        public WebhookEnqueuer(TypeNameRegistry typeNameRegistry,
            IWebhookEventRepository webhookEventRepository,
            IWebhookRepository webhookRepository,
            IClock clock,
            JsonSerializer webhookSerializer)
        {
            Guard.NotNull(webhookEventRepository, nameof(webhookEventRepository));
            Guard.NotNull(webhookSerializer, nameof(webhookSerializer));
            Guard.NotNull(webhookRepository, nameof(webhookRepository));
            Guard.NotNull(typeNameRegistry, nameof(typeNameRegistry));
            Guard.NotNull(clock, nameof(clock));

            this.webhookEventRepository = webhookEventRepository;
            this.webhookSerializer = webhookSerializer;
            this.webhookRepository = webhookRepository;

            this.clock = clock;

            this.typeNameRegistry = typeNameRegistry;
        }

        public Task ClearAsync()
        {
            return TaskHelper.Done;
        }

        public async Task On(Envelope<IEvent> @event)
        {
            if (@event.Payload is ContentEvent contentEvent)
            {
                var eventType = typeNameRegistry.GetName(@event.Payload.GetType());

                var webhooks = await webhookRepository.QueryByAppAsync(contentEvent.AppId.Id);

                var matchingWebhooks = webhooks.Where(w => w.Schemas.Any(s => Matchs(s, contentEvent))).ToList();

                if (matchingWebhooks.Count > 0)
                {
                    var now = clock.GetCurrentInstant();

                    var eventPayload = CreatePayload(@event, eventType);
                    var eventName = $"{contentEvent.SchemaId.Name.ToPascalCase()}{CreateContentEventName(eventType)}";

                    foreach (var webhook in matchingWebhooks)
                    {
                        await EnqueueJobAsync(eventPayload, webhook, contentEvent, eventName, now);
                    }
                }
            }
        }

        private async Task EnqueueJobAsync(string payload, IWebhookEntity webhook, AppEvent contentEvent, string eventName, Instant now)
        {
            var signature = $"{payload}{webhook.SharedSecret}".Sha256Base64();

            var job = new WebhookJob
            {
                Id = Guid.NewGuid(),
                AppId = contentEvent.AppId.Id,
                RequestUrl = webhook.Url,
                RequestBody = payload,
                RequestSignature = signature,
                EventName = eventName,
                Expires = now.Plus(ExpirationTime),
                WebhookId = webhook.Id
            };

            await webhookEventRepository.EnqueueAsync(job, now);
        }

        private static bool Matchs(WebhookSchema webhookSchema, SchemaEvent @event)
        {
            return
               (@event.SchemaId.Id == webhookSchema.SchemaId) &&
               (@event is ContentCreated && webhookSchema.SendCreate ||
                @event is ContentUpdated && webhookSchema.SendUpdate ||
                @event is ContentDeleted && webhookSchema.SendDelete ||
                @event is ContentPublished && webhookSchema.SendPublish ||
                @event is ContentUnpublished && webhookSchema.SendUnpublish);
        }

        private string CreatePayload(Envelope<IEvent> @event, string eventType)
        {
            return new JObject(
                new JProperty("type", eventType),
                new JProperty("payload", JObject.FromObject(@event.Payload, webhookSerializer)),
                new JProperty("timestamp", @event.Headers.Timestamp().ToString()))
                .ToString(Formatting.Indented);
        }

        private static string CreateContentEventName(string eventType)
        {
            return eventType.StartsWith(ContentPrefix) ? eventType.Substring(ContentPrefix.Length) : eventType;
        }
    }
}
