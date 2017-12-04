// ==========================================================================
//  WebhookActionHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
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
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);

        private readonly JsonSerializer serializer;

        public WebhookActionHandler(JsonSerializer serializer)
        {
            Guard.NotNull(serializer, nameof(serializer));

            this.serializer = serializer;
        }

        protected override (string Description, RuleJobData Data) CreateJob(Envelope<AppEvent> @event, string eventName, WebhookAction action)
        {
            var body = CreatePayload(@event, eventName);

            var signature = $"{body.ToString(Formatting.Indented)}{action.SharedSecret}".Sha256Base64();

            var ruleDescription = $"Send event to webhook {action.Url}";
            var ruleData = new RuleJobData
            {
                ["RequestUrl"] = action.Url,
                ["RequestBody"] = body,
                ["RequestSignature"] = signature
            };

            return (ruleDescription, ruleData);
        }

        private JObject CreatePayload(Envelope<AppEvent> @event, string eventName)
        {
            return new JObject(
                new JProperty("type", eventName),
                new JProperty("payload", JObject.FromObject(@event.Payload, serializer)),
                new JProperty("timestamp", @event.Headers.Timestamp().ToString()));
        }

        public override async Task<(string Dump, Exception Exception)> ExecuteJobAsync(RuleJobData job)
        {
            var requestBody = job["RequestBody"].ToString(Formatting.Indented);
            var request = BuildRequest(job, requestBody);

            HttpResponseMessage response = null;

            try
            {
                using (var client = new HttpClient { Timeout = Timeout })
                {
                    response = await client.SendAsync(request);

                    var responseString = await response.Content.ReadAsStringAsync();
                    var requestDump = DumpFormatter.BuildDump(request, response, requestBody, responseString, TimeSpan.Zero, false);

                    return (requestDump, null);
                }
            }
            catch (Exception ex)
            {
                if (request != null)
                {
                    var requestDump = DumpFormatter.BuildDump(request, response, requestBody, ex.ToString(), TimeSpan.Zero, false);

                    return (requestDump, ex);
                }
                else
                {
                    var requestDump = ex.ToString();

                    return (requestDump, ex);
                }
            }
        }

        private static HttpRequestMessage BuildRequest(Dictionary<string, JToken> job, string requestBody)
        {
            var requestUrl = job["RequestUrl"].ToString();
            var requestSignature = job["RequestSignature"].ToString();

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("X-Signature", requestSignature);
            request.Headers.Add("User-Agent", "Squidex Webhook");

            return request;
        }
    }
}
