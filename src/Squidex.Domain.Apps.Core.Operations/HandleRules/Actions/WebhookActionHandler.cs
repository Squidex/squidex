// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Http;

namespace Squidex.Domain.Apps.Core.HandleRules.Actions
{
    public sealed class WebhookActionHandler : RuleActionHandler<WebhookAction>
    {
        private readonly RuleEventFormatter formatter;

        public WebhookActionHandler(RuleEventFormatter formatter)
        {
            Guard.NotNull(formatter, nameof(formatter));

            this.formatter = formatter;
        }

        protected override (string Description, RuleJobData Data) CreateJob(Envelope<AppEvent> @event, string eventName, WebhookAction action)
        {
            var body = formatter.ToRouteData(@event, eventName);

            var signature = $"{body.ToString(Formatting.Indented)}{action.SharedSecret}".Sha256Base64();

            var ruleDescription = $"Send event to webhook '{action.Url}'";
            var ruleData = new RuleJobData
            {
                ["RequestUrl"] = action.Url,
                ["RequestBody"] = body,
                ["RequestSignature"] = signature
            };

            return (ruleDescription, ruleData);
        }

        public override async Task<(string Dump, Exception Exception)> ExecuteJobAsync(RuleJobData job)
        {
            var requestBody = job["RequestBody"].ToString(Formatting.Indented);
            var requestMsg = BuildRequest(job, requestBody);

            HttpResponseMessage response = null;

            try
            {
                response = await HttpClientPool.GetHttpClient().SendAsync(requestMsg);

                var responseString = await response.Content.ReadAsStringAsync();
                var requestDump = DumpFormatter.BuildDump(requestMsg, response, requestBody, responseString, TimeSpan.Zero, false);

                Exception ex = null;

                if (!response.IsSuccessStatusCode)
                {
                    ex = new HttpRequestException($"Response code does not indicate success: {(int)response.StatusCode} ({response.StatusCode}).");
                }

                return (requestDump, ex);
            }
            catch (Exception ex)
            {
                if (requestMsg != null)
                {
                    var requestDump = DumpFormatter.BuildDump(requestMsg, response, requestBody, ex.ToString(), TimeSpan.Zero, false);

                    return (requestDump, ex);
                }
                else
                {
                    throw;
                }
            }
        }

        private static HttpRequestMessage BuildRequest(Dictionary<string, JToken> job, string requestBody)
        {
            var requestUrl = job["RequestUrl"].Value<string>();
            var requestSig = job["RequestSignature"].Value<string>();

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("X-Signature", requestSig);
            request.Headers.Add("User-Agent", "Squidex Webhook");

            return request;
        }
    }
}
