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
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;
using Xunit;

#pragma warning disable SA1401 // Fields must be private

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public class ContentQueryServiceTests
    {
        private readonly IAppEntity app;
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly IContentEnricher contentEnricher = A.Fake<IContentEnricher>();
        private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
        private readonly IContentLoader contentVersionLoader = A.Fake<IContentLoader>();
        private readonly ISchemaEntity schema;
        private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
        private readonly Guid contentId = Guid.NewGuid();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly NamedContentData contentData = new NamedContentData();
        private readonly NamedContentData contentTransformed = new NamedContentData();
        private readonly ClaimsPrincipal user;
        private readonly ClaimsIdentity identity = new ClaimsIdentity();
        private readonly Context requestContext;
        private readonly ContentQueryParser queryParser = A.Fake<ContentQueryParser>();
        private readonly ContentQueryService sut;

        public static IEnumerable<object?[]> ApiStatusTests()
        {
            yield return new object?[]
            {
                0,
                new[] { Status.Published },
                SearchScope.Published
            };

            yield return new object?[]
            {
                1,
                null,
                SearchScope.All
            };
        }

        public ContentQueryServiceTests()
        {
            user = new ClaimsPrincipal(identity);

            app = Mocks.App(appId);

            requestContext = new Context(user, app);

            var schemaDef =
                new Schema(schemaId.Name)
                    .SetScripts(new SchemaScripts { Query = "<query-script>" });

            schema = Mocks.Schema(appId, schemaId, schemaDef);

            SetupEnricher();

            A.CallTo(() => queryParser.ParseQuery(requestContext, schema, A<Q>.Ignored))
                .Returns(new ClrQuery());

            sut = new ContentQueryService(
                appProvider,
                contentEnricher,
                contentRepository,
                contentVersionLoader,
                scriptEngine,
                queryParser);
        }

        [Fact]
        public async Task Should_return_schema_from_id_if_string_is_guid()
        {
            SetupSchemaFound();

            var ctx = requestContext;

            var result = await sut.GetSchemaOrThrowAsync(ctx, schemaId.Id.ToString());

            Assert.Equal(schema, result);
        }

        [Fact]
        public async Task Should_return_schema_from_name_if_string_not_guid()
        {
            SetupSchemaFound();

            var ctx = requestContext;

            var result = await sut.GetSchemaOrThrowAsync(ctx, schemaId.Name);

            Assert.Equal(schema, result);
        }

        [Fact]
        public async Task Should_throw_404_if_schema_not_found()
        {
            SetupSchemaNotFound();

            var ctx = requestContext;

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.GetSchemaOrThrowAsync(ctx, schemaId.Name));
        }

        [Fact]
        public async Task Should_throw_404_if_schema_not_found_in_check()
        {
            SetupSchemaNotFound();

            var ctx = requestContext;

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.GetSchemaOrThrowAsync(ctx, schemaId.Name));
        }

        [Fact]
        public async Task Should_throw_for_single_content_if_no_permission()
        {
            SetupUser(false, false);
            SetupSchemaFound();

            var ctx = requestContext;

            await Assert.ThrowsAsync<DomainForbiddenException>(() => sut.FindContentAsync(ctx, schemaId.Name, contentId));
        }

        [Fact]
        public async Task Should_throw_404_for_single_content_if_content_not_found()
        {
            var status = new[] { Status.Published };

            SetupUser(isFrontend: false);
            SetupSchemaFound();
            SetupContent(status, null, SearchScope.All);

            var ctx = requestContext;

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(async () => await sut.FindContentAsync(ctx, schemaId.Name, contentId));
        }

        [Fact]
        public async Task Should_return_single_content_for_frontend_without_transform()
        {
            var content = CreateContent(contentId);

            SetupUser(isFrontend: true);
            SetupSchemaFound();
            SetupSchemaScripting(contentId);
            SetupContent(null, content, SearchScope.All);

            var ctx = requestContext;

            var result = await sut.FindContentAsync(ctx, schemaId.Name, contentId);

            Assert.Equal(contentTransformed, result!.Data);
            Assert.Equal(content.Id, result.Id);

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Theory]
        [MemberData(nameof(ApiStatusTests))]
        public async Task Should_return_single_content_for_api_with_transform(int unpublished, Status[] status, SearchScope scope)
        {
            var content = CreateContent(contentId);

            SetupUser(isFrontend: false);
            SetupSchemaFound();
            SetupSchemaScripting(contentId);
            SetupContent(status, content, scope);

            var ctx = requestContext.WithUnpublished(unpublished == 1);

            var result = await sut.FindContentAsync(ctx, schemaId.Name, contentId);

            Assert.Equal(contentTransformed, result!.Data);
            Assert.Equal(content.Id, result.Id);

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public async Task Should_return_versioned_content_from_repository_and_transform()
        {
            var content = CreateContent(contentId);

            SetupUser(true);
            SetupSchemaFound();
            SetupSchemaScripting(contentId);

            A.CallTo(() => contentVersionLoader.GetAsync(contentId, 10))
                .Returns(content);

            var ctx = requestContext;

            var result = await sut.FindContentAsync(ctx, schemaId.Name, contentId, 10);

            Assert.Equal(contentTransformed, result!.Data);
            Assert.Equal(content.Id, result.Id);
        }

        [Fact]
        public async Task Should_throw_for_query_if_no_permission()
        {
            SetupUser(false, false);
            SetupSchemaFound();

            var ctx = requestContext;

            await Assert.ThrowsAsync<DomainForbiddenException>(() => sut.QueryAsync(ctx, schemaId.Name, Q.Empty));
        }

        [Fact]
        public async Task Should_query_contents_by_query_for_frontend_without_scripting()
        {
            const int count = 5, total = 200;

            var content = CreateContent(contentId);

            SetupUser(isFrontend: true);
            SetupSchemaFound();
            SetupSchemaScripting(contentId);
            SetupContents(null, count, total, content, SearchScope.All);

            var ctx = requestContext;

            var result = await sut.QueryAsync(ctx, schemaId.Name, Q.Empty);

            Assert.Equal(contentData, result[0].Data);
            Assert.Equal(content.Id, result[0].Id);

            Assert.Equal(total, result.Total);

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Theory]
        [MemberData(nameof(ApiStatusTests))]
        public async Task Should_query_contents_by_query_for_api_with_scripting(int unpublished, Status[] status, SearchScope scope)
        {
            const int count = 5, total = 200;

            var content = CreateContent(contentId);

            SetupUser(isFrontend: false);
            SetupSchemaFound();
            SetupSchemaScripting(contentId);
            SetupContents(status, count, total, content, scope);

            var ctx = requestContext.WithUnpublished(unpublished == 1);

            var result = await sut.QueryAsync(ctx, schemaId.Name, Q.Empty);

            Assert.Equal(contentData, result[0].Data);
            Assert.Equal(contentId, result[0].Id);

            Assert.Equal(total, result.Total);

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .MustHaveHappened(count, Times.Exactly);
        }

        [Fact]
        public async Task Should_query_contents_by_id_for_frontend_and_without_scripting()
        {
            const int count = 5, total = 200;

            var ids = Enumerable.Range(0, count).Select(x => Guid.NewGuid()).ToList();

            SetupUser(isFrontend: true);
            SetupSchemaFound();
            SetupSchemaScripting(ids.ToArray());
            SetupContents(null, total, ids, SearchScope.All);

            var ctx = requestContext;

            var result = await sut.QueryAsync(ctx, schemaId.Name, Q.Empty.WithIds(ids));

            Assert.Equal(ids, result.Select(x => x.Id).ToList());
            Assert.Equal(total, result.Total);

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Theory]
        [MemberData(nameof(ApiStatusTests))]
        public async Task Should_query_contents_by_id_for_api_and_with_scripting(int unpublished, Status[] status, SearchScope scope)
        {
            const int count = 5, total = 200;

            var ids = Enumerable.Range(0, count).Select(x => Guid.NewGuid()).ToList();

            SetupUser(isFrontend: false);
            SetupSchemaFound();
            SetupSchemaScripting(ids.ToArray());
            SetupContents(status, total, ids, scope);

            var ctx = requestContext.WithUnpublished(unpublished == 1);

            var result = await sut.QueryAsync(ctx, schemaId.Name, Q.Empty.WithIds(ids));

            Assert.Equal(ids, result.Select(x => x.Id).ToList());
            Assert.Equal(total, result.Total);

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .MustHaveHappened(count, Times.Exactly);
        }

        [Fact]
        public async Task Should_query_all_contents_by_id_for_frontend_and_without_scripting()
        {
            const int count = 5;

            var ids = Enumerable.Range(0, count).Select(x => Guid.NewGuid()).ToList();

            SetupUser(isFrontend: true);
            SetupSchemaFound();
            SetupSchemaScripting(ids.ToArray());
            SetupContents(null, ids, SearchScope.All);

            var ctx = requestContext;

            var result = await sut.QueryAsync(ctx, ids);

            Assert.Equal(ids, result.Select(x => x.Id).ToList());

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Theory]
        [MemberData(nameof(ApiStatusTests))]
        public async Task Should_query_all_contents_by_id_for_api_and_with_scripting(int unpublished, Status[] status, SearchScope scope)
        {
            const int count = 5;

            var ids = Enumerable.Range(0, count).Select(x => Guid.NewGuid()).ToList();

            SetupUser(isFrontend: false);
            SetupSchemaFound();
            SetupSchemaScripting(ids.ToArray());
            SetupContents(status, ids, scope);

            var ctx = requestContext.WithUnpublished(unpublished == 1);

            var result = await sut.QueryAsync(ctx, ids);

            Assert.Equal(ids, result.Select(x => x.Id).ToList());

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .MustHaveHappened(count, Times.Exactly);
        }

        [Fact]
        public async Task Should_skip_contents_when_user_has_no_permission()
        {
            var ids = Enumerable.Range(0, 1).Select(x => Guid.NewGuid()).ToList();

            SetupUser(isFrontend: false, allowSchema: false);
            SetupSchemaFound();
            SetupSchemaScripting(ids.ToArray());
            SetupContents(new Status[0], ids, SearchScope.All);

            var ctx = requestContext;

            var result = await sut.QueryAsync(ctx, ids);

            Assert.Empty(result);
        }

        [Fact]
        public async Task Should_not_call_repository_if_no_id_defined()
        {
            var ids = new List<Guid>();

            SetupUser(isFrontend: false, allowSchema: false);
            SetupSchemaFound();

            var ctx = requestContext;

            var result = await sut.QueryAsync(ctx, ids);

            Assert.Empty(result);

            A.CallTo(() => contentRepository.QueryAsync(app, A<Status[]>.Ignored, A<HashSet<Guid>>.Ignored, A<SearchScope>.Ignored))
                .MustNotHaveHappened();
        }

        private void SetupUser(bool isFrontend, bool allowSchema = true)
        {
            if (isFrontend)
            {
                identity.AddClaim(new Claim(OpenIdClaims.ClientId, DefaultClients.Frontend));
            }

            if (allowSchema)
            {
                identity.AddClaim(new Claim(SquidexClaimTypes.Permissions, Permissions.ForApp(Permissions.AppContentsRead, app.Name, schema.SchemaDef.Name).Id));
            }

            requestContext.UpdatePermissions();
        }

        private void SetupSchemaScripting(params Guid[] ids)
        {
            foreach (var id in ids)
            {
                A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.That.Matches(x => x.User == user && x.ContentId == id && x.Data == contentData), "<query-script>"))
                    .Returns(contentTransformed);
            }
        }

        private void SetupContents(Status[]? status, int count, int total, IContentEntity content, SearchScope scope)
        {
            A.CallTo(() => contentRepository.QueryAsync(app, schema, A<Status[]>.That.Is(status), A<ClrQuery>.Ignored, scope))
                .Returns(ResultList.Create(total, Enumerable.Repeat(content, count)));
        }

        private void SetupContents(Status[]? status, int total, List<Guid> ids, SearchScope scope)
        {
            A.CallTo(() => contentRepository.QueryAsync(app, schema, A<Status[]>.That.Is(status), A<HashSet<Guid>>.Ignored, scope))
                .Returns(ResultList.Create(total, ids.Select(CreateContent).Shuffle()));
        }

        private void SetupContents(Status[]? status, List<Guid> ids, SearchScope scope)
        {
            A.CallTo(() => contentRepository.QueryAsync(app, A<Status[]>.That.Is(status), A<HashSet<Guid>>.Ignored, scope))
                .Returns(ids.Select(x => (CreateContent(x), schema)).ToList());
        }

        private void SetupContent(Status[]? status, IContentEntity? content, SearchScope scope)
        {
            A.CallTo(() => contentRepository.FindContentAsync(app, schema, A<Status[]>.That.Is(status), contentId, scope))
                .Returns(content);
        }

        private void SetupSchemaFound()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Name))
                .Returns(schema);

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Id, false))
                .Returns(schema);
        }

        private void SetupSchemaNotFound()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Name))
                .Returns((ISchemaEntity?)null);

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Id, false))
                .Returns((ISchemaEntity?)null);
        }

        private void SetupEnricher()
        {
            A.CallTo(() => contentEnricher.EnrichAsync(A<IEnumerable<IContentEntity>>.Ignored, requestContext))
                .ReturnsLazily(x =>
                {
                    var input = x.GetArgument<IEnumerable<IContentEntity>>(0)!;

                    return Task.FromResult<IReadOnlyList<IEnrichedContentEntity>>(input.Select(c => SimpleMapper.Map(c, new ContentEntity())).ToList());
                });
        }

        private IContentEntity CreateContent(Guid id)
        {
            return CreateContent(id, Status.Published);
        }

        private IContentEntity CreateContent(Guid id, Status status)
        {
            var content = new ContentEntity
            {
                Id = id,
                Data = contentData,
                SchemaId = schemaId,
                Status = status
            };

            return content;
        }
    }
}