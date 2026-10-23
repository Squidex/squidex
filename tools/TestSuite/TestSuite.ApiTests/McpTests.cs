// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.ClientLibrary;
using TestSuite.Model;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class McpTests(McpFixture fixture) : IClassFixture<McpFixture>
{
    public McpFixture _ { get; } = fixture;

    [Fact]
    public async Task Should_list_tools()
    {
        await using var mcp = await _.CreateMcpClientAsync();

        var tools = await mcp.ListToolsAsync();

        // Snapshot the full definitions, because the descriptions and schemas are what the agents see.
        await VerifyJson(System.Text.Json.JsonSerializer.Serialize(
            tools.Select(x => x.ProtocolTool).OrderBy(x => x.Name),
            McpJsonUtilities.DefaultOptions));
    }

    [Fact]
    public async Task Should_list_schemas()
    {
        await using var mcp = await _.CreateMcpClientAsync();

        // The fixture app is shared with other tests, which create their own schemas.
        var result = JToken.Parse(await CallAsync(mcp, "list_schemas"));

        Assert.Contains(result, x => (string?)x["name"] == _.SchemaName);
    }

    [Fact]
    public async Task Should_get_schema()
    {
        await using var mcp = await _.CreateMcpClientAsync();

        var result = await CallAsync(mcp, "get_schema", new ()
        {
            ["schema"] = _.SchemaName,
        });

        await VerifyJson(result);
    }

    [Fact]
    public async Task Should_query_contents_with_json()
    {
        // STEP 1: Create a published content item with a unique value.
        var value = Guid.NewGuid().ToString();

        await _.Contents.CreateAsync(new TestEntityData { String = value }, ContentCreateOptions.AsPublish);


        // STEP 2: Query the content item.
        await using var mcp = await _.CreateMcpClientAsync();

        var result = await CallAsync(mcp, "query_contents", new ()
        {
            ["schema"] = _.SchemaName,
            ["query"] = JsonConvert.SerializeObject(new
            {
                filter = new { path = $"data.{TestEntityData.StringField}.iv", op = "eq", value },
            }),
        });

        await VerifyJson(result);
    }

    [Fact]
    public async Task Should_query_contents_with_odata()
    {
        // STEP 1: Create published content items with a unique value.
        var value = Guid.NewGuid().ToString();

        await _.Contents.CreateAsync(
            new TestEntityData { String = value, Number = 1 },
            ContentCreateOptions.AsPublish);

        await _.Contents.CreateAsync(
            new TestEntityData { String = value, Number = 2 },
            ContentCreateOptions.AsPublish);


        // STEP 2: Query the content items with filter and sorting.
        await using var mcp = await _.CreateMcpClientAsync();

        var result = await CallAsync(mcp, "query_contents", new ()
        {
            ["schema"] = _.SchemaName,
            ["odata"] = $"$filter=data/{TestEntityData.StringField}/iv eq '{value}'&$orderby=data/{TestEntityData.NumberField}/iv desc",
        });

        await VerifyJson(result);
    }

    [Fact]
    public async Task Should_not_query_unpublished_contents_by_default()
    {
        // STEP 1: Create an unpublished content item with a unique value.
        var value = Guid.NewGuid().ToString();

        await _.Contents.CreateAsync(new TestEntityData { String = value });


        // STEP 2: Query the content item.
        await using var mcp = await _.CreateMcpClientAsync();

        var result = await CallAsync(mcp, "query_contents", new ()
        {
            ["schema"] = _.SchemaName,
            ["query"] = JsonConvert.SerializeObject(new
            {
                filter = new { path = $"data.{TestEntityData.StringField}.iv", op = "eq", value },
            }),
        });

        await VerifyJson(result);
    }

    [Fact]
    public async Task Should_query_unpublished_contents_if_requested()
    {
        // STEP 1: Create an unpublished content item with a unique value.
        var value = Guid.NewGuid().ToString();

        await _.Contents.CreateAsync(new TestEntityData { String = value });


        // STEP 2: Query the content item.
        await using var mcp = await _.CreateMcpClientAsync();

        var result = await CallAsync(mcp, "query_contents", new ()
        {
            ["schema"] = _.SchemaName,
            ["query"] = JsonConvert.SerializeObject(new
            {
                filter = new { path = $"data.{TestEntityData.StringField}.iv", op = "eq", value },
            }),
            ["unpublished"] = true,
        });

        await VerifyJson(result);
    }

    [Fact]
    public async Task Should_get_content()
    {
        // STEP 1: Create a published content item.
        var content = await _.Contents.CreateAsync(
            new TestEntityData { Number = 42 },
            ContentCreateOptions.AsPublish);


        // STEP 2: Get the content item.
        await using var mcp = await _.CreateMcpClientAsync();

        var result = await CallAsync(mcp, "get_content", new ()
        {
            ["schema"] = _.SchemaName,
            ["id"] = content.Id,
        });

        await VerifyJson(result);
    }

    [Fact]
    public async Task Should_return_error_if_schema_not_found()
    {
        await using var mcp = await _.CreateMcpClientAsync();

        var result = await mcp.CallToolAsync("get_content", new Dictionary<string, object?>
        {
            ["schema"] = "unknown",
            ["id"] = Guid.NewGuid().ToString(),
        });

        await Verify(result);
    }

    [Fact]
    public async Task Should_return_error_if_odata_query_is_invalid()
    {
        await using var mcp = await _.CreateMcpClientAsync();

        var result = await mcp.CallToolAsync("query_contents", new Dictionary<string, object?>
        {
            ["schema"] = _.SchemaName,
            ["odata"] = "$filter=invalid(",
        });

        await Verify(result);
    }

    [Fact]
    public async Task Should_get_asset()
    {
        // STEP 1: Upload an asset.
        var asset = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");


        // STEP 2: Get the asset.
        await using var mcp = await _.CreateMcpClientAsync();

        var result = await CallAsync(mcp, "get_asset", new ()
        {
            ["id"] = asset.Id,
        });

        await VerifyJson(result)
            .ScrubInlineGuids()
            .AddScrubber(x => x.Replace(_.Url.TrimEnd('/'), "{url}"));
    }

    [Fact]
    public async Task Should_query_assets_with_json()
    {
        // STEP 1: Upload an asset.
        var asset = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");


        // STEP 2: Query the asset.
        await using var mcp = await _.CreateMcpClientAsync();

        var result = await CallAsync(mcp, "query_assets", new ()
        {
            ["query"] = JsonConvert.SerializeObject(new
            {
                filter = new { path = "id", op = "eq", value = asset.Id },
            }),
        });

        await VerifyJson(result)
            .ScrubInlineGuids()
            .AddScrubber(x => x.Replace(_.Url.TrimEnd('/'), "{url}"));
    }

    [Fact]
    public async Task Should_query_assets_with_odata()
    {
        // STEP 1: Upload an asset.
        var asset = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");


        // STEP 2: Query the asset.
        await using var mcp = await _.CreateMcpClientAsync();

        var result = await CallAsync(mcp, "query_assets", new ()
        {
            ["odata"] = $"$filter=id eq '{asset.Id}'",
        });

        await VerifyJson(result)
            .ScrubInlineGuids()
            .AddScrubber(x => x.Replace(_.Url.TrimEnd('/'), "{url}"));
    }

    [Fact]
    public async Task Should_return_401_if_not_authenticated()
    {
        using var httpClient = new HttpClient();

        var response = await PostToolsListAsync(httpClient, _.McpUrl(_.AppName));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_return_404_if_app_not_found()
    {
        using var httpClient = _.Client.CreateHttpClient();

        var response = await PostToolsListAsync(httpClient, _.McpUrl(Guid.NewGuid().ToString()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_allow_anonymous_access_if_client_is_anonymous()
    {
        // STEP 1: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 2: Make the client anonymous.
        await app.Apps.PutClientAsync("default", new UpdateClientDto { AllowAnonymous = true });


        // STEP 3: Use MCP without authentication.
        using var httpClient = new HttpClient();

        await using var mcp = await _.CreateMcpClientAsync(httpClient, app.Options.AppName);

        var result = await mcp.CallToolAsync("list_schemas");

        await Verify(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_authenticate_with_client_secret(bool fullClientId)
    {
        // STEP 1: Create app and get the secret of the default client.
        var (app, _) = await _.PostAppAsync();

        var client = (await app.Apps.GetClientsAsync()).Items.Single(x => x.Id == "default");


        // STEP 2: Use MCP with the client secret.
        using var httpClient = CreateBasicClient(
            fullClientId ? $"{app.Options.AppName}:{client.Id}" : client.Id,
            client.Secret);

        await using var mcp = await _.CreateMcpClientAsync(httpClient, app.Options.AppName);

        var result = await mcp.CallToolAsync("list_schemas");

        await Verify(result);
    }

    [Fact]
    public async Task Should_return_401_if_client_secret_is_invalid()
    {
        // STEP 1: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 2: Use MCP with a wrong secret.
        using var httpClient = CreateBasicClient("default", "invalid");

        var response = await PostToolsListAsync(httpClient, _.McpUrl(app.Options.AppName));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_return_401_if_client_secret_is_used_for_other_app()
    {
        // STEP 1: Create two apps.
        var (app1, _) = await _.PostAppAsync();
        var (app2, _) = await _.PostAppAsync();

        var client = (await app1.Apps.GetClientsAsync()).Items.Single(x => x.Id == "default");


        // STEP 2: Use the secret of the first app for the second app.
        using var httpClient = CreateBasicClient($"{app1.Options.AppName}:{client.Id}", client.Secret);

        var response = await PostToolsListAsync(httpClient, _.McpUrl(app2.Options.AppName));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_not_accept_client_secret_outside_of_mcp()
    {
        // STEP 1: Create app.
        var (app, _) = await _.PostAppAsync();

        var client = (await app.Apps.GetClientsAsync()).Items.Single(x => x.Id == "default");


        // STEP 2: Use the client secret for the REST API.
        using var httpClient = CreateBasicClient(client.Id, client.Secret);

        var response = await httpClient.GetAsync(
            _.Client.GenerateUrl($"api/apps/{app.Options.AppName}/settings"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static HttpClient CreateBasicClient(string clientId, string secret)
    {
        var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{secret}")));
        return httpClient;
    }

    private static async Task<string> CallAsync(McpClient mcp, string tool,
        Dictionary<string, object?>? arguments = null)
    {
        var result = await mcp.CallToolAsync(tool, arguments ?? []);

        var text = Assert.Single(result.Content.OfType<TextContentBlock>()).Text;
        Assert.NotEqual(true, result.IsError);

        return text;
    }

    private static Task<HttpResponseMessage> PostToolsListAsync(HttpClient httpClient, string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(
                """{"jsonrpc":"2.0","id":1,"method":"tools/list"}""",
                Encoding.UTF8,
                "application/json"),
        };

        request.Headers.Accept.ParseAdd("application/json");
        request.Headers.Accept.ParseAdd("text/event-stream");

        return httpClient.SendAsync(request);
    }
}
