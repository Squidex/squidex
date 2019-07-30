// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public class ContentQueryParserTests
    {
        private readonly ISchemaEntity schema;
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-app");
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

            sut = new ContentQueryParser(cache, JsonHelper.DefaultSerializer, options);
        }

        [Fact]
        public void Should_throw_if_odata_query_is_invalid()
        {
            var query = Q.Empty.WithODataQuery("$filter=invalid");

            Assert.Throws<ValidationException>(() => sut.ParseQuery(requestContext, schema, query));
        }

        [Fact]
        public void Should_throw_if_json_query_is_invalid()
        {
            var query = Q.Empty.WithJsonQuery("invalid");

            Assert.Throws<ValidationException>(() => sut.ParseQuery(requestContext, schema, query));
        }

        [Fact]
        public void Should_parse_odata_query()
        {
            var query = Q.Empty.WithODataQuery("$top=100&$orderby=data/firstName/iv asc&$search=Hello World");

            var parsed = sut.ParseQuery(requestContext, schema, query);

            Assert.Equal("FullText: 'Hello World'; Take: 100; Sort: data.firstName.iv Ascending", parsed.ToString());
        }

        [Fact]
        public void Should_parse_odata_query_and_enrich_with_defaults()
        {
            var query = Q.Empty.WithODataQuery("$top=200&$filter=data/firstName/iv eq 'ABC'");

            var parsed = sut.ParseQuery(requestContext, schema, query);

            Assert.Equal("Filter: data.firstName.iv == 'ABC'; Take: 200; Sort: lastModified Descending", parsed.ToString());
        }

        [Fact]
        public void Should_parse_json_query_and_enrich_with_defaults()
        {
            var query = Q.Empty.WithJsonQuery(Json("{ 'filter': { 'path': 'data.firstName.iv', 'op': 'eq', 'value': 'ABC' } }"));

            var parsed = sut.ParseQuery(requestContext, schema, query);

            Assert.Equal("Filter: data.firstName.iv == 'ABC'; Take: 30; Sort: lastModified Descending", parsed.ToString());
        }

        [Fact]
        public void Should_parse_json_full_text_query_and_enrich_with_defaults()
        {
            var query = Q.Empty.WithJsonQuery(Json("{ 'fullText': 'Hello' }"));

            var parsed = sut.ParseQuery(requestContext, schema, query);

            Assert.Equal("FullText: 'Hello'; Take: 30; Sort: lastModified Descending", parsed.ToString());
        }

        [Fact]
        public void Should_apply_default_page_size()
        {
            var query = Q.Empty;

            var parsed = sut.ParseQuery(requestContext, schema, query);

            Assert.Equal("Take: 30; Sort: lastModified Descending", parsed.ToString());
        }

        [Fact]
        public void Should_limit_number_of_contents()
        {
            var query = Q.Empty.WithODataQuery("$top=300&$skip=20");

            var parsed = sut.ParseQuery(requestContext, schema, query);

            Assert.Equal("Skip: 20; Take: 200; Sort: lastModified Descending", parsed.ToString());
        }

        private static string Json(string text)
        {
            return text.Replace('\'', '"');
        }
    }
}
