// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Algolia.Search;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Core.HandleRules.Actions
{
    public sealed class AlgoliaActionHandler : RuleActionHandler<AlgoliaAction>
    {
        private readonly ConcurrentDictionary<(string AppId, string ApiKey, string IndexName), Index> clients = new ConcurrentDictionary<(string AppId, string ApiKey, string IndexName), Index>();
        private readonly JsonSerializer serializer;

        public AlgoliaActionHandler(JsonSerializer serializer)
        {
            Guard.NotNull(serializer, nameof(serializer));

            this.serializer = serializer;
        }

        protected override (string Description, RuleJobData Data) CreateJob(Envelope<AppEvent> @event, string eventName, AlgoliaAction action)
        {
            var ruleDescription = string.Empty;
            var ruleData = new RuleJobData
            {
                ["AppId"] = action.AppId,
                ["ApiKey"] = action.ApiKey,
                ["IndexName"] = action.IndexName
            };

            if (@event.Payload is ContentEvent contentEvent)
            {
                ruleData["ContentId"] = contentEvent.ContentId.ToString();
                ruleData["Operation"] = "Upsert";

                switch (@event.Payload)
                {
                    case ContentCreated created:
                    {
                        ruleDescription = $"Add entry to Algolia index: {action.IndexName}";
                        ruleData["Content"] = new JObject(
                            new JProperty("data", JObject.FromObject(created.Data, serializer)));
                        break;
                    }

                    case ContentUpdated updated:
                    {
                        ruleDescription = $"Update entry in Algolia index: {action.IndexName}";
                        ruleData["Content"] = new JObject(
                            new JProperty("data", JObject.FromObject(updated.Data, serializer)));
                        break;
                    }

                    case ContentStatusChanged statusChanged:
                    {
                        ruleDescription = $"Update entry in Algolia index: {action.IndexName}";
                        ruleData["Content"] = new JObject(
                            new JProperty("status", statusChanged.Status.ToString()));
                        break;
                    }

                    case ContentDeleted deleted:
                    {
                        ruleDescription = $"Delete entry from Index: {action.IndexName}";
                        ruleData["Content"] = new JObject();
                        break;
                    }
                }
            }

            return (ruleDescription, ruleData);
        }

        public override async Task<(string Dump, Exception Exception)> ExecuteJobAsync(RuleJobData job)
        {
            var appId = (string)job["AppId"];
            var apiKey = (string)job["ApiKey"];
            var indexName = (string)job["IndexName"];

            var index = clients.GetOrAdd((appId, apiKey, indexName), s =>
            {
                var client = new AlgoliaClient(appId, apiKey);

                return client.InitIndex(indexName);
            });

            var operation = (string)job["Operation"];
            var content = (JObject)job["Content"];
            var contentId = (string)job["ContentId"];

            try
            {
                switch (operation)
                {
                    case "Upsert":
                    {
                        content["objectID"] = contentId;

                        var resonse = await index.PartialUpdateObjectAsync(content);

                        return (resonse.ToString(Formatting.Indented), null);
                    }

                    case "Delete":
                    {
                        var resonse = await index.DeleteObjectAsync(contentId);

                        return (resonse.ToString(Formatting.Indented), null);
                    }

                    default:
                    {
                        return ("Nothing to do!", null);
                    }
                }
            }
            catch (AlgoliaException ex)
            {
                return (ex.Message, ex);
            }
            catch (Exception ex)
            {
                return (null, ex);
            }
        }
    }
}
