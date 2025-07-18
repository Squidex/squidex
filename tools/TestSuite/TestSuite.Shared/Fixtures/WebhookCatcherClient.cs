// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;

#pragma warning disable MA0048 // File name must match type name

namespace TestSuite.Fixtures;

public sealed class WebhookSession
{
    public string Uuid { get; set; }
}

public sealed class WebhookRequest
{
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; }

    [JsonPropertyName("method")]
    public string Method { get; set; }

    [JsonIgnore]
    public string Content { get; set; }

    [JsonIgnore]
    public Dictionary<string, string> Headers { get; set; } = [];

    [JsonPropertyName("request_payload_base64")]
    public string EncodedContent { get; set; }

    [JsonPropertyName("headers")]
    public WebhookHeader[] HeaderList { get; set; }
}

public sealed class WebhookHeader
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}

public sealed class WebhookCatcherClient
{
    private readonly HttpClient httpClient;

    public string EndpointHost { get; }

    public int EndpointPort { get; }

    public WebhookCatcherClient(string apiHost, int apiPort, string endpointHost, int endpointPort)
    {
        if (string.IsNullOrWhiteSpace(apiHost))
        {
            apiHost = "localhost";
        }

        if (string.IsNullOrWhiteSpace(endpointHost))
        {
            endpointHost = "localhost";
        }

        EndpointHost = endpointHost;
        EndpointPort = endpointPort;

        httpClient = new HttpClient
        {
            BaseAddress = new Uri($"http://{apiHost}:{apiPort}"),
        };
    }

    public async Task<(string, string)> CreateSessionAsync(
        CancellationToken ct = default)
    {
        var request = new { status_code = 200, };
        var response = await httpClient.PostAsJsonAsync("/api/session", request, ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<WebhookSession>(ct);
        return ($"http://{EndpointHost}:{EndpointPort}/{json!.Uuid}", json!.Uuid);
    }

    public async Task<WebhookRequest[]> GetRequestsAsync(string sessionId,
        CancellationToken ct = default)
    {
        var response = await httpClient.GetFromJsonAsync<WebhookRequest[]>($"/api/session/{sessionId}/requests", ct);

        foreach (var request in response!)
        {
            var content = request.EncodedContent;
            if (content != null)
            {
                request.Content = Encoding.Default.GetString(Convert.FromBase64String(content));
            }

            if (request.HeaderList != null)
            {
                foreach (var header in request.HeaderList)
                {
                    request.Headers[header.Name] = header.Value;
                }
            }
        }

        return response;
    }
}
