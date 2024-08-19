// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net.Http.Json;
using FluentAssertions;
using Newtonsoft.Json;
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
    public async Task Should_query_assets()
    {
        var asset = await _.Client.Assets.UploadFileAsync("Assets/logo-squared.png", "image/png");

        var query = new
        {
            query = @"
                query FindAsset($id: String!) {
                    findAsset(id: $id) {
                        id
                        version
                        created
                        createdBy
                        createdByUser {
                          id
                          email
                          displayName
                        }
                        editToken
                        lastModified
                        lastModifiedBy
                        lastModifiedByUser {
                          id
                          email
                          displayName
                        }
                        url
                        thumbnailUrl
                        mimeType
                        fileName
                        fileHash
                        fileSize
                        fileVersion
                        isImage
                        isProtected
                        pixelWidth
                        pixelHeight
                        parentId
                        tags
                        type
                        metadataText
                        metadataPixelWidth: metadata(path: ""pixelWidth"")
                        metadataUnknown: metadata(path: ""unknown"")
                        metadata
                        slug  
                    }
                }",
            variables = new
            {
                id = asset.Id,
            }
        };

        var result = await _.Client.SharedDynamicContents.GraphQlAsync<JToken>(query);

        var settings = new VerifySettings();
        settings.IgnoreMember("editToken");
        settings.IgnoreMember("thumbnailUrl");
        settings.IgnoreMember("url");
        settings.IgnoreMember("version");

        await VerifyJson(JsonConvert.SerializeObject(result), settings);
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
                query ContentsQuery($id: String!) {
                    findMyWritesContent(id: $id) {
                        flatData {
                            json
                        }   
                    }
                }",
            variables = new
            {
                id = content_0.Id,
            }
        };

        var result = await _.Client.SharedDynamicContents.GraphQlAsync<JToken>(query);

        Assert.Equal(1, result?["findMyWritesContent"]?["flatData"]?["json"]?["value"]?.Value<int>());
        Assert.Equal(2, result?["findMyWritesContent"]?["flatData"]?["json"]?["obj"]?["value"]?.Value<int>());
    }

    [Fact]
    public async Task Should_query_graphql_with_components()
    {
        var query = new
        {
            query = @"
                {
                    cities: queryCitiesContents {
                        data: flatData {
                            name,
                            topLocation {
                                name
                            },
                            locations {
                                name
                            }
                        }
                    }
                }"
        };

        var result = await _.Client.SharedDynamicContents.GraphQlAsync<JToken>(query);

        var cities = result?["cities"]?.ToObject<List<City>>();

        cities.Should().BeEquivalentTo(new List<City>
        {
            new City
            {
                Data = new CityData
                {
                    Name = "Leipzig",
                    TopLocation = new LocationData
                    {
                        Name = "Leipzig Top Location"
                    },
                    Locations =
                    [
                        new LocationData
                        {
                            Name = "Leipzig Location 1"
                        },
                        new LocationData
                        {
                            Name = "Leipzig Location 2"
                        },
                    ],
                }
            },
            new City
            {
                Data = new CityData
                {
                    Name = "Munich",
                    TopLocation = new LocationData
                    {
                        Name = "Munich Top Location"
                    },
                    Locations =
                    [
                        new LocationData
                        {
                            Name = "Munich Location 1"
                        },
                        new LocationData
                        {
                            Name = "Munich Location 2"
                        },
                    ],
                }
            }
        });
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
            result!["countries"]!.ToObject<List<Country>>()![0].Data.States
                .SelectMany(x => x.Data.Cities)
                .Select(x => x.Data.Name)
                .Order();

        Assert.Equal(["Leipzig", "Munich"], cityNames);
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
            result!["countries"]!
                .SelectMany(x => x["data"]!["states"]!)
                .SelectMany(x => x["cities"]!)
                .Select(x => x["data"]!["name"]!.Value<string>())
                .Order();

        Assert.Equal(["Leipzig", "Munich"], cityNames);
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
            result["countries"]!
                .SelectMany(x => x["data"]!["states"]!)
                .SelectMany(x => x["cities"]!)
                .Select(x => x["data"]!["name"]!.Value<string>())
                .Order();

        Assert.Equal(["Leipzig"], cityNames);
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
            result["cities"]!
                .SelectMany(x => x["states"]!)
                .Select(x => x["data"]!["name"]!.Value<string>())
                .Order();

        Assert.Equal(["Bavaria", "Saxony"], stateNames);
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
            result!["cities"]!
                .SelectMany(x => x!["states"]!)
                .Select(x => x!["data"]!["name"]!.Value<string>())
                .Order();

        Assert.Equal(["Saxony"], stateNames);
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
            result["cities"]!
                .Select(x => x["data__dynamic"]!["name"]!["iv"]!.Value<string>())
                .Order();

        Assert.Equal(["Leipzig", "Munich"], cityNames);
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

        var url = _.Client.GenerateUrl($"api/content/{_.AppName}/graphql/batch");

        // Create the request manually to check the content type.
        var httpClient = _.Client.CreateHttpClient();
        var httpResponse = await httpClient.PostAsync(url, query.ToContent(_.Client.Options));

        Assert.Equal("application/json", httpResponse.Content.Headers.ContentType?.MediaType);
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
            result["queryContentsByIds"]!
                .Select(x => x["data"]!["name"]!.Value<string>())
                .Order();

        Assert.Equal(["Bavaria", "Leipzig", "Munich", "Saxony"], names);
    }

    [Fact]
    public async Task Should_query_multiple_items_with_separate_queries()
    {
        var allCities = await _.Cities.GetAsync();

        var query = new
        {
            query = @"
                query ContentsQuery($id1: String!, $id2: String!) {
                    a: findCitiesContent(id: $id1, version: 0) {
                        id,
                        flatData {
                            name
                        }
                    },
                    b: findCitiesContent(id: $id2, version: 0) {
                        id,
                        flatData {
                            name
                        }
                    }
                }",
            variables = new
            {
                id1 = allCities.Items[0].Id,
                id2 = allCities.Items[1].Id,
            }
        };

        var result = await _.Client.SharedDynamicContents.GraphQlAsync<JToken>(query);

        var city1Id = result!["a"]!["id"]!.ToString();
        var city2Id = result!["b"]!["id"]!.ToString();

        Assert.Equal(allCities.Items[0].Id, city1Id);
        Assert.Equal(allCities.Items[1].Id, city2Id);
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

        // Create the request manually to check the headers.
        var httpClient = _.Client.CreateHttpClient();
        var httpResponse = await httpClient.PostAsJsonAsync($"api/content/{_.AppName}/graphql", query);

        Assert.Equal(new[]
        {
            "Auth-State",
            "X-Fields",
            "X-Flatten",
            "X-Languages",
            "X-NoCleanup",
            "X-NoDefaults",
            "X-NoEnrichment",
            "X-NoResolveLanguages",
            "X-ResolveFlow",
            "X-ResolveUrls",
            "X-Unpublished"
        }, httpResponse.Headers.Vary.Order().ToArray());
    }
}
