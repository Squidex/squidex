// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets.Queries
{
    public class AssetQueryParserTests
    {
        private readonly ITagService tagService = A.Fake<ITagService>();
        private readonly Context requestContext = new Context();
        private readonly AssetQueryParser sut;

        public AssetQueryParserTests()
        {
            var options = Options.Create(new AssetOptions { DefaultPageSize = 30 });

            sut = new AssetQueryParser(JsonHelper.DefaultSerializer, tagService, options);
        }

        [Fact]
        public void Should_transform_odata_query()
        {
            var query = Q.Empty.WithODataQuery("$top=100&$orderby=fileName asc&$search=Hello World");

            var parsed = sut.ParseQuery(requestContext, query);

            Assert.Equal("FullText: 'Hello World'; Take: 100; Sort: fileName Ascending", parsed.ToString());
        }

        [Fact]
        public void Should_transform_odata_query_and_enrich_with_defaults()
        {
            var query = Q.Empty.WithODataQuery("$top=200&$filter=fileName eq 'ABC'");

            var parsed = sut.ParseQuery(requestContext, query);

            Assert.Equal("Filter: fileName == 'ABC'; Take: 200; Sort: lastModified Descending", parsed.ToString());
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
    }
}
