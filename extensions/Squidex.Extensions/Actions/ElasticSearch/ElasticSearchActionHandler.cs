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
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;

namespace Squidex.Extensions.Actions.ElasticSearch
{
    public sealed class ElasticSearchActionHandler : RuleActionHandler<ElasticSearchAction, ElasticSearchJob>
    {
        private const string DescriptionIgnore = "Ignore";

        private readonly ClientPool<(Uri Host, string Username, string Password), ElasticLowLevelClient> clients;

        public ElasticSearchActionHandler(RuleEventFormatter formatter)
            : base(formatter)
        {
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

        protected override (string Description, ElasticSearchJob Data) CreateJob(EnrichedEvent @event, ElasticSearchAction action)
        {
            if (@event is EnrichedContentEvent contentEvent)
            {
                var contentId = contentEvent.Id.ToString();

                var ruleDescription = string.Empty;
                var ruleJob = new ElasticSearchJob
                {
                    Host = action.Host.ToString(),
                    ContentId = contentId,
                    IndexName = Format(action.IndexName, @event),
                    IndexType = Format(action.IndexType, @event)
                };

                if (contentEvent.Type == EnrichedContentEventType.Deleted ||
                    contentEvent.Type == EnrichedContentEventType.Unpublished)
                {
                    ruleDescription = $"Delete entry index: {action.IndexName}";
                }
                else
                {
                    ruleDescription = $"Upsert to index: {action.IndexName}";

                    ruleJob.Content = ToPayload(contentEvent);
                    ruleJob.Content["objectID"] = contentId;
                }

                ruleJob.Username = action.Username;
                ruleJob.Password = action.Password;

                return (ruleDescription, ruleJob);
            }

            return (DescriptionIgnore, new ElasticSearchJob());
        }

        protected override async Task<(string Dump, Exception Exception)> ExecuteJobAsync(ElasticSearchJob job)
        {
            if (string.IsNullOrWhiteSpace(job.Host))
            {
                return (DescriptionIgnore, null);
            }

            var client = clients.GetClient((new Uri(job.Host, UriKind.Absolute), job.Username, job.Password));

            try
            {
                if (job.Content != null)
                {
                    var doc = job.Content.ToString();

                    var response = await client.IndexAsync<StringResponse>(job.IndexName, job.IndexType, job.ContentId, doc);

                    return (response.Body, response.OriginalException);
                }
                else
                {
                    var response = await client.DeleteAsync<StringResponse>(job.IndexName, job.IndexType, job.ContentId);

                    return (response.Body, response.OriginalException);
                }
            }
            catch (ElasticsearchClientException ex)
            {
                return (ex.Message, ex);
            }
        }
    }

    public sealed class ElasticSearchJob
    {
        public string Host { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string ContentId { get; set; }

        public string IndexName { get; set; }

        public string IndexType { get; set; }

        public JObject Content { get; set; }
    }
}
