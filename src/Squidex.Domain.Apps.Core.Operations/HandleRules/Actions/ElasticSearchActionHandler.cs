// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Elasticsearch.Net;
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
    public sealed class ElasticSearchActionHandler : RuleActionHandler<ElasticSearchAction>
    {
        private readonly ClientPool<(Uri Host, string Username, string Password), ElasticLowLevelClient> clients;
        private readonly RuleEventFormatter formatter;

        public ElasticSearchActionHandler(RuleEventFormatter formatter)
        {
            Guard.NotNull(formatter, nameof(formatter));

            this.formatter = formatter;

            clients = new ClientPool<(Uri Host, string Username, string Password), ElasticLowLevelClient>(key =>
            {
                var config = new ConnectionConfiguration(key.Host);

                if (!string.IsNullOrEmpty(key.Username) && !string.IsNullOrWhiteSpace(key.Password))
                {
                    config = config.BasicAuthentication(key.Username, key.Password);
                }

                return new ElasticLowLevelClient(config);
            });
        }

        protected override (string Description, RuleJobData Data) CreateJob(Envelope<AppEvent> @event, string eventName, ElasticSearchAction action)
        {
            var ruleDescription = string.Empty;
            var ruleData = new RuleJobData
            {
                ["Host"] = action.Host,
                ["Username"] = action.Username,
                ["Password"] = action.Password
            };

            if (@event.Payload is ContentEvent contentEvent)
            {
                ruleData["ContentId"] = contentEvent.ContentId.ToString();
                ruleData["IndexName"] = formatter.FormatString(action.IndexName, @event);
                ruleData["IndexType"] = formatter.FormatString(action.IndexType, @event);

                var timestamp = @event.Headers.Timestamp().ToString();

                switch (@event.Payload)
                {
                    case ContentCreated created:
                        {
                            ruleDescription = $"Add entry to ES index: {action.IndexName}";

                            ruleData["Operation"] = "Create";
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
                            ruleDescription = $"Update entry in ES index: {action.IndexName}";

                            ruleData["Operation"] = "Update";
                            ruleData["Content"] = new JObject(
                                new JProperty("lastModified", timestamp),
                                new JProperty("lastModifiedBy", updated.Actor.ToString()),
                                new JProperty("data", formatter.ToRouteData(updated.Data)));
                            break;
                        }

                    case ContentStatusChanged statusChanged:
                        {
                            ruleDescription = $"Update entry in ES index: {action.IndexName}";

                            ruleData["Operation"] = "Update";
                            ruleData["Content"] = new JObject(
                                new JProperty("lastModified", timestamp),
                                new JProperty("lastModifiedBy", statusChanged.Actor.ToString()),
                                new JProperty("status", statusChanged.Status.ToString()));
                            break;
                        }

                    case ContentDeleted deleted:
                        {
                            ruleDescription = $"Delete entry from ES index: {action.IndexName}";

                            ruleData["Operation"] = "Delete";
                            ruleData["Content"] = new JObject();
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

            var host = new Uri(job["Host"].Value<string>(), UriKind.Absolute);

            var username = job["Username"].Value<string>();
            var password = job["Password"].Value<string>();

            var client = clients.GetClient((host, username, password));

            var indexName = job["IndexName"].Value<string>();
            var indexType = job["IndexType"].Value<string>();

            var operation = operationToken.Value<string>();
            var content = job["Content"].Value<JObject>();
            var contentId = job["ContentId"].Value<string>();

            try
            {
                switch (operation)
                {
                    case "Create":
                        {
                            var doc = content.ToString();

                            var response = await client.IndexAsync<StringResponse>(indexName, indexType, contentId, doc);

                            return (response.Body, response.OriginalException);
                        }

                    case "Update":
                        {
                            var doc = new JObject(new JProperty("doc", content)).ToString();

                            var response = await client.UpdateAsync<StringResponse>(indexName, indexType, contentId, doc);

                            return (response.Body, response.OriginalException);
                        }

                    case "Delete":
                        {
                            var response = await client.DeleteAsync<StringResponse>(indexName, indexType, contentId);

                            return (response.Body, response.OriginalException);
                        }

                    default:
                        return (null, null);
                }
            }
            catch (ElasticsearchClientException ex)
            {
                return (ex.Message, ex);
            }
        }
    }
}
