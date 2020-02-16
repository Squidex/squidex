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
        private readonly ContentQueryParser queryParser = A.Fake<ContentQueryParser>();
        private readonly ContentQueryService sut;

        public ContentQueryServiceTests()
        {
            var schemaDef =
                new Schema(schemaId.Name)
                    .SetScripts(new SchemaScripts { Query = "<query-script>" });

            schema = Mocks.Schema(appId, schemaId, schemaDef);

            SetupEnricher();

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Name))
                .Returns(schema);

            A.CallTo(() => queryParser.ParseQuery(A<Context>._, schema, A<Q>._))
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
        public async Task GetSchemaOrThrowAsync_should_return_schema_from_guid_string()
        {
            var input = schemaId.Id.ToString();

            var ctx = CreateContext(isFrontend: false, allowSchema: true);

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Id, false))
                .Returns(schema);

            var result = await sut.GetSchemaOrThrowAsync(ctx, input);

            Assert.Equal(schema, result);
        }

        [Fact]
        public async Task GetSchemaOrThrowAsync_should_return_schema_from_name()
        {
            var input = schemaId.Name;

            var ctx = CreateContext(isFrontend: false, allowSchema: true);

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Name))
                .Returns(schema);

            var result = await sut.GetSchemaOrThrowAsync(ctx, input);

            Assert.Equal(schema, result);
        }

        [Fact]
        public async Task GetSchemaOrThrowAsync_should_throw_404_if_not_found()
        {
            var ctx = CreateContext(isFrontend: false, allowSchema: true);

            A.CallTo(() => appProvider.GetSchemaAsync(A<Guid>._, A<string>._))
                .Returns((ISchemaEntity?)null);

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.GetSchemaOrThrowAsync(ctx, schemaId.Name));
        }

        [Fact]
        public async Task FindContentAsync_should_throw_exception_if_user_has_no_permission()
        {
            var ctx = CreateContext(isFrontend: false, allowSchema: false);

            A.CallTo(() => contentRepository.FindContentAsync(ctx.App, schema, contentId, A<SearchScope>._))
                .Returns(CreateContent(contentId));

            await Assert.ThrowsAsync<DomainForbiddenException>(() => sut.FindContentAsync(ctx, schemaId.Name, contentId));
        }

        [Fact]
        public async Task FindContentAsync_should_throw_404_if_not_found()
        {
            var ctx = CreateContext(isFrontend: false, allowSchema: true);

            A.CallTo(() => contentRepository.FindContentAsync(ctx.App, schema, contentId, A<SearchScope>._))
                .Returns<IContentEntity?>(null);

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(async () => await sut.FindContentAsync(ctx, schemaId.Name, contentId));
        }

        [Fact]
        public async Task FindContentAsync_should_return_content_for_frontend_without_transform()
        {
            var ctx = CreateContext(isFrontend: true, allowSchema: true);

            var content = CreateContent(contentId);

            A.CallTo(() => contentRepository.FindContentAsync(ctx.App, schema, contentId, SearchScope.All))
                .Returns(content);

            var result = await sut.FindContentAsync(ctx, schemaId.Name, contentId);

            Assert.Equal(contentTransformed, result!.Data);
            Assert.Equal(content.Id, result.Id);

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>._, A<string>._))
                .MustNotHaveHappened();
        }

        [Theory]
        [InlineData(true,  SearchScope.All)]
        [InlineData(false, SearchScope.Published)]
        public async Task FindContentAsync_should_return_content_for_api_and_transform(bool unpublished, SearchScope scope)
        {
            var ctx =
                CreateContext(isFrontend: false, allowSchema: true)
                    .WithUnpublished(unpublished);

            var content = CreateContent(contentId);

            A.CallTo(() => contentRepository.FindContentAsync(ctx.App, schema, contentId, scope))
                .Returns(content);

            SetupSchemaScripting(contentId);

            var result = await sut.FindContentAsync(ctx, schemaId.Name, contentId);

            Assert.Equal(contentTransformed, result!.Data);
            Assert.Equal(content.Id, result.Id);

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>._, A<string>._))
                .MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public async Task FindContentAsync_should_return_content_by_version_for_api_and_transform()
        {
            var ctx = CreateContext(isFrontend: false, allowSchema: true);

            var content = CreateContent(contentId);

            A.CallTo(() => contentVersionLoader.GetAsync(contentId, 13))
                .Returns(content);

            SetupSchemaScripting(contentId);

            var result = await sut.FindContentAsync(ctx, schemaId.Name, contentId, 13);

            Assert.Equal(contentTransformed, result!.Data);
            Assert.Equal(content.Id, result.Id);

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>._, A<string>._))
                .MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public async Task QueryAsync_should_throw_if_user_has_no_permission()
        {
            var ctx = CreateContext(isFrontend: false, allowSchema: false);

            await Assert.ThrowsAsync<DomainForbiddenException>(() => sut.QueryAsync(ctx, schemaId.Name, Q.Empty));
        }

        [Fact]
        public async Task QueryAsync_should_return_contents_for_frontend_without_transform()
        {
            var ctx = CreateContext(isFrontend: true, allowSchema: true);

            var content = CreateContent(contentId);

            A.CallTo(() => contentRepository.QueryAsync(ctx.App, schema, A<ClrQuery>._, SearchScope.All))
                .Returns(ResultList.CreateFrom(5, content));

            var result = await sut.QueryAsync(ctx, schemaId.Name, Q.Empty);

            Assert.Equal(contentData, result[0].Data);
            Assert.Equal(content.Id, result[0].Id);

            Assert.Equal(5, result.Total);

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>._, A<string>._))
                .MustNotHaveHappened();
        }

        [Theory]
        [InlineData(true, SearchScope.All)]
        [InlineData(false, SearchScope.Published)]
        public async Task QueryAsync_should_return_contents_for_api_and_transform(bool unpublished, SearchScope scope)
        {
            var ctx =
                CreateContext(isFrontend: false, allowSchema: true)
                    .WithUnpublished(unpublished);

            var content = CreateContent(contentId);

            A.CallTo(() => contentRepository.QueryAsync(ctx.App, schema, A<ClrQuery>._, scope))
                .Returns(ResultList.CreateFrom(5, content));

            SetupSchemaScripting(contentId);

            var result = await sut.QueryAsync(ctx, schemaId.Name, Q.Empty);

            Assert.Equal(contentData, result[0].Data);
            Assert.Equal(contentId, result[0].Id);

            Assert.Equal(5, result.Total);

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>._, A<string>._))
                .MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public async Task QueryByIds_should_return_contents_for_frontend_without_transform()
        {
            var ctx = CreateContext(isFrontend: true, allowSchema: true);

            var ids = Enumerable.Range(0, 5).Select(x => Guid.NewGuid()).ToList();

            A.CallTo(() => contentRepository.QueryAsync(ctx.App, A<HashSet<Guid>>._, SearchScope.All))
                .Returns(ids.Select(x => (CreateContent(x), schema)).ToList());

            var result = await sut.QueryAsync(ctx, ids);

            Assert.Equal(ids, result.Select(x => x.Id).ToList());

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>._, A<string>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task QueryByIds_should_not_return_contents_if_user_has_no_permission()
        {
            var ctx = CreateContext(isFrontend: false, allowSchema: false);

            var ids = Enumerable.Range(0, 5).Select(x => Guid.NewGuid()).ToList();

            A.CallTo(() => contentRepository.QueryAsync(ctx.App, A<HashSet<Guid>>._, SearchScope.All))
                .Returns(ids.Select(x => (CreateContent(x), schema)).ToList());

            var result = await sut.QueryAsync(ctx, ids);

            Assert.Empty(result);
        }

        [Theory]
        [InlineData(true, SearchScope.All)]
        [InlineData(false, SearchScope.Published)]
        public async Task QueryByIds_should_return_contents_for_api_with_transform(bool unpublished, SearchScope scope)
        {
            var ctx =
                CreateContext(isFrontend: false, allowSchema: true)
                    .WithUnpublished(unpublished);

            var ids = Enumerable.Range(0, 5).Select(x => Guid.NewGuid()).ToList();

            A.CallTo(() => contentRepository.QueryAsync(ctx.App, A<HashSet<Guid>>._, scope))
                .Returns(ids.Select(x => (CreateContent(x), schema)).ToList());

            SetupSchemaScripting();

            var result = await sut.QueryAsync(ctx, ids);

            Assert.Equal(ids, result.Select(x => x.Id).ToList());

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>._, A<string>._))
                .MustHaveHappened(5, Times.Exactly);
        }

        [Fact]
        public async Task QueryByIds_should_not_call_repository_if_no_id_defined()
        {
            var ctx = CreateContext(isFrontend: false, allowSchema: true);

            var result = await sut.QueryAsync(ctx, new List<Guid>());

            Assert.Empty(result);

            A.CallTo(() => contentRepository.QueryAsync(ctx.App, A<HashSet<Guid>>._, A<SearchScope>._))
                .MustNotHaveHappened();
        }

        private void SetupSchemaScripting(params Guid[] ids)
        {
            foreach (var id in ids)
            {
                A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.That.Matches(x => x.ContentId == id && x.Data == contentData), "<query-script>"))
                    .Returns(contentTransformed);
            }
        }

        private void SetupEnricher()
        {
            A.CallTo(() => contentEnricher.EnrichAsync(A<IEnumerable<IContentEntity>>._, A<Context>._))
                .ReturnsLazily(x =>
                {
                    var input = x.GetArgument<IEnumerable<IContentEntity>>(0)!;

                    return Task.FromResult<IReadOnlyList<IEnrichedContentEntity>>(input.Select(c => SimpleMapper.Map(c, new ContentEntity())).ToList());
                });
        }

        private Context CreateContext(bool isFrontend, bool allowSchema)
        {
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            if (isFrontend)
            {
                claimsIdentity.AddClaim(new Claim(OpenIdClaims.ClientId, DefaultClients.Frontend));
            }

            if (allowSchema)
            {
                var permission = Permissions.ForApp(Permissions.AppContentsRead, appId.Name, schemaId.Name).Id;

                claimsIdentity.AddClaim(new Claim(SquidexClaimTypes.Permissions, permission));
            }

            return new Context(claimsPrincipal, Mocks.App(appId));
        }

        private IContentEntity CreateContent(Guid id)
        {
            var content = new ContentEntity
            {
                Id = id,
                Data = contentData,
                SchemaId = schemaId,
                Status = Status.Published
            };

            return content;
        }
    }
}