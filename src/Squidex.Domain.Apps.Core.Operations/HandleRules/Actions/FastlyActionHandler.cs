// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules.Actions.Utils;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Actions;

#pragma warning disable SA1649 // File name must match first type name

namespace Squidex.Domain.Apps.Core.HandleRules.Actions
{
    public sealed class FastlyJob
    {
        public string FastlyApiKey { get; set; }
        public string FastlyServiceID { get; set; }

        public string Key { get; set; }
    }

    public sealed class FastlyActionHandler : RuleActionHandler<FastlyAction, FastlyJob>
    {
        private const string Description = "Purge key in fastly";

        private readonly ClientPool<string, HttpClient> clients;

        public FastlyActionHandler()
        {
            clients = new ClientPool<string, HttpClient>(key =>
            {
                return new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(2)
                };
            });
        }

        protected override (string Description, FastlyJob Data) CreateJob(EnrichedEvent @event, FastlyAction action)
        {
            var ruleJob = new FastlyJob
            {
                Key = @event.AggregateId.ToString(),
                FastlyApiKey = action.ApiKey,
                FastlyServiceID = action.ServiceId
            };

            return (Description, ruleJob);
        }

        protected override async Task<(string Dump, Exception Exception)> ExecuteJobAsync(FastlyJob job)
        {
            var httpClient = clients.GetClient(string.Empty);

            return await httpClient.OneWayRequestAsync(BuildRequest(job), null);
        }

        private static HttpRequestMessage BuildRequest(FastlyJob job)
        {
            var requestUrl = $"https://api.fastly.com/service/{job.FastlyServiceID}/purge/{job.Key}";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

            request.Headers.Add("Fastly-Key", job.FastlyApiKey);

            return request;
        }
    }
}
