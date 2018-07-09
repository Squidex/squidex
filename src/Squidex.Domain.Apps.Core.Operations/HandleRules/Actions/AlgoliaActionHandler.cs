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
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Infrastructure;

#pragma warning disable SA1649 // File name must match first type name

namespace Squidex.Domain.Apps.Core.HandleRules.Actions
{
    public sealed class AlgoliaJob
    {
        public string AppId { get; set; }

        public string ApiKey { get; set; }

        public string ContentId { get; set; }

        public string IndexName { get; set; }

        public JObject Content { get; set; }
    }

    public sealed class AlgoliaActionHandler : RuleActionHandler<AlgoliaAction, AlgoliaJob>
    {
        private const string DescriptionIgnore = "Ignore";

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
                    IndexName = formatter.Format(action.IndexName, @event)
                };

                if (contentEvent.Type == EnrichedContentEventType.Deleted ||
                    contentEvent.Type == EnrichedContentEventType.Unpublished)
                {
                    ruleDescription = $"Delete entry from Algolia index: {action.IndexName}";
                }
                else
                {
                    ruleDescription = $"Add entry to Algolia index: {action.IndexName}";

                    ruleJob.Content = formatter.ToPayload(contentEvent);
                    ruleJob.Content["objectID"] = contentId;
                }

                return (ruleDescription, ruleJob);
            }

            return (DescriptionIgnore, new AlgoliaJob());
        }

        protected override async Task<(string Dump, Exception Exception)> ExecuteJobAsync(AlgoliaJob job)
        {
            if (string.IsNullOrWhiteSpace(job.AppId))
            {
                return (null, new InvalidOperationException("The action cannot handle this event."));
            }

            var index = clients.GetClient((job.AppId, job.ApiKey, job.IndexName));

            try
            {
                if (job.Content != null)
                {
                    var response = await index.PartialUpdateObjectAsync(job.Content);

                    return (response.ToString(Formatting.Indented), null);
                }
                else
                {
                    var response = await index.DeleteObjectAsync(job.ContentId);

                    return (response.ToString(Formatting.Indented), null);
                }
            }
            catch (AlgoliaException ex)
            {
                return (ex.Message, ex);
            }
        }
    }
}
