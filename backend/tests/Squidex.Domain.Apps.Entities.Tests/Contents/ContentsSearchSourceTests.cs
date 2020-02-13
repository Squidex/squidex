// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Search;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class ContentsSearchSourceTests
    {
        private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
        private readonly IContentTextIndexer contentIndex = A.Fake<IContentTextIndexer>();
        private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly Context requestContext;
        private readonly ContentsSearchSource sut;

        public ContentsSearchSourceTests()
        {
            requestContext = new Context(Mocks.FrontendUser(), Mocks.App(appId));

            sut = new ContentsSearchSource(contentQuery, contentIndex, urlGenerator);
        }

        [Fact]
        public async Task Should_return_content_with_default_name()
        {
            var content = new ContentEntity { Id = Guid.NewGuid(), SchemaId = schemaId };

            await TestContentAsyc(content, "Content");
        }

        [Fact]
        public async Task Should_return_content_with_multiple_invariant_reference_fields()
        {
            var content = new ContentEntity
            {
                Id = Guid.NewGuid(),
                Data =
                    new NamedContentData()
                        .AddField("field1",
                            new ContentFieldData()
                                .AddJsonValue("iv", JsonValue.Create("hello")))
                        .AddField("field2",
                            new ContentFieldData()
                                .AddJsonValue("iv", JsonValue.Create("world"))),
                ReferenceFields = new[]
                {
                    Fields.String(1, "field1", Partitioning.Invariant),
                    Fields.String(2, "field2", Partitioning.Invariant)
                },
                SchemaId = schemaId
            };

            await TestContentAsyc(content, "hello, world");
        }

        [Fact]
        public async Task Should_return_content_with_invariant_reference_field()
        {
            var content = new ContentEntity
            {
                Id = Guid.NewGuid(),
                Data =
                    new NamedContentData()
                        .AddField("field",
                            new ContentFieldData()
                                .AddJsonValue("iv", JsonValue.Create("hello"))),
                ReferenceFields = new[]
                {
                    Fields.String(1, "field", Partitioning.Invariant)
                },
                SchemaId = schemaId
            };

            await TestContentAsyc(content, "hello");
        }

        [Fact]
        public async Task Should_return_content_with_localized_reference_field()
        {
            var content = new ContentEntity
            {
                Id = Guid.NewGuid(),
                Data =
                    new NamedContentData()
                        .AddField("field",
                            new ContentFieldData()
                                .AddJsonValue("en", JsonValue.Create("hello"))),
                ReferenceFields = new[]
                {
                    Fields.String(1, "field", Partitioning.Language)
                },
                SchemaId = schemaId
            };

            await TestContentAsyc(content, "hello");
        }

        [Fact]
        public async Task Should_return_content_with_invariant_field_and_reference_data()
        {
            var content = new ContentEntity
            {
                Id = Guid.NewGuid(),
                Data =
                    new NamedContentData()
                        .AddField("field",
                            new ContentFieldData()
                                .AddJsonValue("iv", JsonValue.Create("raw"))),
                ReferenceData =
                    new NamedContentData()
                        .AddField("field",
                            new ContentFieldData()
                                .AddJsonValue("en", JsonValue.Create("resolved"))),
                ReferenceFields = new[]
                {
                    Fields.String(1, "field", Partitioning.Language)
                },
                SchemaId = schemaId
            };

            await TestContentAsyc(content, "resolved");
        }

        private async Task TestContentAsyc(IEnrichedContentEntity content, string expectedName)
        {
            var ids = new List<Guid> { content.Id };

            A.CallTo(() => contentIndex.SearchAsync("query~", requestContext.App, default, requestContext.Scope()))
                .Returns(ids);

            A.CallTo(() => contentQuery.QueryAsync(requestContext, ids))
                .Returns(ResultList.CreateFrom(1, content));

            A.CallTo(() => urlGenerator.ContentUI(appId, schemaId, content.Id))
                .Returns("content-url");

            var result = await sut.SearchAsync("query", requestContext);

            result.Should().BeEquivalentTo(
                new SearchResults()
                    .Add(expectedName, SearchResultType.Content, "content-url"));
        }
    }
}
