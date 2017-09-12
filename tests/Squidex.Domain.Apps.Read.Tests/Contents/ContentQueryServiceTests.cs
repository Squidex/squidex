// ==========================================================================
//  ContentQueryServiceTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.OData.UriParser;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Contents.Edm;
using Squidex.Domain.Apps.Read.Contents.Repositories;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Xunit;

namespace Squidex.Domain.Apps.Read.Contents
{
    public class ContentQueryServiceTests
    {
        private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
        private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
        private readonly ISchemaProvider schemas = A.Fake<ISchemaProvider>();
        private readonly ISchemaEntity schema = A.Fake<ISchemaEntity>();
        private readonly IContentEntity content = A.Fake<IContentEntity>();
        private readonly IAppEntity app = A.Fake<IAppEntity>();
        private readonly Guid appId = Guid.NewGuid();
        private readonly Guid schemaId = Guid.NewGuid();
        private readonly Guid contentId = Guid.NewGuid();
        private readonly NamedContentData data = new NamedContentData();
        private readonly NamedContentData transformedData = new NamedContentData();
        private readonly ClaimsPrincipal user;
        private readonly ClaimsIdentity identity = new ClaimsIdentity();
        private readonly EdmModelBuilder modelBuilder = A.Fake<EdmModelBuilder>();
        private readonly ContentQueryService sut;

        public ContentQueryServiceTests()
        {
            user = new ClaimsPrincipal(identity);

            A.CallTo(() => app.Id).Returns(appId);

            A.CallTo(() => content.Id).Returns(contentId);
            A.CallTo(() => content.Data).Returns(data);
            A.CallTo(() => content.Status).Returns(Status.Published);

            sut = new ContentQueryService(contentRepository, schemas, scriptEngine, modelBuilder);
        }

        [Fact]
        public async Task Should_return_schema_from_id_if_string_is_guid()
        {
            A.CallTo(() => schemas.FindSchemaByIdAsync(schemaId, false))
                .Returns(schema);

            var result = await sut.FindSchemaAsync(app, schemaId.ToString());

            Assert.Equal(schema, result);
        }

        [Fact]
        public async Task Should_return_schema_from_name_if_string_not_guid()
        {
            A.CallTo(() => schemas.FindSchemaByNameAsync(appId, "my-schema"))
                .Returns(schema);

            var result = await sut.FindSchemaAsync(app, "my-schema");

            Assert.Equal(schema, result);
        }

        [Fact]
        public async Task Should_throw_if_schema_not_found()
        {
            A.CallTo(() => schemas.FindSchemaByNameAsync(appId, "my-schema"))
                .Returns((ISchemaEntity)null);

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.FindSchemaAsync(app, "my-schema"));
        }

        [Fact]
        public async Task Should_return_content_from_repository_and_transform()
        {
            A.CallTo(() => schemas.FindSchemaByIdAsync(schemaId, false))
                .Returns(schema);
            A.CallTo(() => contentRepository.FindContentAsync(app, schema, contentId))
                .Returns(content);

            A.CallTo(() => schema.ScriptQuery)
                .Returns("<script-query>");

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.That.Matches(x => x.User == user && x.ContentId == contentId && ReferenceEquals(x.Data, data)), "<query-script>"))
                .Returns(transformedData);

            var result = await sut.FindContentAsync(app, schemaId.ToString(), user, contentId);

            Assert.Equal(schema, result.Schema);
            Assert.Equal(data, result.Content.Data);
            Assert.Equal(content.Id, result.Content.Id);
        }

        [Fact]
        public async Task Should_throw_if_content_to_find_does_not_exist()
        {
            A.CallTo(() => schemas.FindSchemaByIdAsync(schemaId, false))
                .Returns(schema);
            A.CallTo(() => contentRepository.FindContentAsync(app, schema, contentId))
                .Returns((IContentEntity)null);

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(async () => await sut.FindContentAsync(app, schemaId.ToString(), user, contentId));
        }

        [Fact]
        public async Task Should_return_non_archived_contents_from_repository_and_transform()
        {
            await TestManyRequest(true, false, Status.Draft, Status.Published);
        }

        [Fact]
        public async Task Should_return_archived_contents_from_repository_and_transform()
        {
            await TestManyRequest(true, true, Status.Archived);
        }

        [Fact]
        public async Task Should_return_draft_contents_from_repository_and_transform()
        {
            await TestManyRequest(false, false, Status.Published);
        }

        [Fact]
        public async Task Should_return_draft_contents_from_repository_and_transform_when_requesting_archive_as_non_frontend()
        {
            await TestManyRequest(false, true, Status.Published);
        }

        private async Task TestManyRequest(bool isFrontend, bool archive, params Status[] status)
        {
            if (isFrontend)
            {
                identity.AddClaim(new Claim(OpenIdClaims.ClientId, "squidex-frontend"));
            }

            var ids = new HashSet<Guid>();

            A.CallTo(() => schemas.FindSchemaByIdAsync(schemaId, false))
                .Returns(schema);
            A.CallTo(() => contentRepository.QueryAsync(app, schema, A<Status[]>.That.IsSameSequenceAs(status), ids, A<ODataUriParser>.Ignored))
                .Returns(new List<IContentEntity> { content });
            A.CallTo(() => contentRepository.CountAsync(app, schema, A<Status[]>.That.IsSameSequenceAs(status), ids, A<ODataUriParser>.Ignored))
                .Returns(123);

            A.CallTo(() => schema.ScriptQuery)
                .Returns("<script-query>");

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.That.Matches(x => x.User == user && x.ContentId == contentId && ReferenceEquals(x.Data, data)), "<query-script>"))
                .Returns(transformedData);

            var result = await sut.QueryWithCountAsync(app, schemaId.ToString(), user, archive, ids, null);

            Assert.Equal(123, result.Total);
            Assert.Equal(schema, result.Schema);
            Assert.Equal(data, result.Items[0].Data);
            Assert.Equal(content.Id, result.Items[0].Id);
        }
    }
}
