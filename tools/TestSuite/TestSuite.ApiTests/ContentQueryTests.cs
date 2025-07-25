﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.ClientLibrary;
using TestSuite.Model;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class ContentQueryTests(ContentQueryFixture fixture) : IClassFixture<ContentQueryFixture>
{
    public ContentQueryFixture _ { get; } = fixture;

    [Fact]
    public async Task Should_query_with_old_fields()
    {
        var context =
            QueryContext.Default
                .WithFields(TestEntityData.StringField);

        var items_0 = await _.Client.DynamicContents(_.SchemaName).GetAsync(context: context);

        Assert.True(items_0.Items.TrueForAll(x => x.Data.Count == 1));
    }

    [Fact]
    public async Task Should_query_with_new_fields()
    {
        var context =
            QueryContext.Default
                .WithFields($"data.{TestEntityData.StringField}");

        var items_0 = await _.Client.DynamicContents(_.SchemaName).GetAsync(context: context);

        Assert.True(items_0.Items.TrueForAll(x => x.Data.Count == 1 && x.Data.ContainsKey(TestEntityData.StringField)));
    }


    [Fact]
    public async Task Should_find_one()
    {
        var all = await _.Client.DynamicContents(_.SchemaName).GetAsync();

        var content = await _.Client.DynamicContents(_.SchemaName).GetAsync(all.Items[0].Id);

        Assert.NotNull(content);
    }


    [Fact]
    public async Task Should_find_one_with_fields()
    {
        var all = await _.Client.DynamicContents(_.SchemaName).GetAsync();

        var context =
            QueryContext.Default
                .WithFields($"data.{TestEntityData.StringField}");

        var content = await _.Client.DynamicContents(_.SchemaName).GetAsync(all.Items[0].Id, context);

        Assert.NotNull(content);
        Assert.Single(content.Data);
        Assert.Contains(TestEntityData.StringField, content.Data);
    }

    [Fact]
    public async Task Should_query_newly_created_schema()
    {
        for (var i = 0; i < 20; i++)
        {
            var schemaName = $"schema-{Guid.NewGuid()}";

            await TestEntity.CreateSchemaAsync(_.Client.Schemas, schemaName);

            var contentClient = _.Client.Contents<TestEntity, TestEntityData>(schemaName);
            var contentItems = await contentClient.GetAsync();

            Assert.Equal(0, contentItems.Total);
        }
    }

    [Fact]
    public async Task Should_query_by_ids()
    {
        var q = new ContentQuery { OrderBy = "data/number/iv asc" };

        var items_0 = await _.Contents.GetAsync(q);
        var itemsIds = items_0.Items.Take(3).Select(x => x.Id).ToHashSet();

        var items_1 = await _.Contents.GetAsync(new ContentQuery { Ids = itemsIds });

        Assert.Equal(3, items_1.Items.Count);
        Assert.Equal(3, items_1.Total);

        foreach (var item in items_1.Items)
        {
            Assert.Equal(_.SchemaName, item.SchemaName);
        }
    }

    [Fact]
    public async Task Should_query_by_ids_across_schemas()
    {
        var q = new ContentQuery { OrderBy = "data/number/iv asc" };

        var items_0 = await _.Contents.GetAsync(q);
        var itemsIds = items_0.Items.Take(3).Select(x => x.Id).ToHashSet();

        var items_1 = await _.Client.SharedDynamicContents.GetAsync(itemsIds);

        Assert.Equal(3, items_1.Items.Count);
        Assert.Equal(3, items_1.Total);

        foreach (var item in items_1.Items)
        {
            Assert.Equal(_.SchemaName, item.SchemaName);
        }
    }

    [Fact]
    public async Task Should_query_by_ids_filter()
    {
        var q_0 = new ContentQuery { Filter = "data/number/iv gt 3 and data/number/iv lt 7", OrderBy = "data/number/iv asc" };

        var items_0 = await _.Contents.GetAsync(q_0);

        var q_1 = new ContentQuery
        {
            JsonQuery = new
            {
                sort = new[]
                {
                    new
                    {
                        path = "data.number.iv",
                    },
                },
                filter = new
                {
                    or = items_0.Items.Select(x => new
                    {
                        path = "id",
                        op = "eq",
                        value = x.Id,
                    }).ToArray(),
                },
            },
        };

        var items_1 = await _.Contents.GetAsync(q_1);

        AssertItems(items_0, 3, [4, 5, 6]);
        AssertItems(items_1, 3, [4, 5, 6]);
    }

    [Fact]
    public async Task Should_query_with_all()
    {
        var values = new List<int>();

        await _.Contents.GetAllAsync(content =>
        {
            values.Add(content.Data.Number);
            return Task.CompletedTask;
        });

        Assert.Equal([1, 2, 3, 4, 5, 6, 7, 8, 9, 10], values.Order().ToArray());
    }

    [Fact]
    public async Task Should_query_with_streaming()
    {
        var values = new List<int>();

        await _.Contents.StreamAllAsync(content =>
        {
            values.Add(content.Data.Number);
            return Task.CompletedTask;
        });

        Assert.Equal([1, 2, 3, 4, 5, 6, 7, 8, 9, 10], values.Order().ToArray());
    }

    [Fact]
    public async Task Should_query_all_with_odata()
    {
        var q = new ContentQuery { OrderBy = "data/number/iv asc" };

        var items = await _.Contents.GetAsync(q);

        AssertItems(items, 10, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
    }

    [Fact]
    public async Task Should_query_all_with_json()
    {
        var q = new ContentQuery
        {
            JsonQuery = new
            {
                sort = new[]
                {
                    new
                    {
                        path = "data.number.iv",
                    },
                },
            },
        };

        var items = await _.Contents.GetAsync(q);

        AssertItems(items, 10, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
    }

    [Fact]
    public async Task Should_query_random_with_odata()
    {
        var q = new ContentQuery { Random = 5 };

        var items = await _.Contents.GetAsync(q);

        Assert.Equal(5, items.Items.Count);
    }

    [Fact]
    public async Task Should_query_random_with_json()
    {
        var q = new ContentQuery
        {
            JsonQuery = new
            {
                random = 5,
            },
        };

        var items = await _.Contents.GetAsync(q);

        Assert.Equal(5, items.Items.Count);
    }

    [Fact]
    public async Task Should_query_by_skip_with_odata()
    {
        var q = new ContentQuery { OrderBy = "data/number/iv asc", Skip = 5 };

        var items = await _.Contents.GetAsync(q);

        AssertItems(items, 10, [6, 7, 8, 9, 10]);
    }

    [Fact]
    public async Task Should_query_by_skip_with_json()
    {
        var q = new ContentQuery
        {
            JsonQuery = new
            {
                sort = new[]
                {
                    new
                    {
                        path = "data.number.iv",
                    },
                },
                skip = 5,
            },
        };

        var items = await _.Contents.GetAsync(q);

        AssertItems(items, 10, [6, 7, 8, 9, 10]);
    }

    [Fact]
    public async Task Should_query_by_skip_and_top_with_odata()
    {
        var q = new ContentQuery { Skip = 2, Top = 5, OrderBy = "data/number/iv asc" };

        var items = await _.Contents.GetAsync(q);

        AssertItems(items, 10, [3, 4, 5, 6, 7]);
    }

    [Fact]
    public async Task Should_query_by_skip_and_top_with_json()
    {
        var q = new ContentQuery
        {
            JsonQuery = new
            {
                skip = 2,
                sort = new[]
                {
                    new
                    {
                        path = "data.number.iv",
                    },
                },
                top = 5,
            },
        };

        var items = await _.Contents.GetAsync(q);

        AssertItems(items, 10, [3, 4, 5, 6, 7]);
    }

    [Fact]
    public async Task Should_query_by_filter_with_odata()
    {
        var q = new ContentQuery { Filter = "data/number/iv gt 3 and data/number/iv lt 7", OrderBy = "data/number/iv asc" };

        var items = await _.Contents.GetAsync(q);

        AssertItems(items, 3, [4, 5, 6]);
    }

    [Fact]
    public async Task Should_query_by_filter_with_json()
    {
        var q = new ContentQuery
        {
            JsonQuery = new
            {
                sort = new[]
                {
                    new
                    {
                        path = "data.number.iv",
                    },
                },
                filter = new
                {
                    and = new[]
                    {
                        new
                        {
                            path = "data.number.iv",
                            op = "gt",
                            value = 3,
                        },
                        new
                        {
                            path = "data.number.iv",
                            op = "lt",
                            value = 7,
                        },
                    },
                },
            },
        };

        var items = await _.Contents.GetAsync(q);

        AssertItems(items, 3, [4, 5, 6]);
    }

    [Fact]
    public async Task Should_query_by_json_filter_with_json()
    {
        var q = new ContentQuery
        {
            JsonQuery = new
            {
                sort = new[]
                {
                    new
                    {
                        path = "data.json.iv.nested1.nested2",
                    },
                },
                filter = new
                {
                    and = new[]
                    {
                        new
                        {
                            path = "data.json.iv.nested1.nested2",
                            op = "gt",
                            value = 3,
                        },
                        new
                        {
                            path = "data.json.iv.nested1.nested2",
                            op = "lt",
                            value = 7,
                        },
                    },
                },
            },
        };

        var items = await _.Contents.GetAsync(q);

        AssertItems(items, 3, [4, 5, 6]);
    }

    [Fact]
    public async Task Should_query_by_full_text_with_odata()
    {
        var q = new ContentQuery { Search = "text2" };

        var items = await _.Contents.PollAsync(q, x => true);

        AssertItems(items, 1, [2]);
    }

    [Fact]
    public async Task Should_query_by_full_text_with_json()
    {
        var q = new ContentQuery
        {
            JsonQuery = new
            {
                fullText = "text2",
            },
        };

        var items = await _.Contents.PollAsync(q, x => true);

        AssertItems(items, 1, [2]);
    }

    [Fact]
    public async Task Should_query_by_near_location_with_odata()
    {
        var q = new ContentQuery { Filter = "geo.distance(data/geo/iv, geography'POINT(103 3)') lt 1000" };

        var items = await _.Contents.PollAsync(q, x => true);

        AssertItems(items, 1, [3]);
    }

    [Fact]
    public async Task Should_query_by_near_location_with_json()
    {
        var q = new ContentQuery
        {
            JsonQuery = new
            {
                filter = new
                {
                    path = "data.geo.iv",
                    op = "lt",
                    value = new
                    {
                        longitude = 103,
                        latitude = 3,
                        distance = 1000,
                    },
                },
            },
        };

        var items = await _.Contents.PollAsync(q, x => true);

        AssertItems(items, 1, [3]);
    }

    [Fact]
    public async Task Should_query_by_near_geoson_location_with_odata()
    {
        var q = new ContentQuery { Filter = "geo.distance(data/geo/iv, geography'POINT(104 4)') lt 1000" };

        var items = await _.Contents.PollAsync(q, x => true);

        AssertItems(items, 1, [4]);
    }

    [Fact]
    public async Task Should_query_json_with_dot()
    {
        TestEntity? content = null;
        try
        {
            // STEP 1: Create a content item with a text that caused a bug before.
            content = await _.Contents.CreateAsync(
                new TestEntityData
                {
                    Json = new JObject
                    {
                        ["search.field.with.dot"] = 42,
                    },
                },
                ContentCreateOptions.AsPublish);


            // STEP 2: Get the item and ensure that the text is the same.
            var q = new ContentQuery
            {
                JsonQuery = new
                {
                    filter = new
                    {
                        and = new[]
                        {
                            new
                            {
                                path = "data.json.iv.search\\.field\\.with\\.dot",
                                op = "eq",
                                value = 42,
                            },
                        },
                    },
                },
            };

            var queried = await _.Contents.GetAsync(q);

            Assert.Equal(42, (int)queried.Items[0].Data.Json!["search.field.with.dot"]!);
        }
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Fact]
    public async Task Should_query_by_near_geoson_location_with_json()
    {
        var q = new ContentQuery
        {
            JsonQuery = new
            {
                filter = new
                {
                    path = "data.geo.iv",
                    op = "lt",
                    value = new
                    {
                        longitude = 104,
                        latitude = 4,
                        distance = 1000,
                    },
                },
            },
        };

        var items = await _.Contents.PollAsync(q, x => true);

        AssertItems(items, 1, [4]);
    }

    [Fact]
    public async Task Should_create_and_query_with_inline_graphql()
    {
        var query = new
        {
            query = @"
                    mutation {
                        createMyReadsContent(data: {
                            number: {
                                iv: 555
                            }
                        }) {
                            id,
                            data {
                                number {
                                    iv
                                }
                            }
                        }
                    }",
        };

        var result = await _.Client.SharedDynamicContents.GraphQlAsync<JObject>(query);

        var value = result?["createMyReadsContent"]?["data"]?["number"]?["iv"]?.Value<int>();

        Assert.Equal(555, value);
    }

    [Fact]
    public async Task Should_create_and_query_with_variable_graphql()
    {
        var query = new
        {
            query = @"
                    mutation Mutation($data: MyReadsDataInputDto!) {
                        createMyReadsContent(data: $data) {
                            id,
                            data {
                                number {
                                    iv
                                }
                            }
                        }
                    }",
            variables = new
            {
                data = new
                {
                    number = new
                    {
                        iv = 998,
                    },
                },
            },
        };

        var result = await _.Client.SharedDynamicContents.GraphQlAsync<JObject>(query);

        var value = result?["createMyReadsContent"]?["data"]?["number"]?["iv"]?.Value<int>();

        Assert.Equal(998, value);
    }

    [Fact]
    public async Task Should_query_with_graphql_batching()
    {
        var query1 = new
        {
            query = @"
                    query ContentsQuery($filter: String!) {
                        queryMyReadsContents(filter: $filter, orderby: ""data/number/iv asc"") {
                            id,
                            data {
                                number {
                                    iv
                                }
                            }
                        }
                    }",
            variables = new
            {
                filter = @"data/number/iv gt 3 and data/number/iv lt 7",
            },
        };

        var query2 = new
        {
            query = @"
                    query ContentsQuery($filter: String!) {
                        queryMyReadsContents(filter: $filter, orderby: ""data/number/iv asc"") {
                            id,
                            data {
                                number {
                                    iv
                                }
                            }
                        }
                    }",
            variables = new
            {
                filter = @"data/number/iv gt 4 and data/number/iv lt 7",
            },
        };

        var results = await _.Client.SharedDynamicContents.GraphQlAsync<QueryResult>([query1, query2]);

        var items1 = results.ElementAt(0).Data.Items;
        var items2 = results.ElementAt(1).Data.Items;

        Assert.Equal([4, 5, 6], items1.Select(x => x.Data.Number).ToArray());
        Assert.Equal([5, 6], items2.Select(x => x.Data.Number).ToArray());
    }

    [Fact]
    public async Task Should_query_with_graphql()
    {
        var query = new
        {
            query = @"
                    query ContentsQuery($filter: String!) {
                        queryMyReadsContents(filter: $filter, orderby: ""data/number/iv asc"") {
                            id,
                            data {
                                number {
                                    iv
                                }
                            }
                        }
                    }",
            variables = new
            {
                filter = @"data/number/iv gt 3 and data/number/iv lt 7",
            },
        };

        var result = await _.Client.SharedDynamicContents.GraphQlAsync<QueryResult>(query);
        var items = result.Items;

        Assert.Equal([4, 5, 6], items.Select(x => x.Data.Number).ToArray());
    }

    [Fact]
    public async Task Should_query_with_graphql_get()
    {
        var query = new
        {
            query = @"
                    query ContentsQuery($filter: String!) {
                        queryMyReadsContents(filter: $filter, orderby: ""data/number/iv asc"") {
                            id,
                            data {
                                number {
                                    iv
                                }
                            }
                        }
                    }",
            variables = new
            {
                filter = @"data/number/iv gt 3 and data/number/iv lt 7",
            },
        };

        var result = await _.Client.SharedDynamicContents.GraphQlGetAsync<QueryResult>(query);
        var items = result.Items;

        Assert.Equal([4, 5, 6], items.Select(x => x.Data.Number).ToArray());
    }

    [Fact]
    public async Task Should_query_with_graphql_with_dynamic()
    {
        var query = new
        {
            query = @"
                {
                    queryMyReadsContents(filter: ""data/number/iv gt 3 and data/number/iv lt 7"", orderby: ""data/number/iv asc"") {
                      id,
                      data {
                        number {
                          iv
                        }
                      }
                    }
                }",
        };

        var result = await _.Client.SharedDynamicContents.GraphQlAsync<JObject>(query);
        var items = result["queryMyReadsContents"];

        Assert.Equal([4, 5, 6], items?.Select(x => x["data"]?["number"]?["iv"]?.Value<int>() ?? -1).ToArray() ?? []);
    }

    [Fact]
    public async Task Should_query_with_grapqhl_complex_search()
    {
        var query = new
        {
            query = @"
                    query ContentsQuery($search: String!) {
                        queryMyReadsContents(search: $search) {
                            id,
                            data {
                                number {
                                    iv
                                }
                            }
                        }
                    }",
            variables = new
            {
                search = @"The answer is 42",
            },
        };

        await _.Client.SharedDynamicContents.GraphQlAsync<QueryResult>(query);
    }

    private sealed class QueryResult
    {
        [JsonProperty("queryMyReadsContents")]
        public QueryItem[] Items { get; set; }
    }

    private sealed class QueryItem
    {
        public Guid Id { get; set; }

        public QueryItemData Data { get; set; }
    }

    private sealed class QueryItemData
    {
        [JsonConverter(typeof(InvariantConverter))]
        public int Number { get; set; }
    }

    private static void AssertItems(ContentsResult<TestEntity, TestEntityData> entities, int total, int[] expected)
    {
        Assert.Equal(total, entities.Total);
        Assert.Equal(expected, entities.Items.Select(x => x.Data.Number).ToArray());
    }
}
