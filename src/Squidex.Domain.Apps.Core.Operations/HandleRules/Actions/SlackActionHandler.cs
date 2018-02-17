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
    public sealed class SlackActionHandler : RuleActionHandler<SlackAction>
    {
        private readonly RuleEventFormatter formatter;

        public SlackActionHandler(RuleEventFormatter formatter)
        {
            Guard.NotNull(formatter, nameof(formatter));

            this.formatter = formatter;
        }

        protected override (string Description, RuleJobData Data) CreateJob(Envelope<AppEvent> @event, string eventName, SlackAction action)
        {
            var body = CreatePayload(@event, action.Text);

            var ruleDescription = "Send message to slack";
            var ruleData = new RuleJobData
            {
                ["RequestUrl"] = action.WebhookUrl,
                ["RequestBody"] = body
            };

            return (ruleDescription, ruleData);
        }

        private JObject CreatePayload(Envelope<AppEvent> @event, string text)
        {
            return new JObject(new JProperty("text", formatter.FormatString(text, @event)));
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

                return (requestDump, null);
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

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };

            return request;
        }
    }
}
