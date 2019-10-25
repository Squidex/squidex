﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Infrastructure;

namespace Squidex.Extensions.Actions.Slack
{
    public sealed class SlackActionHandler : RuleActionHandler<SlackAction, SlackJob>
    {
        private const string Description = "Send message to slack";

        private readonly IHttpClientFactory httpClientFactory;

        public SlackActionHandler(RuleEventFormatter formatter, IHttpClientFactory httpClientFactory)
            : base(formatter)
        {
            Guard.NotNull(httpClientFactory);

            this.httpClientFactory = httpClientFactory;
        }

        protected override (string Description, SlackJob Data) CreateJob(EnrichedEvent @event, SlackAction action)
        {
            var body = new { text = Format(action.Text, @event) };

            var ruleJob = new SlackJob
            {
                RequestUrl = action.WebhookUrl.ToString(),
                RequestBody = ToJson(body)
            };

            return (Description, ruleJob);
        }

        protected override async Task<Result> ExecuteJobAsync(SlackJob job, CancellationToken ct = default)
        {
            using (var httpClient = httpClientFactory.CreateClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(2);

                var request = new HttpRequestMessage(HttpMethod.Post, job.RequestUrl)
                {
                    Content = new StringContent(job.RequestBody, Encoding.UTF8, "application/json")
                };

                return await httpClient.OneWayRequestAsync(request, job.RequestBody, ct);
            }
        }
    }

    public sealed class SlackJob
    {
        public string RequestUrl { get; set; }

        public string RequestBody { get; set; }
    }
}
