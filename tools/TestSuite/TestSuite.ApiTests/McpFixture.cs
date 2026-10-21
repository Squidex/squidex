// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using ModelContextProtocol.Client;
using TestSuite.Fixtures;

namespace TestSuite.ApiTests;

public sealed class McpFixture : TestSchemaFixtureBase
{
    public McpFixture()
        : base("my-mcp")
    {
    }

    public string McpUrl(string appName)
    {
        return Client.GenerateUrl($"api/apps/{appName}/mcp")!;
    }

    public Task<McpClient> CreateMcpClientAsync(HttpClient? httpClient = null, string? appName = null)
    {
        var transport = new HttpClientTransport(new HttpClientTransportOptions
        {
            Endpoint = new Uri(McpUrl(appName ?? AppName)),
        }, httpClient ?? Client.CreateHttpClient());

        return McpClient.CreateAsync(transport);
    }
}
