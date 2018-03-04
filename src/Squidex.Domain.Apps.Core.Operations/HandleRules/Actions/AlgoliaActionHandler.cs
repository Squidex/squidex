// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Algolia.Search;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Contents;
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
        private readonly ClientPool<(string AppId, string ApiKey, string IndexName), Index> clients;
        private readonly RuleEventFormatter formatter;

        public AlgoliaActionHandler(RuleEventFormatter formatter)
        {
            Guard.NotNull(formatter, nameof(formatter));

            this.formatter = formatter;

            clients = new ClientPool<(string AppId, string ApiKey, string IndexName), Index>(key =>
            {
                var client = new AlgoliaClient(key.AppId, key.ApiKey);

                return client.InitIndex(key.IndexName);
            });
        }

        protected override (string Description, RuleJobData Data) CreateJob(Envelope<AppEvent> @event, string eventName, AlgoliaAction action)
        {
            var ruleDescription = string.Empty;
            var ruleData = new RuleJobData
            {
                ["AppId"] = action.AppId,
                ["ApiKey"] = action.ApiKey
            };

            if (@event.Payload is ContentEvent contentEvent)
            {
                ruleData["ContentId"] = contentEvent.ContentId.ToString();
                ruleData["Operation"] = "Upsert";
                ruleData["IndexName"] = formatter.FormatString(action.IndexName, @event);

                var timestamp = @event.Headers.Timestamp().ToString();

                switch (@event.Payload)
                {
                    case ContentCreated created:
                    {
                        ruleDescription = $"Add entry to Algolia index: {action.IndexName}";

                        ruleData["Content"] = new JObject(
                            new JProperty("id", contentEvent.ContentId),
                            new JProperty("created", timestamp),
                            new JProperty("createdBy", created.Actor.ToString()),
                            new JProperty("lastModified", timestamp),
                            new JProperty("lastModifiedBy", created.Actor.ToString()),
                            new JProperty("status", Status.Draft.ToString()),
                            new JProperty("data", formatter.ToRouteData(created.Data)));
                        break;
                    }

                    case ContentUpdated updated:
                    {
                        ruleDescription = $"Update entry in Algolia index: {action.IndexName}";

                        ruleData["Content"] = new JObject(
                            new JProperty("lastModified", timestamp),
                            new JProperty("lastModifiedBy", updated.Actor.ToString()),
                            new JProperty("data", formatter.ToRouteData(updated.Data)));
                        break;
                    }

                    case ContentStatusChanged statusChanged:
                    {
                        ruleDescription = $"Update entry in Algolia index: {action.IndexName}";

                        ruleData["Content"] = new JObject(
                            new JProperty("lastModified", timestamp),
                            new JProperty("lastModifiedBy", statusChanged.Actor.ToString()),
                            new JProperty("status", statusChanged.Status.ToString()));
                        break;
                    }

                    case ContentDeleted deleted:
                    {
                        ruleDescription = $"Delete entry from Algolia index: {action.IndexName}";

                        ruleData["Content"] = new JObject();
                        ruleData["Operation"] = "Delete";
                        break;
                    }
                }
            }

            return (ruleDescription, ruleData);
        }

        public override async Task<(string Dump, Exception Exception)> ExecuteJobAsync(RuleJobData job)
        {
            if (!job.TryGetValue("Operation", out var operationToken))
            {
                return (null, new InvalidOperationException("The action cannot handle this event."));
            }

            var appId = job["AppId"].Value<string>();
            var apiKey = job["ApiKey"].Value<string>();
            var indexName = job["IndexName"].Value<string>();

            var index = clients.GetClient((appId, apiKey, indexName));

            var operation = operationToken.Value<string>();
            var content = job["Content"].Value<JObject>();
            var contentId = job["ContentId"].Value<string>();

            try
            {
                switch (operation)
                {
                    case "Upsert":
                    {
                        content["objectID"] = contentId;

                        var response = await index.PartialUpdateObjectAsync(content);

                        return (response.ToString(Formatting.Indented), null);
                    }

                    case "Delete":
                    {
                        var response = await index.DeleteObjectAsync(contentId);

                        return (response.ToString(Formatting.Indented), null);
                    }

                    default:
                        return (null, null);
                }
            }
            catch (AlgoliaException ex)
            {
                return (ex.Message, ex);
            }
        }
    }
}
