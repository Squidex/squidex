// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Extensions.Actions.Prerender;

public sealed class PrerenderActionHandler : RuleActionHandler<PrerenderAction, PrerenderJob>
{
    private readonly IHttpClientFactory httpClientFactory;

    public PrerenderActionHandler(RuleEventFormatter formatter, IHttpClientFactory httpClientFactory)
        : base(formatter)
    {
        this.httpClientFactory = httpClientFactory;
    }

    protected override async Task<(string Description, PrerenderJob Data)> CreateJobAsync(EnrichedEvent @event, PrerenderAction action)
    {
        var url = await FormatAsync(action.Url, @event);

        var requestObject = new { prerenderToken = action.Token, url };
        var requestBody = ToJson(requestObject);

        return ($"Recache {url}", new PrerenderJob { RequestBody = requestBody });
    }

    protected override async Task<Result> ExecuteJobAsync(PrerenderJob job,
        CancellationToken ct = default)
    {
        var httpClient = httpClientFactory.CreateClient("Prerender");

        var request = new HttpRequestMessage(HttpMethod.Post, "/recache")
        {
            Content = new StringContent(job.RequestBody, Encoding.UTF8, "application/json")
        };

        return await httpClient.OneWayRequestAsync(request, job.RequestBody, ct);
    }
}

public sealed class PrerenderJob
{
    public string RequestBody { get; set; }
}
