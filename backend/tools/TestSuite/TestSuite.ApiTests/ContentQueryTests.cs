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
            var items = await _.Contents.GetAsync(new ODataQuery { OrderBy = "data/number/iv asc" });

            var itemsById = await _.Contents.GetAsync(new HashSet<Guid>(items.Items.Take(3).Select(x => x.EntityId)));

            Assert.Equal(3, itemsById.Items.Count);
            Assert.Equal(3, itemsById.Total);

            foreach (var item in itemsById.Items)
            {
                Assert.Equal(_.AppName, item.AppName);
                Assert.Equal(_.SchemaName, item.SchemaName);
            }
        }

        [Fact]
        public async Task Should_return_all()
        {
            var items = await _.Contents.GetAsync(new ODataQuery { OrderBy = "data/number/iv asc" });

            AssertItems(items, 10, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }

        [Fact]
        public async Task Should_return_items_with_skip()
        {
            var items = await _.Contents.GetAsync(new ODataQuery { Skip = 5, OrderBy = "data/number/iv asc" });

            AssertItems(items, 10, new[] { 6, 7, 8, 9, 10 });
        }

        [Fact]
        public async Task Should_return_items_with_skip_and_top()
        {
            var items = await _.Contents.GetAsync(new ODataQuery { Skip = 2, Top = 5, OrderBy = "data/number/iv asc" });

            AssertItems(items, 10, new[] { 3, 4, 5, 6, 7 });
        }

        [Fact]
        public async Task Should_return_items_with_ordering()
        {
            var items = await _.Contents.GetAsync(new ODataQuery { Skip = 2, Top = 5, OrderBy = "data/number/iv desc" });

            AssertItems(items, 10, new[] { 8, 7, 6, 5, 4 });
        }

        [Fact]
        public async Task Should_return_items_with_filter()
        {
            var items = await _.Contents.GetAsync(new ODataQuery { Filter = "data/number/iv gt 3 and data/number/iv lt 7", OrderBy = "data/number/iv asc" });

            AssertItems(items, 3, new[] { 4, 5, 6 });
        }

        [Fact]
        public async Task Should_query_items_with_graphql()
        {
            var query = new
            {
                query = @"
                {
                    queryNumbersContents(filter: ""data/number/iv gt 3 and data/number/iv lt 7"", orderby: ""data/number/iv asc"") {
                      id,
                      data {
                        value {
                          iv
                        }
                      }
                    }
                }"
            };

            var result = await _.Contents.GraphQlAsync<QueryResult>(query);

            var items = result.Items;

            Assert.Equal(items.Select(x => x.Data.Value).ToArray(), new[] { 4, 5, 6 });
        }

        [Fact]
        public async Task Should_query_items_with_graphql_with_dynamic()
        {
            var query = new
            {
                query = @"
                {
                    queryNumbersContents(filter: ""data/number/iv gt 3 and data/number/iv lt 7"", orderby: ""data/number/iv asc"") {
                      id,
                      data {
                        value {
                          iv
                        }
                      }
                    }
                }"
            };

            var result = await _.Contents.GraphQlAsync<JObject>(query);

            var items = result["queryNumbersContents"];

            Assert.Equal(items.Select(x => x["data"]["value"]["iv"].Value<int>()).ToArray(), new[] { 4, 5, 6 });
        }

        private sealed class QueryResult
        {
            [JsonProperty("queryNumbersContents")]
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
            public int Value { get; set; }
        }

        private void AssertItems(SquidexEntities<TestEntity, TestEntityData> entities, int total, int[] expected)
        {
            Assert.Equal(total, entities.Total);
            Assert.Equal(expected, entities.Items.Select(x => x.Data.Number).ToArray());
        }
    }
}
