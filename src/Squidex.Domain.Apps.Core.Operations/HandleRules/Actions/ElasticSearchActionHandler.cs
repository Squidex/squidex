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
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Infrastructure;

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

        protected override (string Description, ElasticSearchJob Data) CreateJob(EnrichedEvent @event, ElasticSearchAction action)
        {
            if (@event is EnrichedContentEvent contentEvent)
            {
                var contentId = contentEvent.Id.ToString();

                var ruleDescription = string.Empty;
                var ruleJob = new ElasticSearchJob
                {
                    Host = action.Host.ToString(),
                    Username = action.Username,
                    Password = action.Password,
                    ContentId = contentId,
                    IndexName = formatter.Format(action.IndexName, @event),
                    IndexType = formatter.Format(action.IndexType, @event),
                };

                if (contentEvent.Type == EnrichedContentEventType.Deleted ||
                    contentEvent.Type == EnrichedContentEventType.Unpublished)
                {
                    ruleDescription = $"Delete entry index: {action.IndexName}";
                }
                else
                {
                    ruleDescription = $"Upsert to index: {action.IndexName}";

                    ruleJob.Content = formatter.ToPayload(contentEvent);
                    ruleJob.Content["objectID"] = contentId;
                }
            }

            return (DescriptionIgnore, new ElasticSearchJob());
        }

        protected override async Task<(string Dump, Exception Exception)> ExecuteJobAsync(ElasticSearchJob job)
        {
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
}
