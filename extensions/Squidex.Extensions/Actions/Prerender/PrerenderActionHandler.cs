// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
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

            var request =
                new JObject(
                    new JProperty("prerenderToken", action.Token),
                    new JProperty("url", url));

            return ($"Recache {url}", new PrerenderJob { RequestBody = request.ToString() });
        }

        protected override async Task<(string Dump, Exception Exception)> ExecuteJobAsync(PrerenderJob job)
        {
            using (var httpClient = httpClientFactory.CreateClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.prerender.io/recache")
                {
                    Content = new StringContent(job.RequestBody, Encoding.UTF8, "application/json")
                };

                return await httpClient.OneWayRequestAsync(request, job.RequestBody);
            }
        }
    }

    public sealed class PrerenderJob
    {
        public string RequestBody { get; set; }
    }
}
