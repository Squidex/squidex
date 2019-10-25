﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;

namespace Squidex.Extensions.Actions.Prerender
{
    public sealed class PrerenderActionHandler : RuleActionHandler<PrerenderAction, PrerenderJob>
    {
        private readonly IHttpClientFactory httpClientFactory;

        public PrerenderActionHandler(RuleEventFormatter formatter, IHttpClientFactory httpClientFactory)
            : base(formatter)
        {
            this.httpClientFactory = httpClientFactory;
        }

        protected override (string Description, PrerenderJob Data) CreateJob(EnrichedEvent @event, PrerenderAction action)
        {
            var url = Format(action.Url, @event);

            var request = new { prerenderToken = action.Token, url };
            var requestBody = ToJson(request);

            return ($"Recache {url}", new PrerenderJob { RequestBody = requestBody });
        }

        protected override async Task<Result> ExecuteJobAsync(PrerenderJob job, CancellationToken ct = default)
        {
            using (var httpClient = httpClientFactory.CreateClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.prerender.io/recache")
                {
                    Content = new StringContent(job.RequestBody, Encoding.UTF8, "application/json")
                };

                return await httpClient.OneWayRequestAsync(request, job.RequestBody, ct);
            }
        }
    }

    public sealed class PrerenderJob
    {
        public string RequestBody { get; set; }
    }
}
