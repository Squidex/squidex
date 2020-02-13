// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using FakeItEasy;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets.Queries
{
    public class AssetQueryParserTests
    {
        private readonly ITagService tagService = A.Fake<ITagService>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly Context requestContext;
        private readonly AssetQueryParser sut;

        public AssetQueryParserTests()
        {
            requestContext = new Context(Mocks.FrontendUser(), Mocks.App(appId));

            var options = Options.Create(new AssetOptions { DefaultPageSize = 30 });

            sut = new AssetQueryParser(JsonHelper.DefaultSerializer, tagService, options);
        }

        [Fact]
        public void Should_use_existing_query()
        {
            var clrQuery = new ClrQuery();

            var parsed = sut.ParseQuery(requestContext, Q.Empty.WithQuery(clrQuery));

            Assert.Same(parsed, clrQuery);
        }

        [Fact]
        public void Should_throw_if_odata_query_is_invalid()
        {
            var query = Q.Empty.WithODataQuery("$filter=invalid");

            Assert.Throws<ValidationException>(() => sut.ParseQuery(requestContext, query));
        }

        [Fact]
        public void Should_throw_if_json_query_is_invalid()
        {
            var query = Q.Empty.WithJsonQuery("invalid");

            Assert.Throws<ValidationException>(() => sut.ParseQuery(requestContext, query));
        }

        [Fact]
        public void Should_parse_odata_query()
        {
            var query = Q.Empty.WithODataQuery("$top=100&$orderby=fileName asc&$search=Hello World");

            var parsed = sut.ParseQuery(requestContext, query);

            Assert.Equal("FullText: 'Hello World'; Take: 100; Sort: fileName Ascending", parsed.ToString());
        }

        [Fact]
        public void Should_parse_odata_query_and_enrich_with_defaults()
        {
            var query = Q.Empty.WithODataQuery("$top=200&$filter=fileName eq 'ABC'");

            var parsed = sut.ParseQuery(requestContext, query);

            Assert.Equal("Filter: fileName == 'ABC'; Take: 200; Sort: lastModified Descending", parsed.ToString());
        }

        [Fact]
        public void Should_parse_json_query_and_enrich_with_defaults()
        {
            var query = Q.Empty.WithJsonQuery(Json("{ 'filter': { 'path': 'fileName', 'op': 'eq', 'value': 'ABC' } }"));

            var parsed = sut.ParseQuery(requestContext, query);

            Assert.Equal("Filter: fileName == 'ABC'; Take: 30; Sort: lastModified Descending", parsed.ToString());
        }

        [Fact]
        public void Should_parse_json_full_text_query_and_enrich_with_defaults()
        {
            var query = Q.Empty.WithJsonQuery(Json("{ 'fullText': 'Hello' }"));

            var parsed = sut.ParseQuery(requestContext, query);

            Assert.Equal("FullText: 'Hello'; Take: 30; Sort: lastModified Descending", parsed.ToString());
        }

        [Fact]
        public void Should_apply_default_page_size()
        {
            var query = Q.Empty;

            var parsed = sut.ParseQuery(requestContext, query);

            Assert.Equal("Take: 30; Sort: lastModified Descending", parsed.ToString());
        }

        [Fact]
        public void Should_limit_number_of_assets()
        {
            var query = Q.Empty.WithODataQuery("$top=300&$skip=20");

            var parsed = sut.ParseQuery(requestContext, query);

            Assert.Equal("Skip: 20; Take: 200; Sort: lastModified Descending", parsed.ToString());
        }

        [Fact]
        public void Should_denormalize_tags()
        {
            A.CallTo(() => tagService.GetTagIdsAsync(appId.Id, TagGroups.Assets, A<HashSet<string>>.That.Contains("name1")))
                .Returns(new Dictionary<string, string> { ["name1"] = "id1" });

            var query = Q.Empty.WithODataQuery("$filter=tags eq 'name1'");

            var parsed = sut.ParseQuery(requestContext, query);

            Assert.Equal("Filter: tags == 'id1'; Take: 30; Sort: lastModified Descending", parsed.ToString());
        }

        private static string Json(string text)
        {
            return text.Replace('\'', '"');
        }
    }
}
