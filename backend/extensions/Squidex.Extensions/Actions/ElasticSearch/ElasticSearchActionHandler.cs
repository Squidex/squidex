// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;

#pragma warning disable IDE0059 // Value assigned to symbol is never used

namespace Squidex.Extensions.Actions.ElasticSearch
{
    public sealed class ElasticSearchActionHandler : RuleActionHandler<ElasticSearchAction, ElasticSearchJob>
    {
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
                    IndexName = Format(action.IndexName, @event),
                    IndexType = Format(action.IndexType, @event),
                    ContentId = contentId
                };

                if (contentEvent.Type == EnrichedContentEventType.Deleted ||
                    contentEvent.Type == EnrichedContentEventType.Unpublished)
                {
                    ruleDescription = $"Delete entry index: {action.IndexName}";
                }
                else
                {
                    ruleDescription = $"Upsert to index: {action.IndexName}";

                    var json = ToJson(contentEvent);

                    ruleJob.Content = $"{{ \"objectId\": \"{contentId}\", {json.Substring(1)}";
                }

                ruleJob.Username = action.Username;
                ruleJob.Password = action.Password;

                return (ruleDescription, ruleJob);
            }

            return ("Ignore", new ElasticSearchJob());
        }

        protected override async Task<Result> ExecuteJobAsync(ElasticSearchJob job, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(job.Host))
            {
                return Result.Ignored();
            }

            var client = clients.GetClient((new Uri(job.Host, UriKind.Absolute), job.Username, job.Password));

            try
            {
                if (job.Content != null)
                {
                    var response = await client.IndexAsync<StringResponse>(job.IndexName, job.IndexType, job.ContentId, job.Content, ctx: ct);

                    return Result.SuccessOrFailed(response.OriginalException, response.Body);
                }
                else
                {
                    var response = await client.DeleteAsync<StringResponse>(job.IndexName, job.IndexType, job.ContentId, ctx: ct);

                    return Result.SuccessOrFailed(response.OriginalException, response.Body);
                }
            }
            catch (ElasticsearchClientException ex)
            {
                return Result.Failed(ex);
            }
        }
    }

    public sealed class ElasticSearchJob
    {
        public string Host { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string ContentId { get; set; }

        public string Content { get; set; }

        public string IndexName { get; set; }

        public string IndexType { get; set; }
    }
}
