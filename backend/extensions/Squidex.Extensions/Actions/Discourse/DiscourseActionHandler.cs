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

namespace Squidex.Extensions.Actions.Discourse;

public sealed class DiscourseActionHandler : RuleActionHandler<DiscourseAction, DiscourseJob>
{
    private const string DescriptionCreatePost = "Create discourse Post";
    private const string DescriptionCreateTopic = "Create discourse Topic";

    private readonly IHttpClientFactory httpClientFactory;

    public DiscourseActionHandler(RuleEventFormatter formatter, IHttpClientFactory httpClientFactory)
        : base(formatter)
    {
        this.httpClientFactory = httpClientFactory;
    }

    protected override async Task<(string Description, DiscourseJob Data)> CreateJobAsync(EnrichedEvent @event, DiscourseAction action)
    {
        var url = $"{action.Url.ToString().TrimEnd('/')}/posts.json?api_key={action.ApiKey}&api_username={action.ApiUsername}";

        var json = new Dictionary<string, object>
        {
            ["title"] = await FormatAsync(action.Title, @event)
        };

        if (action.Topic != null)
        {
            json.Add("topic_id", action.Topic.Value);
        }

        if (action.Category != null)
        {
            json.Add("category", action.Category.Value);
        }

        json["raw"] = await FormatAsync(action.Text, @event);

        var requestBody = ToJson(json);

        var ruleJob = new DiscourseJob
        {
            ApiKey = action.ApiKey,
            ApiUserName = action.ApiUsername,
            RequestUrl = url,
            RequestBody = requestBody
        };

        var description =
            action.Topic != null ?
            DescriptionCreateTopic :
            DescriptionCreatePost;

        return (description, ruleJob);
    }

    protected override async Task<Result> ExecuteJobAsync(DiscourseJob job,
        CancellationToken ct = default)
    {
        using (var httpClient = httpClientFactory.CreateClient())
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, job.RequestUrl)
            {
                Content = new StringContent(job.RequestBody, Encoding.UTF8, "application/json")
            })
            {
                request.Headers.TryAddWithoutValidation("Api-Key", job.ApiKey);
                request.Headers.TryAddWithoutValidation("Api-Username", job.ApiUserName);

                return await httpClient.OneWayRequestAsync(request, job.RequestBody, ct);
            }
        }
    }
}

public sealed class DiscourseJob
{
    public string ApiKey { get; set; }

    public string ApiUserName { get; set; }

    public string RequestUrl { get; set; }

    public string RequestBody { get; set; }
}
