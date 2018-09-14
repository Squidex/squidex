// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Infrastructure;

namespace Squidex.Extensions.Actions.Webhook
{
    public sealed class WebhookActionHandler : RuleActionHandler<WebhookAction, WebhookJob>
    {
        private readonly IHttpClientFactory httpClientFactory;

        public WebhookActionHandler(RuleEventFormatter formatter, IHttpClientFactory httpClientFactory)
            : base(formatter)
        {
            Guard.NotNull(httpClientFactory, nameof(httpClientFactory));

            this.httpClientFactory = httpClientFactory;
        }

        protected override (string Description, WebhookJob Data) CreateJob(EnrichedEvent @event, WebhookAction action)
        {
            var requestBody = ToEnvelopeJson(@event);
            var requestUrl = Format(action.Url, @event);

            var ruleDescription = $"Send event to webhook '{requestUrl}'";
            var ruleJob = new WebhookJob
            {
                RequestUrl = Format(action.Url.ToString(), @event),
                RequestSignature = $"{requestBody}{action.SharedSecret}".Sha256Base64(),
                RequestBody = requestBody
            };

            return (ruleDescription, ruleJob);
        }

        protected override async Task<(string Dump, Exception Exception)> ExecuteJobAsync(WebhookJob job)
        {
            using (var httpClient = httpClientFactory.CreateClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, job.RequestUrl)
                {
                    Content = new StringContent(job.RequestBody, Encoding.UTF8, "application/json")
                };

                request.Headers.Add("X-Signature", job.RequestSignature);
                request.Headers.Add("X-Application", "Squidex Webhook");
                request.Headers.Add("User-Agent", "Squidex Webhook");

                return await httpClient.OneWayRequestAsync(request, job.RequestBody);
            }
        }
    }

    public sealed class WebhookJob
    {
        public string RequestUrl { get; set; }

        public string RequestSignature { get; set; }

        public string RequestBody { get; set; }
    }
}
