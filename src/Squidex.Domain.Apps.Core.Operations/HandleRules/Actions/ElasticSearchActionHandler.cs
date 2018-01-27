// ==========================================================================
//  ElasticSearchActionHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.SearchEngines;

namespace Squidex.Domain.Apps.Core.HandleRules.Actions
{
    public class ElasticSearchActionHandler : RuleActionHandler<ElasticSearchAction>
    {
        private readonly ISearchEngine searchEngine;
        private readonly JsonSerializer serializer;

        public ElasticSearchActionHandler(ISearchEngine searchEngine, JsonSerializer serializer)
        {
            Guard.NotNull(searchEngine, nameof(searchEngine));
            Guard.NotNull(serializer, nameof(serializer));

            this.searchEngine = searchEngine;
            this.serializer = serializer;
        }

        protected override (string Description, RuleJobData Data) CreateJob(Envelope<AppEvent> @event, string eventName,
            ElasticSearchAction action)
        {
            // todo: get the contentId from the event envelope
            var payload = CreatePayload(@event, eventName);
            var ruleDescription = $"Indexing event data in elasticsearch index {action.IndexName}";
            var ruleData = new RuleJobData
            {
                ["IndexName"] = action.IndexName,
                ["Payload"] = payload,
                ["EventType"] = eventName,
                ["TypeNameForSchema"] = action.TypeNameForSchema,
                ["HostUrl"] = action.HostUrl,
                ["RequiresAuthentication"] = action.RequiresAuthentication,
                ["Username"] = action.Username,
                ["Password"] = action.Password
            };

            return (ruleDescription, ruleData);
        }

        public override async Task<(string Dump, Exception Exception)> ExecuteJobAsync(RuleJobData job)
        {
            var payload = ReformatContentPayload(job["Payload"] as JObject);
            var eventType = job["EventType"].Value<string>();
            var typeName = job["TypeNameForSchema"].Value<string>();
            var indexName = job["IndexName"].Value<string>();
            var contentId = job["ContentId"].Value<Guid>();
            var hostUrl = job["HostUrl"].Value<string>();
            var requiresAuth = job["RequiresAuthentication"].Value<bool>();
            var username = job["Username"].Value<string>();
            var password = job["Password"].Value<string>();

            try
            {
                bool connectResult = requiresAuth
                    ? searchEngine.Connect(hostUrl, username, password)
                    : searchEngine.Connect(hostUrl);

                if (!connectResult)
                {
                    throw new Exception("Couldn't connect to the elastic search service.");
                }

                if (eventType.Contains("Deleted"))
                {
                    await searchEngine.DeleteContentFromIndexAsync(contentId, typeName, indexName);
                }
                else if (eventType.Contains("Updated"))
                {
                    await searchEngine.UpdateContentInIndexAsync(payload, contentId, typeName, indexName);
                }
                else if (eventType.Contains("Created"))
                {
                    await searchEngine.AddContentToIndexAsync(payload, contentId, typeName, indexName);
                }
            }
            catch (Exception e)
            {
                return (string.Empty, e);
            }

            return (string.Empty, null);
        }

        private JObject CreatePayload(Envelope<AppEvent> @event, string eventName)
        {
            return new JObject(
                new JProperty("type", eventName),
                new JProperty("payload", JObject.FromObject(@event.Payload, serializer)),
                new JProperty("timestamp", @event.Headers.Timestamp().ToString()));
        }

        private JObject ReformatContentPayload(JObject content)
        {
            // lets try to keep the original format, because when we query from ES, we expect the same DTO.
            var conentEntity = new
            {
                Id = content["payload"]["contentId"].Value<Guid>(),
                AppId = content["payload"]["appId"].Value<Guid>(),
                CreatedBy = string.Empty,
                LastModifiedBy = string.Empty,
                Created = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LastModified = Instant.FromDateTimeUtc(DateTime.UtcNow),
                Status = 2, // published
                Version = 1,
                Data = content["payload"]["data"]
            };

            return JObject.FromObject(conentEntity, serializer);
        }
    }
}