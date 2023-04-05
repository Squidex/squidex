// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net.Http.Json;
using Newtonsoft.Json.Linq;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Utils;
using TestSuite.Model;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public sealed class GraphQLTests : IClassFixture<GraphQLFixture>
{
    public GraphQLFixture _ { get; }

    public GraphQLTests(GraphQLFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_query_json()
    {
        // STEP 1: Create a content with JSON.
        var content_0 = await _.Contents.CreateAsync(new TestEntityData
        {
            Json = JToken.FromObject(new
            {
                value = 1,
                obj = new
                {
                    value = 2
                }
            })
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Query this content.
        var query = new
        {
            query = @"
                {
                    findMyWritesContent(id: ""<ID>"") {
                        flatData {
                            json
                        }   
                    }
                }".Replace("<ID>", content_0.Id, StringComparison.Ordinal)
        };

        var result = await _.Client.SharedDynamicContents.GraphQlAsync<JToken>(query);

        Assert.Equal(1, result["findMyWritesContent"]["flatData"]["json"]["value"].Value<int>());
        Assert.Equal(2, result["findMyWritesContent"]["flatData"]["json"]["obj"]["value"].Value<int>());
    }

    [Fact]
    public async Task Should_query_graphql_by_reference_selector()
    {
        var query = new
        {
            query = @"
                {
                    countries: queryCountriesContents {
                        data: flatData {
                            name,
                            states {
                                data: flatData {
                                    name
                                    cities {
                                        data: flatData {
                                            name
                                        }
                                    }
                                }
                            }
                        }
                    }
                }"
        };

        var result = await _.Client.SharedDynamicContents.GraphQlAsync<JToken>(query);

        var cityNames =
            result["countries"].ToObject<List<Country>>()[0].Data.States
                .SelectMany(x => x.Data.Cities)
                .Select(x => x.Data.Name)
                .Order();

        Assert.Equal(new[] { "Leipzig", "Munich" }, cityNames);
    }

    [Fact]
    public async Task Should_query_graphql_by_references_function()
    {
        var query = new
        {
            query = @"
                {
                    countries: queryCountriesContents {
                        data: flatData {
                            name,
                            states {
                                data: flatData {
                                    name
                                },
                                cities: referencesCitiesContents {
                                    data: flatData {
                                        name
                                    }
                                }
                            }
                        }
                    }
                }"
        };

        var result = await _.Client.SharedDynamicContents.GraphQlAsync<JToken>(query);

        var cityNames =
            result["countries"]
                .SelectMany(x => x["data"]["states"])
                .SelectMany(x => x["cities"])
                .Select(x => x["data"]["name"].Value<string>())
                .Order();

        Assert.Equal(new[] { "Leipzig", "Munich" }, cityNames);
    }

    [Fact]
    public async Task Should_query_graphql_by_references_function_and_filter()
    {
        var query = new
        {
            query = @"
                {
                    countries: queryCountriesContents {
                        data: flatData {
                            name,
                            states {
                                data: flatData {
                                    name
                                },
                                cities: referencesCitiesContents(filter: ""data/name/iv eq 'Leipzig'"") {
                                    data: flatData {
                                        name
                                    }
                                }
                            }
                        }
                    }
                }"
        };

        var result = await _.Client.SharedDynamicContents.GraphQlAsync<JToken>(query);

        var cityNames =
            result["countries"]
                .SelectMany(x => x["data"]["states"])
                .SelectMany(x => x["cities"])
                .Select(x => x["data"]["name"].Value<string>())
                .Order();

        Assert.Equal(new[] { "Leipzig" }, cityNames);
    }

    [Fact]
    public async Task Should_query_graphql_by_referencing_function()
    {
        var query = new
        {
            query = @"
                {
                    cities: queryCitiesContents {
                        states: referencingStatesContents {
                            data: flatData {
                                name
                            }
                        }
                    }
                }"
        };

        var result = await _.Client.SharedDynamicContents.GraphQlAsync<JToken>(query);

        var stateNames =
            result["cities"]
                .SelectMany(x => x["states"])
                .Select(x => x["data"]["name"].Value<string>())
                .Order();

        Assert.Equal(new[] { "Bavaria", "Saxony" }, stateNames);
    }

    [Fact]
    public async Task Should_query_graphql_by_referencing_function_and_filter()
    {
        var query = new
        {
            query = @"
                {
                    cities: queryCitiesContents {
                        states: referencingStatesContents(filter: ""data/name/iv eq 'Saxony'"") {
                            data: flatData {
                                name
                            }
                        }
                    }
                }"
        };

        var result = await _.Client.SharedDynamicContents.GraphQlAsync<JToken>(query);

        var stateNames =
            result["cities"]
                .SelectMany(x => x["states"])
                .Select(x => x["data"]["name"].Value<string>())
                .Order();

        Assert.Equal(new[] { "Saxony" }, stateNames);
    }

    [Fact]
    public async Task Should_query_dynamic_data()
    {
        var query = new
        {
            query = @"
                {
                    cities: queryCitiesContents {
                        data__dynamic
                    }
                }"
        };

        var result = await _.Client.SharedDynamicContents.GraphQlAsync<JToken>(query);

        var cityNames =
            result["cities"]
                .Select(x => x["data__dynamic"]["name"]["iv"].Value<string>())
                .Order();

        Assert.Equal(new[] { "Leipzig", "Munich" }, cityNames);
    }

    [Fact]
    public async Task Should_query_correct_content_type_for_graphql()
    {
        var query = new
        {
            query = @"
                {
                    queryCitiesContents {
                        id
                    }
                }"
        };

        var httpClient = _.Client.CreateHttpClient();

        // Create the request manually to check the content type.
        var response = await httpClient.PostAsync(_.Client.GenerateUrl($"api/content/{_.AppName}/graphql/batch"), query.ToContent());

        Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
    }

    [Fact]
    public async Task Should_query_graphql_by_ids()
    {
        var allCities = await _.Cities.GetAsync();
        var allStates = await _.States.GetAsync();

        var ids = allCities.Items.Select(x => x.Id).Union(allStates.Items.Select(x => x.Id)).ToList();

        var query = new
        {
            query = @"
                query ContentsQuery($ids: [String!]!) {
                    queryContentsByIds(ids: $ids) {
                        ... on Content {
                            id
                        }
                        ... on Cities {
                            data: flatData {
                                name
                            }
                        }
                        ... on States {
                            data: flatData {
                                name
                            }
                        }
                    }
                }",
            variables = new
            {
                ids
            }
        };

        var result = await _.Client.SharedDynamicContents.GraphQlAsync<JToken>(query);

        var names =
            result["queryContents"]
                .Select(x => x["data"]["name"].Value<string>())
                .Order();

        Assert.Equal(new[] { "Bavaria", "Leipzig", "Munich", "Saxony" }, names);
    }

    [Fact]
    public async Task Should_return_correct_vary_headers()
    {
        var query = new
        {
            query = @"
                {
                    queryCitiesContents {
                        id
                    }
                }"
        };

        var httpClient = _.Client.CreateHttpClient();

        // Create the request manually to check the headers.
        var response = await httpClient.PostAsJsonAsync($"api/content/{_.AppName}/graphql", query);

        Assert.Equal(new string[]
        {
            "Auth-State",
            "X-Flatten",
            "X-Languages",
            "X-NoCleanup",
            "X-NoEnrichment",
            "X-NoResolveLanguages",
            "X-Resolve-Urls",
            "X-ResolveFlow",
            "X-Unpublished"
        }, response.Headers.Vary.Order().ToArray());
    }
}
