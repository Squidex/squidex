// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json.Linq;
using Squidex.ClientLibrary;
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

        var result = await _.SharedContents.GraphQlAsync<JToken>(query);

        Assert.Equal(1, result["findMyWritesContent"]["flatData"]["json"]["value"].Value<int>());
        Assert.Equal(2, result["findMyWritesContent"]["flatData"]["json"]["obj"]["value"].Value<int>());
    }

    [Fact]
    public async Task Should_query_graphql_reference_selectors()
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

        var result = await _.SharedContents.GraphQlAsync<JToken>(query);

        var cityNames =
            result["countries"].ToObject<List<Country>>()[0].Data.States
                .SelectMany(x => x.Data.Cities)
                .Select(x => x.Data.Name)
                .Order();

        Assert.Equal(new[] { "Leipzig", "Stuttgart" }, cityNames);
    }

    [Fact]
    public async Task Should_query_graphql_reference_operator()
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

        var result = await _.SharedContents.GraphQlAsync<JToken>(query);

        var cityNames =
            result["countries"]
                .SelectMany(x => x["data"]["states"])
                .SelectMany(x => x["cities"])
                .Select(x => x["data"]["name"].Value<string>())
                .Order();

        Assert.Equal(new[] { "Leipzig", "Stuttgart" }, cityNames);
    }

    [Fact]
    public async Task Should_query_graphql_reference_operator_with_filter()
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

        var result = await _.SharedContents.GraphQlAsync<JToken>(query);

        var cityNames =
            result["countries"]
                .SelectMany(x => x["data"]["states"])
                .SelectMany(x => x["cities"])
                .Select(x => x["data"]["name"].Value<string>())
                .Order();

        Assert.Equal(new[] { "Leipzig" }, cityNames);
    }

    [Fact]
    public async Task Should_query_graphql_referencing_operator()
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

        var result = await _.SharedContents.GraphQlAsync<JToken>(query);

        var stateNames =
            result["cities"]
                .SelectMany(x => x["states"])
                .Select(x => x["data"]["name"].Value<string>())
                .Order();

        Assert.Equal(new[] { "Baden Württemberg", "Sachsen" }, stateNames);
    }

    [Fact]
    public async Task Should_query_graphql_referencing_operator_with_filter()
    {
        var query = new
        {
            query = @"
                {
                    cities: queryCitiesContents {
                        states: referencingStatesContents(filter: ""data/name/iv eq 'Sachsen'"") {
                            data: flatData {
                                name
                            }
                        }
                    }
                }"
        };

        var result = await _.SharedContents.GraphQlAsync<JToken>(query);

        var stateNames =
            result["cities"]
                .SelectMany(x => x["states"])
                .Select(x => x["data"]["name"].Value<string>())
                .Order();

        Assert.Equal(new[] { "Sachsen" }, stateNames);
    }
}
