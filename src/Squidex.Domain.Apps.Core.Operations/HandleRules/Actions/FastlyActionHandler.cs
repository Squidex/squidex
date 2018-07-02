// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Infrastructure.Http;

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
            if (string.IsNullOrWhiteSpace(job.Key))
            {
                return (null, new InvalidOperationException("The action cannot handle this event."));
            }

            var requestMsg = BuildRequest(job);

            HttpResponseMessage response = null;

            try
            {
                response = await HttpClientPool.GetHttpClient().SendAsync(requestMsg);

                var responseString = await response.Content.ReadAsStringAsync();
                var requestDump = DumpFormatter.BuildDump(requestMsg, response, null, responseString, TimeSpan.Zero, false);

                return (requestDump, null);
            }
            catch (Exception ex)
            {
                var requestDump = DumpFormatter.BuildDump(requestMsg, response, null, ex.ToString(), TimeSpan.Zero, false);

                return (requestDump, ex);
            }
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
