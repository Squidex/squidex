// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.ClientLibrary;
using TestSuite.Fixtures;
using TestSuite.Model;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests
{
    public class ContentQueryTests : IClassFixture<ContentQueryFixture>
    {
        public ContentQueryFixture _ { get; }

        public ContentQueryTests(ContentQueryFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_query_by_ids()
        {
            var items = await _.Contents.GetAsync(new ContentQuery { OrderBy = "data/number/iv asc" });

            var itemsById = await _.Contents.GetAsync(new HashSet<string>(items.Items.Take(3).Select(x => x.Id)));

            Assert.Equal(3, itemsById.Items.Count);
            Assert.Equal(3, itemsById.Total);

            foreach (var item in itemsById.Items)
            {
                Assert.Equal(_.AppName, item.AppName);
                Assert.Equal(_.SchemaName, item.SchemaName);
            }
        }

        [Fact]
        public async Task Should_return_all_with_odata()
        {
            var items = await _.Contents.GetAsync(new ContentQuery { OrderBy = "data/number/iv asc" });

            AssertItems(items, 10, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }

        [Fact]
        public async Task Should_return_all_with_json()
        {
            var items = await _.Contents.GetAsync(new ContentQuery
            {
                JsonQuery = new
                {
                    sort = new[]
                    {
                        new
                        {
                            path = "data.number.iv", order = "ascending"
                        }
                    }
                }
            });

            AssertItems(items, 10, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }

        [Fact]
        public async Task Should_return_items_with_skip_with_odata()
        {
            var items = await _.Contents.GetAsync(new ContentQuery { Skip = 5, OrderBy = "data/number/iv asc" });

            AssertItems(items, 10, new[] { 6, 7, 8, 9, 10 });
        }

        [Fact]
        public async Task Should_return_items_with_skip_with_json()
        {
            var items = await _.Contents.GetAsync(new ContentQuery
            {
                JsonQuery = new
                {
                    sort = new[]
                    {
                        new
                        {
                            path = "data.number.iv", order = "ascending"
                        }
                    },
                    skip = 5
                }
            });

            AssertItems(items, 10, new[] { 6, 7, 8, 9, 10 });
        }

        [Fact]
        public async Task Should_return_items_with_skip_and_top_with_odata()
        {
            var items = await _.Contents.GetAsync(new ContentQuery { Skip = 2, Top = 5, OrderBy = "data/number/iv asc" });

            AssertItems(items, 10, new[] { 3, 4, 5, 6, 7 });
        }

        [Fact]
        public async Task Should_return_items_with_skip_and_top_with_json()
        {
            var items = await _.Contents.GetAsync(new ContentQuery
            {
                JsonQuery = new
                {
                    sort = new[]
                    {
                        new
                        {
                            path = "data.number.iv", order = "ascending"
                        }
                    },
                    skip = 2, top = 5
                }
            });

            AssertItems(items, 10, new[] { 3, 4, 5, 6, 7 });
        }

        [Fact]
        public async Task Should_return_items_with_filter_with_odata()
        {
            var items = await _.Contents.GetAsync(new ContentQuery { Filter = "data/number/iv gt 3 and data/number/iv lt 7", OrderBy = "data/number/iv asc" });

            AssertItems(items, 3, new[] { 4, 5, 6 });
        }

        [Fact]
        public async Task Should_return_items_with_filter_with_json()
        {
            var items = await _.Contents.GetAsync(new ContentQuery
            {
                JsonQuery = new
                {
                    sort = new[]
                    {
                        new
                        {
                            path = "data.number.iv", order = "ascending"
                        }
                    },
                    filter = new
                    {
                        and = new[]
                        {
                            new
                            {
                                path = "data.number.iv",
                                op = "gt",
                                value = 3
                            },
                            new
                            {
                                path = "data.number.iv",
                                op = "lt",
                                value = 7
                            }
                        }
                    }
                }
            });

            AssertItems(items, 3, new[] { 4, 5, 6 });
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
                                iv: 999
                            }
                        }) {
                            id,
                            data {
                                number {
                                    iv
                                }
                            }
                        }
                    }"
            };

            var result = await _.Contents.GraphQlAsync<JObject>(query);

            var value = result["createMyReadsContent"]["data"]["number"]["iv"].Value<int>();

            Assert.Equal(999, value);
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
                            iv = 998
                        }
                    }
                }
            };

            var result = await _.Contents.GraphQlAsync<JObject>(query);

            var value = result["createMyReadsContent"]["data"]["number"]["iv"].Value<int>();

            Assert.Equal(998, value);
        }

        [Fact]
        public async Task Should_query_items_with_graphql()
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
                    filter = @"data/number/iv gt 3 and data/number/iv lt 7"
                }
            };

            var result = await _.Contents.GraphQlAsync<QueryResult>(query);

            var items = result.Items;

            Assert.Equal(items.Select(x => x.Data.Number).ToArray(), new[] { 4, 5, 6 });
        }

        [Fact]
        public async Task Should_query_items_with_graphql_get()
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
                    filter = @"data/number/iv gt 3 and data/number/iv lt 7"
                }
            };

            var result = await _.Contents.GraphQlGetAsync<QueryResult>(query);

            var items = result.Items;

            Assert.Equal(items.Select(x => x.Data.Number).ToArray(), new[] { 4, 5, 6 });
        }

        [Fact]
        public async Task Should_query_items_with_graphql_with_dynamic()
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
                }"
            };

            var result = await _.Contents.GraphQlAsync<JObject>(query);

            var items = result["queryMyReadsContents"];

            Assert.Equal(items.Select(x => x["data"]["number"]["iv"].Value<int>()).ToArray(), new[] { 4, 5, 6 });
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

        private void AssertItems(ContentsResult<TestEntity, TestEntityData> entities, int total, int[] expected)
        {
            Assert.Equal(total, entities.Total);
            Assert.Equal(expected, entities.Items.Select(x => x.Data.Number).ToArray());
        }
    }
}
