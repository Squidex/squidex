// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Infrastructure;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Extensions.Actions.Webhook;

public sealed class WebhookActionHandler : RuleActionHandler<WebhookAction, WebhookJob>
{
    private readonly IHttpClientFactory httpClientFactory;

    public WebhookActionHandler(RuleEventFormatter formatter, IHttpClientFactory httpClientFactory)
        : base(formatter)
    {
        this.httpClientFactory = httpClientFactory;
    }

    protected override async Task<(string Description, WebhookJob Data)> CreateJobAsync(EnrichedEvent @event, WebhookAction action)
    {
        var requestUrl = await FormatAsync(action.Url, @event);
        var requestBody = string.Empty;
        var requestSignature = string.Empty;

        if (action.Method != WebhookMethod.GET)
        {
            if (!string.IsNullOrEmpty(action.Payload))
            {
                requestBody = await FormatAsync(action.Payload, @event);
            }
            else
            {
                requestBody = ToEnvelopeJson(@event);
            }

            requestSignature = $"{requestBody}{action.SharedSecret}".ToSha256Base64();
        }

        var ruleDescription = $"Send event to webhook '{requestUrl}'";
        var ruleJob = new WebhookJob
        {
            Method = action.Method,
            RequestUrl = await FormatAsync(action.Url.ToString(), @event),
            RequestSignature = requestSignature,
            RequestBody = requestBody,
            RequestBodyType = action.PayloadType,
            Headers = await ParseHeadersAsync(action.Headers, @event)
        };

        return (ruleDescription, ruleJob);
    }

    private async Task<Dictionary<string, string>> ParseHeadersAsync(string headers, EnrichedEvent @event)
    {
        if (string.IsNullOrWhiteSpace(headers))
        {
            return null;
        }

        var headersDictionary = new Dictionary<string, string>();

        var lines = headers.Split('\n');

        foreach (var line in lines)
        {
            var indexEqual = line.IndexOf('=', StringComparison.Ordinal);

            if (indexEqual > 0 && indexEqual < line.Length - 1)
            {
                var headerKey = line[..indexEqual];
                var headerValue = line[(indexEqual + 1)..];

                headerValue = await FormatAsync(headerValue, @event);

                headersDictionary[headerKey] = headerValue;
            }
        }

        return headersDictionary;
    }

    protected override async Task<Result> ExecuteJobAsync(WebhookJob job,
        CancellationToken ct = default)
    {
        var httpClient = httpClientFactory.CreateClient();

        var method = HttpMethod.Post;

        switch (job.Method)
        {
            case WebhookMethod.PUT:
                method = HttpMethod.Put;
                break;
            case WebhookMethod.GET:
                method = HttpMethod.Get;
                break;
            case WebhookMethod.DELETE:
                method = HttpMethod.Delete;
                break;
            case WebhookMethod.PATCH:
                method = HttpMethod.Patch;
                break;
        }

        using var request = new HttpRequestMessage(method, job.RequestUrl);

        if (!string.IsNullOrEmpty(job.RequestBody) && job.Method != WebhookMethod.GET)
        {
            var mediaType = job.RequestBodyType.Or("application/json");

            request.Content = new StringContent(job.RequestBody, Encoding.UTF8, mediaType);
        }

        request.Headers.Add("User-Agent", "Squidex Webhook");

        if (job.Headers != null)
        {
            foreach (var (key, value) in job.Headers)
            {
                request.Headers.TryAddWithoutValidation(key, value);
            }
        }

        if (!string.IsNullOrWhiteSpace(job.RequestSignature))
        {
            request.Headers.Add("X-Signature", job.RequestSignature);
        }

        request.Headers.Add("X-Application", "Squidex Webhook");

        return await httpClient.OneWayRequestAsync(request, job.RequestBody, ct);
    }
}

public sealed class WebhookJob
{
    public WebhookMethod Method { get; set; }

    public string RequestUrl { get; set; }

    public string RequestSignature { get; set; }

    public string RequestBody { get; set; }

    public string RequestBodyType { get; set; }

    public Dictionary<string, string> Headers { get; set; }
}
