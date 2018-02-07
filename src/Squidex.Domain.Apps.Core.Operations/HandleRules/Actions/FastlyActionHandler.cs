// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Http;

namespace Squidex.Domain.Apps.Core.HandleRules.Actions
{
    public sealed class FastlyActionHandler : RuleActionHandler<FastlyAction>
    {
        protected override (string Description, RuleJobData Data) CreateJob(Envelope<AppEvent> @event, string eventName, FastlyAction action)
        {
            var ruleDescription = "Purge key in fastly";
            var ruleData = new RuleJobData
            {
                ["FastlyApiKey"] = action.ApiKey,
                ["FastlyServiceID"] = action.ServiceId
            };

            if (@event.Headers.Contains(CommonHeaders.AggregateId))
            {
                ruleData["Key"] = @event.Headers.AggregateId().ToString();
            }

            return (ruleDescription, ruleData);
        }

        public override async Task<(string Dump, Exception Exception)> ExecuteJobAsync(RuleJobData job)
        {
            if (!job.TryGetValue("Key", out var keyToken))
            {
                return (null, new InvalidOperationException("The action cannot handle this event."));
            }

            var requestMsg = BuildRequest(job, keyToken.Value<string>());

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
                if (requestMsg != null)
                {
                    var requestDump = DumpFormatter.BuildDump(requestMsg, response, null, ex.ToString(), TimeSpan.Zero, false);

                    return (requestDump, ex);
                }
                else
                {
                    var requestDump = ex.ToString();

                    return (requestDump, ex);
                }
            }
        }

        private static HttpRequestMessage BuildRequest(Dictionary<string, JToken> job, string key)
        {
            var serviceId = job["FastlyServiceID"].Value<string>();

            var requestUrl = $"https://api.fastly.com/service/{serviceId}/purge/{key}";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

            request.Headers.Add("Fastly-Key", job["FastlyApiKey"].Value<string>());

            return request;
        }
    }
}
