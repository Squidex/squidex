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
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.Edm;
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

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class ContentQueryServiceTests
    {
        private readonly IContentEnricher contentEnricher = A.Fake<IContentEnricher>();
        private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
        private readonly IContentVersionLoader contentVersionLoader = A.Fake<IContentVersionLoader>();
        private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
        private readonly ISchemaEntity schema = A.Fake<ISchemaEntity>();
        private readonly IAppEntity app = A.Fake<IAppEntity>();
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly IAssetUrlGenerator urlGenerator = A.Fake<IAssetUrlGenerator>();
        private readonly Guid contentId = Guid.NewGuid();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly NamedContentData contentData = new NamedContentData();
        private readonly NamedContentData contentTransformed = new NamedContentData();
        private readonly ClaimsPrincipal user;
        private readonly ClaimsIdentity identity = new ClaimsIdentity();
        private readonly EdmModelBuilder modelBuilder = new EdmModelBuilder(new MemoryCache(Options.Create(new MemoryCacheOptions())));
        private readonly Context context;
        private readonly ContentQueryService sut;

        public static IEnumerable<object[]> ApiStatusTests = new[]
        {
            new object[] { 0, new[] { Status.Published } },
            new object[] { 1, null }
        };

        public ContentQueryServiceTests()
        {
            user = new ClaimsPrincipal(identity);

            A.CallTo(() => app.Id).Returns(appId.Id);
            A.CallTo(() => app.Name).Returns(appId.Name);
            A.CallTo(() => app.LanguagesConfig).Returns(LanguagesConfig.English);

            var schemaDef =
                new Schema(schemaId.Name)
                    .ConfigureScripts(new SchemaScripts { Query = "<query-script>" });

            A.CallTo(() => schema.Id).Returns(schemaId.Id);
            A.CallTo(() => schema.AppId).Returns(appId);
            A.CallTo(() => schema.SchemaDef).Returns(schemaDef);

            SetupEnricher();

            context = new Context(user, app);

            var options = Options.Create(new ContentOptions { DefaultPageSize = 30 });

            sut = new ContentQueryService(
                appProvider,
                urlGenerator,
                contentEnricher,
                contentRepository,
                contentVersionLoader,
                scriptEngine,
                options,
                modelBuilder);
        }

        [Fact]
        public void Should_provide_default_page_size()
        {
            var result = sut.DefaultPageSizeGraphQl;

            Assert.Equal(20, result);
        }

        [Fact]
        public async Task Should_return_schema_from_id_if_string_is_guid()
        {
            SetupSchemaFound();

            var result = await sut.GetSchemaOrThrowAsync(context, schemaId.Name);

            Assert.Equal(schema, result);
        }

        [Fact]
        public async Task Should_return_schema_from_name_if_string_not_guid()
        {
            SetupSchemaFound();

            var result = await sut.GetSchemaOrThrowAsync(context, schemaId.Name);

            Assert.Equal(schema, result);
        }

        [Fact]
        public async Task Should_throw_404_if_schema_not_found()
        {
            SetupSchemaNotFound();

            var ctx = context;

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.GetSchemaOrThrowAsync(ctx, schemaId.Name));
        }

        [Fact]
        public async Task Should_throw_404_if_schema_not_found_in_check()
        {
            SetupSchemaNotFound();

            var ctx = context;

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.GetSchemaOrThrowAsync(ctx, schemaId.Name));
        }

        [Fact]
        public async Task Should_apply_default_page_size()
        {
            SetupClaims(isFrontend: false);
            SetupSchemaFound();

            var query = Q.Empty;

            await sut.QueryAsync(context, schemaId.Name, query);

            A.CallTo(() => contentRepository.QueryAsync(app, schema, A<Status[]>.That.Is(Status.Published), false, A<Query>.That.Is("Take: 30; Sort: lastModified Descending"), false))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_limit_number_of_contents()
        {
            var status = new[] { Status.Published };

            SetupClaims(isFrontend: false);
            SetupSchemaFound();

            var query = Q.Empty.WithODataQuery("$top=300&$skip=20");

            await sut.QueryAsync(context, schemaId.Name, query);

            A.CallTo(() => contentRepository.QueryAsync(app, schema, A<Status[]>.That.Is(status), false, A<Query>.That.Is("Skip: 20; Take: 200; Sort: lastModified Descending"), false))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_throw_for_single_content_if_no_permission()
        {
            SetupClaims(false, false);
            SetupSchemaFound();

            var ctx = context;

            await Assert.ThrowsAsync<DomainForbiddenException>(() => sut.FindContentAsync(ctx, schemaId.Name, contentId));
        }

        [Fact]
        public async Task Should_throw_404_for_single_content_if_content_not_found()
        {
            var status = new[] { Status.Published };

            SetupClaims(isFrontend: false);
            SetupSchemaFound();
            SetupContent(status, null, includeDraft: false);

            var ctx = context;

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(async () => await sut.FindContentAsync(ctx, schemaId.Name, contentId));
        }

        [Fact]
        public async Task Should_return_single_content_for_frontend_without_transform()
        {
            var content = CreateContent(contentId);

            SetupClaims(isFrontend: true);
            SetupSchemaFound();
            SetupScripting(contentId);
            SetupContent(null, content, includeDraft: true);

            var ctx = context;

            var result = await sut.FindContentAsync(ctx, schemaId.Name, contentId);

            Assert.Equal(contentTransformed, result.Data);
            Assert.Equal(content.Id, result.Id);

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Theory]
        [MemberData(nameof(ApiStatusTests))]
        public async Task Should_return_single_content_for_api_with_transform(int unpublished, Status[] status)
        {
            var content = CreateContent(contentId);

            SetupClaims(isFrontend: false);
            SetupSchemaFound();
            SetupScripting(contentId);
            SetupContent(status, content, unpublished == 1);

            var ctx = context.WithUnpublished(unpublished == 1);

            var result = await sut.FindContentAsync(ctx, schemaId.Name, contentId);

            Assert.Equal(contentTransformed, result.Data);
            Assert.Equal(content.Id, result.Id);

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public async Task Should_return_versioned_content_from_repository_and_transform()
        {
            var content = CreateContent(contentId);

            SetupClaims(true);
            SetupSchemaFound();
            SetupScripting(contentId);

            A.CallTo(() => contentVersionLoader.LoadAsync(contentId, 10))
                .Returns(content);

            var ctx = context;

            var result = await sut.FindContentAsync(ctx, schemaId.Name, contentId, 10);

            Assert.Equal(contentTransformed, result.Data);
            Assert.Equal(content.Id, result.Id);
        }

        [Fact]
        public async Task Should_throw_for_query_if_no_permission()
        {
            SetupClaims(false, false);
            SetupSchemaFound();

            var ctx = context;

            await Assert.ThrowsAsync<DomainForbiddenException>(() => sut.QueryAsync(ctx, schemaId.Name, Q.Empty));
        }

        [Fact]
        public async Task Should_query_contents_by_query_for_frontend_without_transform()
        {
            const int count = 5, total = 200;

            var content = CreateContent(contentId);

            SetupClaims(isFrontend: true);
            SetupSchemaFound();
            SetupScripting(contentId);
            SetupContents(null, count, total, content, inDraft: true, includeDraft: true);

            var ctx = context;

            var result = await sut.QueryAsync(ctx, schemaId.Name, Q.Empty);

            Assert.Equal(contentData, result[0].Data);
            Assert.Equal(content.Id, result[0].Id);

            Assert.Equal(total, result.Total);

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Theory]
        [MemberData(nameof(ApiStatusTests))]
        public async Task Should_query_contents_by_query_for_api_and_transform(int unpublished, Status[] status)
        {
            const int count = 5, total = 200;

            var content = CreateContent(contentId);

            SetupClaims(isFrontend: false);
            SetupSchemaFound();
            SetupScripting(contentId);
            SetupContents(status, count, total, content, inDraft: false, unpublished == 1);

            var ctx = context.WithUnpublished(unpublished == 1);

            var result = await sut.QueryAsync(ctx, schemaId.Name, Q.Empty);

            Assert.Equal(contentData, result[0].Data);
            Assert.Equal(contentId, result[0].Id);

            Assert.Equal(total, result.Total);

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .MustHaveHappened(count, Times.Exactly);
        }

        [Fact]
        public async Task Should_throw_if_query_is_invalid()
        {
            SetupClaims(isFrontend: false);
            SetupSchemaFound();

            var query = Q.Empty.WithODataQuery("$filter=invalid");

            await Assert.ThrowsAsync<ValidationException>(() => sut.QueryAsync(context, schemaId.Name, query));
        }

        [Fact]
        public async Task Should_query_contents_by_id_for_frontend_and_transform()
        {
            const int count = 5, total = 200;

            var ids = Enumerable.Range(0, count).Select(x => Guid.NewGuid()).ToList();

            SetupClaims(isFrontend: true);
            SetupSchemaFound();
            SetupScripting(ids.ToArray());
            SetupContents(null, total, ids, includeDraft: true);

            var ctx = context;

            var result = await sut.QueryAsync(ctx, schemaId.Name, Q.Empty.WithIds(ids));

            Assert.Equal(ids, result.Select(x => x.Id).ToList());
            Assert.Equal(total, result.Total);

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Theory]
        [MemberData(nameof(ApiStatusTests))]
        public async Task Should_query_contents_by_id_for_api_and_transform(int unpublished, Status[] status)
        {
            const int count = 5, total = 200;

            var ids = Enumerable.Range(0, count).Select(x => Guid.NewGuid()).ToList();

            SetupClaims(isFrontend: false);
            SetupSchemaFound();
            SetupScripting(ids.ToArray());
            SetupContents(status, total, ids, unpublished == 1);

            var ctx = context.WithUnpublished(unpublished == 1);

            var result = await sut.QueryAsync(ctx, schemaId.Name, Q.Empty.WithIds(ids));

            Assert.Equal(ids, result.Select(x => x.Id).ToList());
            Assert.Equal(total, result.Total);

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .MustHaveHappened(count, Times.Exactly);
        }

        [Fact]
        public async Task Should_query_all_contents_by_id_for_frontend_and_transform()
        {
            const int count = 5;

            var ids = Enumerable.Range(0, count).Select(x => Guid.NewGuid()).ToList();

            SetupClaims(isFrontend: true);
            SetupSchemaFound();
            SetupScripting(ids.ToArray());
            SetupContents(null, ids, includeDraft: true);

            var ctx = context;

            var result = await sut.QueryAsync(ctx, ids);

            Assert.Equal(ids, result.Select(x => x.Id).ToList());

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Theory]
        [MemberData(nameof(ApiStatusTests))]
        public async Task Should_query_all_contents_by_id_for_api_and_transform(int unpublished, Status[] status)
        {
            const int count = 5;

            var ids = Enumerable.Range(0, count).Select(x => Guid.NewGuid()).ToList();

            SetupClaims(isFrontend: false);
            SetupSchemaFound();
            SetupScripting(ids.ToArray());
            SetupContents(status, ids, unpublished == 1);

            var ctx = context.WithUnpublished(unpublished == 1);

            var result = await sut.QueryAsync(ctx, ids);

            Assert.Equal(ids, result.Select(x => x.Id).ToList());

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .MustHaveHappened(count, Times.Exactly);
        }

        [Fact]
        public async Task Should_skip_contents_when_user_has_no_permission()
        {
            var ids = Enumerable.Range(0, 1).Select(x => Guid.NewGuid()).ToList();

            SetupClaims(isFrontend: false, allowSchema: false);
            SetupSchemaFound();
            SetupScripting(ids.ToArray());
            SetupContents(new Status[0], ids, includeDraft: false);

            var ctx = context;

            var result = await sut.QueryAsync(ctx, ids);

            Assert.Empty(result);
        }

        [Fact]
        public async Task Should_not_call_repository_if_no_id_defined()
        {
            var ids = new List<Guid>();

            SetupClaims(isFrontend: false, allowSchema: false);
            SetupSchemaFound();

            var ctx = context;

            var result = await sut.QueryAsync(ctx, ids);

            Assert.Empty(result);

            A.CallTo(() => contentRepository.QueryAsync(app, A<Status[]>.Ignored, A<HashSet<Guid>>.Ignored, A<bool>.Ignored))
                .MustNotHaveHappened();
        }

        private void SetupClaims(bool isFrontend, bool allowSchema = true)
        {
            if (isFrontend)
            {
                identity.AddClaim(new Claim(OpenIdClaims.ClientId, DefaultClients.Frontend));
            }

            if (allowSchema)
            {
                identity.AddClaim(new Claim(SquidexClaimTypes.Permissions, Permissions.ForApp(Permissions.AppContentsRead, app.Name, schema.SchemaDef.Name).Id));
            }
        }

        private void SetupScripting(params Guid[] ids)
        {
            foreach (var id in ids)
            {
                A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.That.Matches(x => x.User == user && x.ContentId == id && x.Data == contentData), "<query-script>"))
                    .Returns(contentTransformed);
            }
        }

        private void SetupContents(Status[] status, int count, int total, IContentEntity content, bool inDraft, bool includeDraft)
        {
            A.CallTo(() => contentRepository.QueryAsync(app, schema, A<Status[]>.That.Is(status), inDraft, A<Query>.Ignored, includeDraft))
                .Returns(ResultList.Create(total, Enumerable.Repeat(content, count)));
        }

        private void SetupContents(Status[] status, int total, List<Guid> ids, bool includeDraft)
        {
            A.CallTo(() => contentRepository.QueryAsync(app, schema, A<Status[]>.That.Is(status), A<HashSet<Guid>>.Ignored, includeDraft))
                .Returns(ResultList.Create(total, ids.Select(x => CreateContent(x)).Shuffle()));
        }

        private void SetupContents(Status[] status, List<Guid> ids, bool includeDraft)
        {
            A.CallTo(() => contentRepository.QueryAsync(app, A<Status[]>.That.Is(status), A<HashSet<Guid>>.Ignored, includeDraft))
                .Returns(ids.Select(x => (CreateContent(x), schema)).ToList());
        }

        private void SetupContent(Status[] status, IContentEntity content, bool includeDraft)
        {
            A.CallTo(() => contentRepository.FindContentAsync(app, schema, A<Status[]>.That.Is(status), contentId, includeDraft))
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
                .Returns((ISchemaEntity)null);

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Id, false))
                .Returns((ISchemaEntity)null);
        }

        private void SetupEnricher()
        {
            A.CallTo(() => contentEnricher.EnrichAsync(A<IEnumerable<IContentEntity>>.Ignored, user))
                .ReturnsLazily(x =>
                {
                    var input = (IEnumerable<IContentEntity>)x.Arguments[0];

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
                DataDraft = contentData,
                SchemaId = schemaId,
                Status = status,
            };

            return content;
        }
    }
}