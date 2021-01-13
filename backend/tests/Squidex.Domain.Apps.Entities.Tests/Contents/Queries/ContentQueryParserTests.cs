﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public class ContentQueryParserTests
    {
        private readonly ITextIndex textIndex = A.Fake<ITextIndex>();
        private readonly ISchemaEntity schema;
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly Context requestContext;
        private readonly ContentQueryParser sut;

        public ContentQueryParserTests()
        {
            var options = Options.Create(new ContentOptions { DefaultPageSize = 30 });

            requestContext = new Context(Mocks.FrontendUser(), Mocks.App(appId));

            var schemaDef =
                new Schema(schemaId.Name)
                    .AddString(1, "firstName", Partitioning.Invariant);

            schema = Mocks.Schema(appId, schemaId, schemaDef);

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

            sut = new ContentQueryParser(cache, TestUtils.DefaultSerializer, textIndex, options);
        }

        [Fact]
        public async Task Should_throw_if_odata_query_is_invalid()
        {
            var query = Q.Empty.WithODataQuery("$filter=invalid");

            await Assert.ThrowsAsync<ValidationException>(() => sut.ParseAsync(requestContext, query, schema));
        }

        [Fact]
        public async Task Should_throw_if_json_query_is_invalid()
        {
            var query = Q.Empty.WithJsonQuery("invalid");

            await Assert.ThrowsAsync<ValidationException>(() => sut.ParseAsync(requestContext, query, schema));
        }

        [Fact]
        public async Task Should_parse_odata_query_without_schema()
        {
            var query = Q.Empty.WithODataQuery("$filter=status eq 'Draft'");

            var q = await sut.ParseAsync(requestContext, query);

            Assert.Equal("Filter: status == 'Draft'; Take: 30; Sort: lastModified Descending, id Ascending", q.Query.ToString());
        }

        [Fact]
        public async Task Should_parse_json_query_without_schema()
        {
            var query = Q.Empty.WithJsonQuery("{ 'filter': { 'path': 'status', 'op': 'eq', 'value': 'Draft' } }");

            var q = await sut.ParseAsync(requestContext, query);

            Assert.Equal("Filter: status == 'Draft'; Take: 30; Sort: lastModified Descending, id Ascending", q.Query.ToString());
        }

        [Fact]
        public async Task Should_parse_odata_query()
        {
            var query = Q.Empty.WithODataQuery("$top=100&$orderby=data/firstName/iv asc&$search=Hello World");

            var q = await sut.ParseAsync(requestContext, query, schema);

            Assert.Equal("FullText: 'Hello World'; Take: 100; Sort: data.firstName.iv Ascending, id Ascending", q.Query.ToString());
        }

        [Fact]
        public async Task Should_parse_odata_query_and_enrich_with_defaults()
        {
            var query = Q.Empty.WithODataQuery("$top=200&$filter=data/firstName/iv eq 'ABC'");

            var q = await sut.ParseAsync(requestContext, query, schema);

            Assert.Equal("Filter: data.firstName.iv == 'ABC'; Take: 200; Sort: lastModified Descending, id Ascending", q.Query.ToString());
        }

        [Fact]
        public async Task Should_parse_json_query_and_enrich_with_defaults()
        {
            var query = Q.Empty.WithJsonQuery(Json("{ 'filter': { 'path': 'data.firstName.iv', 'op': 'eq', 'value': 'ABC' } }"));

            var q = await sut.ParseAsync(requestContext, query, schema);

            Assert.Equal("Filter: data.firstName.iv == 'ABC'; Take: 30; Sort: lastModified Descending, id Ascending", q.Query.ToString());
        }

        [Fact]
        public async Task Should_convert_json_query_and_enrich_with_defaults()
        {
            var query = Q.Empty.WithJsonQuery(
                new Query<IJsonValue>
                {
                    Filter = new CompareFilter<IJsonValue>("data.firstName.iv", CompareOperator.Equals, JsonValue.Create("ABC"))
                });

            var q = await sut.ParseAsync(requestContext, query, schema);

            Assert.Equal("Filter: data.firstName.iv == 'ABC'; Take: 30; Sort: lastModified Descending, id Ascending", q.Query.ToString());
        }

        [Fact]
        public async Task Should_parse_json_full_text_query_and_enrich_with_defaults()
        {
            var query = Q.Empty.WithJsonQuery(Json("{ 'fullText': 'Hello' }"));

            var q = await sut.ParseAsync(requestContext, query, schema);

            Assert.Equal("FullText: 'Hello'; Take: 30; Sort: lastModified Descending, id Ascending", q.Query.ToString());
        }

        [Fact]
        public async Task Should_convert_json_full_text_query_and_enrich_with_defaults()
        {
            var query = Q.Empty.WithJsonQuery(
                new Query<IJsonValue>
                {
                    FullText = "Hello"
                });

            var q = await sut.ParseAsync(requestContext, query, schema);

            Assert.Equal("FullText: 'Hello'; Take: 30; Sort: lastModified Descending, id Ascending", q.Query.ToString());
        }

        [Fact]
        public async Task Should_apply_default_page_size()
        {
            var query = Q.Empty;

            var q = await sut.ParseAsync(requestContext, query, schema);

            Assert.Equal("Take: 30; Sort: lastModified Descending, id Ascending", q.Query.ToString());
        }

        [Fact]
        public async Task Should_limit_number_of_contents()
        {
            var query = Q.Empty.WithODataQuery("$top=300&$skip=20");

            var q = await sut.ParseAsync(requestContext, query, schema);

            Assert.Equal("Skip: 20; Take: 200; Sort: lastModified Descending, id Ascending", q.Query.ToString());
        }

        [Fact]
        public async Task Should_not_apply_id_ordering_twice()
        {
            var query = Q.Empty.WithODataQuery("$top=300&$skip=20&$orderby=id desc");

            var q = await sut.ParseAsync(requestContext, query, schema);

            Assert.Equal("Skip: 20; Take: 200; Sort: id Descending", q.Query.ToString());
        }

        private static string Json(string text)
        {
            return text.Replace('\'', '"');
        }
    }
}
