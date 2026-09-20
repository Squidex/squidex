// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Newtonsoft.Json.Linq;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Utils;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public sealed class GraphQLLimitsTests(GraphQLFixture fixture) : IClassFixture<GraphQLFixture>
{
    public GraphQLFixture _ { get; } = fixture;

    [Fact]
    public async Task Should_execute_query_within_depth_limit()
    {
        var result = await QueryAsync(BuildNestedQuery(5));

        Assert.Null(result["errors"]);
        Assert.NotNull(result["data"]);
    }

    [Fact]
    public async Task Should_not_execute_query_over_depth_limit()
    {
        var result = await QueryAsync(BuildNestedQuery(100));

        // Deeply nested queries are rejected before they are executed.
        var errors = Assert.IsType<JArray>(result["errors"]);

        Assert.NotEmpty(errors);
    }

    private static string BuildNestedQuery(int depth)
    {
        var sb = new StringBuilder();

        sb.Append("{ __schema { types { fields { type ");

        for (var i = 0; i < depth; i++)
        {
            sb.Append("{ ofType ");
        }

        sb.Append("{ kind } ");

        for (var i = 0; i < depth; i++)
        {
            sb.Append('}');
        }

        sb.Append("} } } }");

        return sb.ToString();
    }

    private async Task<JObject> QueryAsync(string query)
    {
        var url = _.Client.GenerateUrl($"api/content/{_.AppName}/graphql");

        using var httpClient = _.Client.CreateHttpClient();

        var response = await httpClient.PostAsync(url, new { query }.ToContent(_.Client.Options));

        return JObject.Parse(await response.Content.ReadAsStringAsync());
    }
}
