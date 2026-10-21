// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using System.Text;
using System.Text.Json;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
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
        var json = JsonSerializer.Serialize(tools.Select(x => x.ProtocolTool).OrderBy(x => x.Name), McpJsonUtilities.DefaultOptions);

        await VerifyJson(json);
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

        var result = await CallAsync(mcp, "get_schema", new () { ["schema"] = _.SchemaName });

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

        var query = EqualsQuery($"data.{TestEntityData.StringField}.iv", value);

        var result = await CallAsync(mcp, "query_contents", new () { ["schema"] = _.SchemaName, ["query"] = query });

        await VerifyJson(result);
    }

    [Fact]
    public async Task Should_query_contents_with_odata()
    {
        // STEP 1: Create published content items with a unique value.
        var value = Guid.NewGuid().ToString();

        await _.Contents.CreateAsync(new TestEntityData { String = value, Number = 1 }, ContentCreateOptions.AsPublish);
        await _.Contents.CreateAsync(new TestEntityData { String = value, Number = 2 }, ContentCreateOptions.AsPublish);


        // STEP 2: Query the content items with filter and sorting.
        await using var mcp = await _.CreateMcpClientAsync();

        var odata = $"$filter=data/{TestEntityData.StringField}/iv eq '{value}'&$orderby=data/{TestEntityData.NumberField}/iv desc";

        var result = await CallAsync(mcp, "query_contents", new () { ["schema"] = _.SchemaName, ["odata"] = odata });

        await VerifyJson(result);
    }

    [Fact]
    public async Task Should_only_query_unpublished_contents_if_requested()
    {
        // STEP 1: Create an unpublished content item with a unique value.
        var value = Guid.NewGuid().ToString();

        await _.Contents.CreateAsync(new TestEntityData { String = value });


        // STEP 2: Query the content item.
        await using var mcp = await _.CreateMcpClientAsync();

        var query = EqualsQuery($"data.{TestEntityData.StringField}.iv", value);

        var published = await CallAsync(mcp, "query_contents", new () { ["schema"] = _.SchemaName, ["query"] = query });
        var unpublished = await CallAsync(mcp, "query_contents", new () { ["schema"] = _.SchemaName, ["query"] = query, ["unpublished"] = true });

        await VerifyJson($$"""{"published": {{published}}, "unpublished": {{unpublished}}}""");
    }

    [Fact]
    public async Task Should_get_content()
    {
        // STEP 1: Create a published content item.
        var content = await _.Contents.CreateAsync(new TestEntityData { Number = 42 }, ContentCreateOptions.AsPublish);


        // STEP 2: Get the content item.
        await using var mcp = await _.CreateMcpClientAsync();

        var result = await CallAsync(mcp, "get_content", new () { ["schema"] = _.SchemaName, ["id"] = content.Id });

        await VerifyJson(result);
    }

    [Fact]
    public async Task Should_return_error_if_schema_not_found()
    {
        await using var mcp = await _.CreateMcpClientAsync();

        var result = await mcp.CallToolAsync("get_content", new Dictionary<string, object?> { ["schema"] = "unknown", ["id"] = Guid.NewGuid().ToString() });

        await Verify(Simplify(result));
    }

    [Fact]
    public async Task Should_return_error_if_odata_query_is_invalid()
    {
        await using var mcp = await _.CreateMcpClientAsync();

        var result = await mcp.CallToolAsync("query_contents", new Dictionary<string, object?> { ["schema"] = _.SchemaName, ["odata"] = "$filter=invalid(" });

        await Verify(Simplify(result));
    }

    [Fact]
    public async Task Should_get_asset()
    {
        // STEP 1: Upload an asset.
        var asset = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");


        // STEP 2: Get the asset.
        await using var mcp = await _.CreateMcpClientAsync();

        var result = await CallAsync(mcp, "get_asset", new () { ["id"] = asset.Id });

        await VerifyJson(result).ScrubInlineGuids();
    }

    [Fact]
    public async Task Should_query_assets_with_json()
    {
        // STEP 1: Upload an asset.
        var asset = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");


        // STEP 2: Query the asset.
        await using var mcp = await _.CreateMcpClientAsync();

        var query = EqualsQuery("id", asset.Id);

        var result = await CallAsync(mcp, "query_assets", new () { ["query"] = query });

        await VerifyJson(result).ScrubInlineGuids();
    }

    [Fact]
    public async Task Should_query_assets_with_odata()
    {
        // STEP 1: Upload an asset.
        var asset = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");


        // STEP 2: Query the asset.
        await using var mcp = await _.CreateMcpClientAsync();

        var result = await CallAsync(mcp, "query_assets", new () { ["odata"] = $"$filter=id eq '{asset.Id}'" });

        await VerifyJson(result).ScrubInlineGuids();
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
        var (app, _) = await this._.PostAppAsync();


        // STEP 2: Make the client anonymous.
        await app.Apps.PutClientAsync("default", new UpdateClientDto { AllowAnonymous = true });


        // STEP 3: Use MCP without authentication.
        using var httpClient = new HttpClient();

        await using var mcp = await this._.CreateMcpClientAsync(httpClient, app.Options.AppName);

        var result = await mcp.CallToolAsync("list_schemas");

        await Verify(Simplify(result));
    }

    private static async Task<string> CallAsync(McpClient mcp, string tool, Dictionary<string, object?>? arguments = null)
    {
        var result = await mcp.CallToolAsync(tool, arguments ?? []);

        var text = Assert.Single(result.Content.OfType<TextContentBlock>()).Text;
        Assert.NotEqual(true, result.IsError);

        return text;
    }

    private static object Simplify(CallToolResult result)
    {
        // The metadata contains the server version and would make the snapshots brittle.
        return new
        {
            result.IsError,
            Content = result.Content.OfType<TextContentBlock>().Select(x => x.Text).ToList(),
        };
    }

    private static string EqualsQuery(string path, string value)
    {
        return new JObject
        {
            ["filter"] = new JObject { ["path"] = path, ["op"] = "eq", ["value"] = value },
        }.ToString();
    }

    private static Task<HttpResponseMessage> PostToolsListAsync(HttpClient httpClient, string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent("""{"jsonrpc":"2.0","id":1,"method":"tools/list"}""", Encoding.UTF8, "application/json"),
        };

        request.Headers.Accept.ParseAdd("application/json");
        request.Headers.Accept.ParseAdd("text/event-stream");

        return httpClient.SendAsync(request);
    }
}
