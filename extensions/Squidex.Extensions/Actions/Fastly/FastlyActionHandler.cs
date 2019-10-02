﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Infrastructure;

namespace Squidex.Extensions.Actions.Fastly
{
    public sealed class FastlyActionHandler : RuleActionHandler<FastlyAction, FastlyJob>
    {
        private const string Description = "Purge key in fastly";

        private readonly IHttpClientFactory httpClientFactory;

        public FastlyActionHandler(RuleEventFormatter formatter, IHttpClientFactory httpClientFactory)
            : base(formatter)
        {
            Guard.NotNull(httpClientFactory);

            this.httpClientFactory = httpClientFactory;
        }

        protected override (string Description, FastlyJob Data) CreateJob(EnrichedEvent @event, FastlyAction action)
        {
            var id = @event is IEnrichedEntityEvent entityEvent ? entityEvent.Id.ToString() : string.Empty;

            var ruleJob = new FastlyJob
            {
                Key = id,
                FastlyApiKey = action.ApiKey,
                FastlyServiceID = action.ServiceId
            };

            return (Description, ruleJob);
        }

        protected override async Task<Result> ExecuteJobAsync(FastlyJob job, CancellationToken ct = default)
        {
            using (var httpClient = httpClientFactory.CreateClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(2);

                var requestUrl = $"https://api.fastly.com/service/{job.FastlyServiceID}/purge/{job.Key}";
                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

                request.Headers.Add("Fastly-Key", job.FastlyApiKey);

                return await httpClient.OneWayRequestAsync(request, ct: ct);
            }
        }
    }

    public sealed class FastlyJob
    {
        public string FastlyApiKey { get; set; }

        public string FastlyServiceID { get; set; }

        public string Key { get; set; }
    }
}
