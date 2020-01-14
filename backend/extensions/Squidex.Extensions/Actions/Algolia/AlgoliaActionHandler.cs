// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Algolia.Search;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using AlgoliaIndex = Algolia.Search.Index;

#pragma warning disable IDE0059 // Value assigned to symbol is never used

namespace Squidex.Extensions.Actions.Algolia
{
    public sealed class AlgoliaActionHandler : RuleActionHandler<AlgoliaAction, AlgoliaJob>
    {
        private readonly ClientPool<(string AppId, string ApiKey, string IndexName), AlgoliaIndex> clients;

        public AlgoliaActionHandler(RuleEventFormatter formatter)
            : base(formatter)
        {
            clients = new ClientPool<(string AppId, string ApiKey, string IndexName), AlgoliaIndex>(key =>
            {
                var client = new AlgoliaClient(key.AppId, key.ApiKey);

                return client.InitIndex(key.IndexName);
            });
        }

        protected override (string Description, AlgoliaJob Data) CreateJob(EnrichedEvent @event, AlgoliaAction action)
        {
            if (@event is EnrichedContentEvent contentEvent)
            {
                var contentId = contentEvent.Id.ToString();

                var ruleDescription = string.Empty;
                var ruleJob = new AlgoliaJob
                {
                    AppId = action.AppId,
                    ApiKey = action.ApiKey,
                    ContentId = contentId,
                    IndexName = Format(action.IndexName, @event)
                };

                if (contentEvent.Type == EnrichedContentEventType.Deleted ||
                    contentEvent.Type == EnrichedContentEventType.Unpublished)
                {
                    ruleDescription = $"Delete entry from Algolia index: {action.IndexName}";
                }
                else
                {
                    ruleDescription = $"Add entry to Algolia index: {action.IndexName}";

                    JObject json;
                    try
                    {
                        string jsonString;

                        if (!string.IsNullOrEmpty(action.Document))
                        {
                            jsonString = Format(action.Document, @event)?.Trim();
                        }
                        else
                        {
                            jsonString = ToJson(contentEvent);
                        }

                        json = JObject.Parse(jsonString);
                    }
                    catch (Exception ex)
                    {
                        json = new JObject(new JProperty("error", $"Invalid JSON: {ex.Message}"));
                    }

                    ruleJob.Content = json;
                    ruleJob.Content["objectID"] = contentId;
                }

                return (ruleDescription, ruleJob);
            }

            return ("Ignore", new AlgoliaJob());
        }

        protected override async Task<Result> ExecuteJobAsync(AlgoliaJob job, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(job.AppId))
            {
                return Result.Ignored();
            }

            var index = clients.GetClient((job.AppId, job.ApiKey, job.IndexName));

            try
            {
                if (job.Content != null)
                {
                    var response = await index.PartialUpdateObjectAsync(job.Content, true, ct);

                    return Result.Success(response.ToString(Formatting.Indented));
                }
                else
                {
                    var response = await index.DeleteObjectAsync(job.ContentId, ct);

                    return Result.Success(response.ToString(Formatting.Indented));
                }
            }
            catch (AlgoliaException ex)
            {
                return Result.Failed(ex);
            }
        }
    }

    public sealed class AlgoliaJob
    {
        public string AppId { get; set; }

        public string ApiKey { get; set; }

        public string ContentId { get; set; }

        public string IndexName { get; set; }

        public JObject Content { get; set; }
    }
}
