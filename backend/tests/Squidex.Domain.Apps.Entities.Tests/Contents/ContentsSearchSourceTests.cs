﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Search;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Shared;
using Squidex.Shared.Identity;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class ContentsSearchSourceTests
    {
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
        private readonly ITextIndex contentIndex = A.Fake<ITextIndex>();
        private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly NamedId<DomainId> schemaId1 = NamedId.Of(DomainId.NewGuid(), "my-schema1");
        private readonly NamedId<DomainId> schemaId2 = NamedId.Of(DomainId.NewGuid(), "my-schema2");
        private readonly NamedId<DomainId> schemaId3 = NamedId.Of(DomainId.NewGuid(), "my-schema3");
        private readonly ContentsSearchSource sut;

        public ContentsSearchSourceTests()
        {
            A.CallTo(() => appProvider.GetSchemasAsync(appId.Id))
                .Returns(new List<ISchemaEntity>
                {
                    Mocks.Schema(appId, schemaId1),
                    Mocks.Schema(appId, schemaId2),
                    Mocks.Schema(appId, schemaId3)
                });

            sut = new ContentsSearchSource(appProvider, contentQuery, contentIndex, urlGenerator);
        }

        [Fact]
        public async Task Should_return_content_with_default_name()
        {
            var content = new ContentEntity { Id = DomainId.NewGuid(), SchemaId = schemaId1 };

            await TestContentAsyc(content, "Content");
        }

        [Fact]
        public async Task Should_return_content_with_multiple_invariant_reference_fields()
        {
            var content = new ContentEntity
            {
                Id = DomainId.NewGuid(),
                Data =
                    new ContentData()
                        .AddField("field1",
                            new ContentFieldData()
                                .AddInvariant("hello"))
                        .AddField("field2",
                            new ContentFieldData()
                                .AddInvariant("world")),
                ReferenceFields = new[]
                {
                    Fields.String(1, "field1", Partitioning.Invariant),
                    Fields.String(2, "field2", Partitioning.Invariant)
                },
                SchemaId = schemaId1
            };

            await TestContentAsyc(content, "hello, world");
        }

        [Fact]
        public async Task Should_return_content_with_invariant_reference_field()
        {
            var content = new ContentEntity
            {
                Id = DomainId.NewGuid(),
                Data =
                    new ContentData()
                        .AddField("field",
                            new ContentFieldData()
                                .AddInvariant("hello")),
                ReferenceFields = new[]
                {
                    Fields.String(1, "field", Partitioning.Invariant)
                },
                SchemaId = schemaId1
            };

            await TestContentAsyc(content, "hello");
        }

        [Fact]
        public async Task Should_return_content_with_localized_reference_field()
        {
            var content = new ContentEntity
            {
                Id = DomainId.NewGuid(),
                Data =
                    new ContentData()
                        .AddField("field",
                            new ContentFieldData()
                                .AddLocalized("en", "hello")),
                ReferenceFields = new[]
                {
                    Fields.String(1, "field", Partitioning.Language)
                },
                SchemaId = schemaId1
            };

            await TestContentAsyc(content, "hello");
        }

        [Fact]
        public async Task Should_return_content_with_invariant_field_and_reference_data()
        {
            var content = new ContentEntity
            {
                Id = DomainId.NewGuid(),
                Data =
                    new ContentData()
                        .AddField("field",
                            new ContentFieldData()
                                .AddInvariant("raw")),
                ReferenceData =
                    new ContentData()
                        .AddField("field",
                            new ContentFieldData()
                                .AddLocalized("en", "resolved")),
                ReferenceFields = new[]
                {
                    Fields.String(1, "field", Partitioning.Language)
                },
                SchemaId = schemaId1
            };

            await TestContentAsyc(content, "resolved");
        }

        [Fact]
        public async Task Should_not_invoke_content_index_if_user_has_no_permission()
        {
            var ctx = ContextWithPermissions();

            var result = await sut.SearchAsync("query", ctx, default);

            Assert.Empty(result);

            A.CallTo(() => contentIndex.SearchAsync(ctx.App, A<TextQuery>._, A<SearchScope>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_invoke_context_query_if_no_id_found()
        {
            var ctx = ContextWithPermissions(schemaId1, schemaId2);

            A.CallTo(() => contentIndex.SearchAsync(ctx.App, A<TextQuery>.That.Matches(x => x.Text == "query~"), ctx.Scope()))
                .Returns(new List<DomainId>());

            var result = await sut.SearchAsync("query", ctx, default);

            Assert.Empty(result);

            A.CallTo(() => contentQuery.QueryAsync(ctx, A<Q>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        private async Task TestContentAsyc(ContentEntity content, string expectedName)
        {
            content.AppId = appId;

            var ctx = ContextWithPermissions(schemaId1, schemaId2);

            var ids = new List<DomainId> { content.Id };

            A.CallTo(() => contentIndex.SearchAsync(ctx.App, A<TextQuery>.That.Matches(x => x.Text == "query~" && x.Filter != null), ctx.Scope()))
                .Returns(ids);

            A.CallTo(() => contentQuery.QueryAsync(ctx, A<Q>.That.HasIds(ids), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom<IEnrichedContentEntity>(1, content));

            A.CallTo(() => urlGenerator.ContentUI(appId, schemaId1, content.Id))
                .Returns("content-url");

            var result = await sut.SearchAsync("query", ctx, default);

            result.Should().BeEquivalentTo(
                new SearchResults()
                    .Add(expectedName, SearchResultType.Content, "content-url"));
        }

        private Context ContextWithPermissions(params NamedId<DomainId>[] allowedSchemas)
        {
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            foreach (var schemaId in allowedSchemas)
            {
                var permission = Permissions.ForApp(Permissions.AppContentsReadOwn, appId.Name, schemaId.Name).Id;

                claimsIdentity.AddClaim(new Claim(SquidexClaimTypes.Permissions, permission));
            }

            return new Context(claimsPrincipal, Mocks.App(appId));
        }
    }
}
