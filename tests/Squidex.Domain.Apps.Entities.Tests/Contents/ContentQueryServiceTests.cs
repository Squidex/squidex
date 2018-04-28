// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.OData.UriParser;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.Edm;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class ContentQueryServiceTests
    {
        private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
        private readonly IContentVersionLoader contentVersionLoader = A.Fake<IContentVersionLoader>();
        private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
        private readonly ISchemaEntity schema = A.Fake<ISchemaEntity>();
        private readonly IContentEntity content = A.Fake<IContentEntity>();
        private readonly IAppEntity app = A.Fake<IAppEntity>();
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly Guid appId = Guid.NewGuid();
        private readonly Guid schemaId = Guid.NewGuid();
        private readonly Guid contentId = Guid.NewGuid();
        private readonly string appName = "my-app";
        private readonly NamedContentData contentData = new NamedContentData();
        private readonly NamedContentData contentTransformed = new NamedContentData();
        private readonly ClaimsPrincipal user;
        private readonly ClaimsIdentity identity = new ClaimsIdentity();
        private readonly EdmModelBuilder modelBuilder = A.Fake<EdmModelBuilder>();
        private readonly ContentQueryService sut;

        public ContentQueryServiceTests()
        {
            user = new ClaimsPrincipal(identity);

            A.CallTo(() => app.Id).Returns(appId);
            A.CallTo(() => app.Name).Returns(appName);
            A.CallTo(() => app.LanguagesConfig).Returns(LanguagesConfig.English);

            A.CallTo(() => content.Id).Returns(contentId);
            A.CallTo(() => content.Data).Returns(contentData);
            A.CallTo(() => content.Status).Returns(Status.Published);

            A.CallTo(() => schema.SchemaDef).Returns(new Schema("my-schema"));

            sut = new ContentQueryService(contentRepository, contentVersionLoader, appProvider, scriptEngine, modelBuilder);
        }

        [Fact]
        public async Task Should_return_schema_from_id_if_string_is_guid()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(appId, schemaId, false))
                .Returns(schema);

            var result = await sut.GetSchemaAsync(app, schemaId.ToString());

            Assert.Equal(schema, result);
        }

        [Fact]
        public async Task Should_return_schema_from_name_if_string_not_guid()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(appId, "my-schema"))
                .Returns(schema);

            var result = await sut.GetSchemaAsync(app, "my-schema");

            Assert.Equal(schema, result);
        }

        [Fact]
        public async Task Should_throw_if_schema_not_found()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(appId, "my-schema"))
                .Returns((ISchemaEntity)null);

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.GetSchemaAsync(app, "my-schema"));
        }

        [Fact]
        public async Task Should_return_content_from_repository_and_transform()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(appId, schemaId, false))
                .Returns(schema);
            A.CallTo(() => contentRepository.FindContentAsync(app, schema, contentId))
                .Returns(content);

            A.CallTo(() => schema.ScriptQuery)
                .Returns("<script-query>");

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.That.Matches(x => x.User == user && x.ContentId == contentId && ReferenceEquals(x.Data, contentData)), "<query-script>"))
                .Returns(contentTransformed);

            var result = await sut.FindContentAsync(app, schemaId.ToString(), user, contentId);

            Assert.Equal(contentTransformed, result.Data);
            Assert.Equal(content.Id, result.Id);
        }

        [Fact]
        public async Task Should_return_versioned_content_from_repository_and_transform()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(appId, schemaId, false))
                .Returns(schema);
            A.CallTo(() => contentVersionLoader.LoadAsync(contentId, 10))
                .Returns(content);

            A.CallTo(() => schema.ScriptQuery)
                .Returns("<script-query>");

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.That.Matches(x => x.User == user && x.ContentId == contentId && ReferenceEquals(x.Data, contentData)), "<query-script>"))
                .Returns(contentTransformed);

            var result = await sut.FindContentAsync(app, schemaId.ToString(), user, contentId, 10);

            Assert.Equal(contentTransformed, result.Data);
            Assert.Equal(content.Id, result.Id);
        }

        [Fact]
        public async Task Should_throw_if_content_to_find_does_not_exist()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(appId, schemaId, false))
                .Returns(schema);

            A.CallTo(() => contentRepository.FindContentAsync(app, schema, contentId))
                .Returns((IContentEntity)null);

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(async () => await sut.FindContentAsync(app, schemaId.ToString(), user, contentId));
        }

        [Fact]
        public async Task Should_return_contents_with_ids_from_repository()
        {
            await TestManyIdRequest(true, false, new HashSet<Guid> { Guid.NewGuid() }, Status.Draft, Status.Published);
        }

        [Fact]
        public async Task Should_return_contents_with_ids_from_repository_and_transform_as_non_frontend()
        {
            await TestManyIdRequest(false, false, new HashSet<Guid> { Guid.NewGuid() }, Status.Published);
        }

        [Fact]
        public async Task Should_return_non_archived_contents_from_repository()
        {
            await TestManyRequest(true, false, Status.Draft, Status.Published);
        }

        [Fact]
        public async Task Should_return_non_archived_contents_from_repository_and_transform_as_non_frontend()
        {
            await TestManyRequest(false, false, Status.Published);
        }

        [Fact]
        public async Task Should_return_archived_contents_from_repository()
        {
            await TestManyRequest(true, true, Status.Archived);
        }

        [Fact]
        public async Task Should_return_draft_contents_from_repository()
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
            SetupClaims(isFrontend);

            SetupFakeWithOdataQuery(status);
            SetupFakeWithScripting();

            var result = await sut.QueryAsync(app, schemaId.ToString(), user, archive, string.Empty);

            Assert.Equal(contentData, result[0].Data);
            Assert.Equal(content.Id, result[0].Id);

            Assert.Equal(123, result.Total);

            if (!isFrontend)
            {
                A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                    .MustHaveHappened(Repeated.Exactly.Times(result.Count));
            }
            else
            {
                A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                    .MustNotHaveHappened();
            }
        }

        private async Task TestManyIdRequest(bool isFrontend, bool archive, HashSet<Guid> ids, params Status[] status)
        {
            SetupClaims(isFrontend);

            SetupFakeWithIdQuery(status, ids);
            SetupFakeWithScripting();

            var result = await sut.QueryAsync(app, schemaId.ToString(), user, archive, ids);

            Assert.Equal(contentData, result[0].Data);
            Assert.Equal(content.Id, result[0].Id);

            Assert.Equal(123, result.Total);

            if (!isFrontend)
            {
                A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                    .MustHaveHappened(Repeated.Exactly.Times(result.Count));
            }
            else
            {
                A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                    .MustNotHaveHappened();
            }
        }

        private void SetupClaims(bool isFrontend)
        {
            if (isFrontend)
            {
                identity.AddClaim(new Claim(OpenIdClaims.ClientId, "squidex-frontend"));
            }
        }

        private void SetupFakeWithIdQuery(Status[] status, HashSet<Guid> ids)
        {
            A.CallTo(() => appProvider.GetSchemaAsync(appId, schemaId, false))
                .Returns(schema);

            A.CallTo(() => contentRepository.QueryAsync(app, schema, A<Status[]>.That.IsSameSequenceAs(status), ids))
                .Returns(ResultList.Create(Enumerable.Repeat(content, 1), 123));
        }

        private void SetupFakeWithOdataQuery(Status[] status)
        {
            A.CallTo(() => appProvider.GetSchemaAsync(appId, schemaId, false))
                .Returns(schema);

            A.CallTo(() => contentRepository.QueryAsync(app, schema, A<Status[]>.That.IsSameSequenceAs(status), A<ODataUriParser>.Ignored))
                .Returns(ResultList.Create(Enumerable.Repeat(content, 1), 123));
        }

        private void SetupFakeWithScripting()
        {
            A.CallTo(() => schema.ScriptQuery)
                .Returns("<script-query>");

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.That.Matches(x => x.User == user && x.ContentId == contentId && ReferenceEquals(x.Data, contentData)), "<query-script>"))
                .Returns(contentTransformed);
        }
    }
}