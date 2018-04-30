// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

#pragma warning disable SA1649 // File name must match first type name

namespace Squidex.Domain.Apps.Core.HandleRules.Actions
{
    public sealed class ElasticSearchJob
    {
        public string Host { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

        public string ContentId { get; set; }

        public string IndexName { get; set; }
        public string IndexType { get; set; }

        public string Operation { get; set; }

        public JObject Content { get; set; }
    }

    public sealed class ElasticSearchActionHandler : RuleActionHandler<ElasticSearchAction, ElasticSearchJob>
    {
        private const string DescriptionIgnore = "Ignore";

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

        protected override async Task<(string Description, ElasticSearchJob Data)> CreateJobAsync(Envelope<AppEvent> @event, string eventName, ElasticSearchAction action)
        {
            if (@event.Payload is ContentEvent contentEvent)
            {
                var contentId = contentEvent.ContentId.ToString();

                var ruleDescription = string.Empty;
                var ruleJob = new ElasticSearchJob
                {
                    Host = action.Host.ToString(),
                    Username = action.Username,
                    Password = action.Password,
                    ContentId = contentId,
                    IndexName = await formatter.FormatStringAsync(action.IndexName, @event),
                    IndexType = await formatter.FormatStringAsync(action.IndexType, @event),
                };

                var timestamp = @event.Headers.Timestamp().ToString();

                var actor = @event.Payload.Actor.ToString();

                switch (@event.Payload)
                {
                    case ContentCreated created:
                        {
                            ruleDescription = $"Add entry to ES index: {action.IndexName}";

                            ruleJob.Operation = "Create";
                            ruleJob.Content = new JObject(
                                new JProperty("id", contentId),
                                new JProperty("created", timestamp),
                                new JProperty("createdBy", actor),
                                new JProperty("lastModified", timestamp),
                                new JProperty("lastModifiedBy", actor),
                                new JProperty("status", Status.Draft.ToString()),
                                new JProperty("data", formatter.ToRouteData(created.Data)));
                            break;
                        }

                    case ContentUpdated updated:
                        {
                            ruleDescription = $"Update entry in ES index: {action.IndexName}";

                            ruleJob.Operation = "Update";
                            ruleJob.Content = new JObject(
                                new JProperty("lastModified", timestamp),
                                new JProperty("lastModifiedBy", actor),
                                new JProperty("data", formatter.ToRouteData(updated.Data)));
                            break;
                        }

                    case ContentStatusChanged statusChanged:
                        {
                            ruleDescription = $"Update entry in ES index: {action.IndexName}";

                            ruleJob.Operation = "Update";
                            ruleJob.Content = new JObject(
                                new JProperty("lastModified", timestamp),
                                new JProperty("lastModifiedBy", actor),
                                new JProperty("status", statusChanged.Status.ToString()));
                            break;
                        }

                    case ContentDeleted deleted:
                        {
                            ruleDescription = $"Delete entry from ES index: {action.IndexName}";

                            ruleJob.Operation = "Delete";
                            break;
                        }
                }
            }

            return (DescriptionIgnore, new ElasticSearchJob());
        }

        protected override async Task<(string Dump, Exception Exception)> ExecuteJobAsync(ElasticSearchJob job)
        {
            if (string.IsNullOrWhiteSpace(job.Operation))
            {
                return (null, new InvalidOperationException("The action cannot handle this event."));
            }

            var client = clients.GetClient((new Uri(job.Host, UriKind.Absolute), job.Username, job.Password));

            try
            {
                switch (job.Operation)
                {
                    case "Create":
                        {
                            var doc = job.Content.ToString();

                            var response = await client.IndexAsync<StringResponse>(job.IndexName, job.IndexType, job.ContentId, doc);

                            return (response.Body, response.OriginalException);
                        }

                    case "Update":
                        {
                            var doc = new JObject(new JProperty("doc", job.Content)).ToString();

                            var response = await client.UpdateAsync<StringResponse>(job.IndexName, job.IndexType, job.ContentId, doc);

                            return (response.Body, response.OriginalException);
                        }

                    case "Delete":
                        {
                            var response = await client.DeleteAsync<StringResponse>(job.IndexName, job.IndexType, job.ContentId);

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
