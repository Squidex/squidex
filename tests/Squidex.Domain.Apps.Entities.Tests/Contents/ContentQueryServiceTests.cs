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
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.Edm;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;
using Xunit;

#pragma warning disable SA1401 // Fields must be private

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class ContentQueryServiceTests
    {
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
        private readonly EdmModelBuilder modelBuilder = A.Fake<EdmModelBuilder>();
        private readonly QueryContext context;
        private readonly ContentQueryService sut;

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

            context = QueryContext.Create(app, user);

            sut = new ContentQueryService(
                appProvider,
                urlGenerator,
                contentRepository,
                contentVersionLoader,
                scriptEngine,
                Options.Create(new ContentOptions()), modelBuilder);
        }

        [Fact]
        public async Task Should_return_schema_from_id_if_string_is_guid()
        {
            SetupSchema();

            var result = await sut.GetSchemaAsync(context, schemaId.Name);

            Assert.Equal(schema, result);
        }

        [Fact]
        public async Task Should_return_schema_from_name_if_string_not_guid()
        {
            SetupSchema();

            var result = await sut.GetSchemaAsync(context, schemaId.Name);

            Assert.Equal(schema, result);
        }

        [Fact]
        public async Task Should_throw_404_if_schema_not_found()
        {
            SetupSchemaNotFound();

            var ctx = context;

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.GetSchemaAsync(ctx, schemaId.Name));
        }

        [Fact]
        public async Task Should_throw_404_if_schema_not_found_in_check()
        {
            SetupSchemaNotFound();

            var ctx = context;

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.ThrowIfSchemaNotExistsAsync(ctx, schemaId.Name));
        }

        public static IEnumerable<object[]> SingleDataFrontend = new[]
        {
            new object[] { true,  new[] { Status.Archived, Status.Draft, Status.Published } },
            new object[] { false, new[] { Status.Archived, Status.Draft, Status.Published } }
        };

        public static IEnumerable<object[]> SingleDataApi = new[]
        {
            new object[] { true,  new[] { Status.Draft, Status.Published } },
            new object[] { false, new[] { Status.Published } }
        };

        [Fact]
        public async Task Should_throw_for_single_content_if_no_permission()
        {
            SetupClaims(false, false);
            SetupSchema();

            var ctx = context;

            await Assert.ThrowsAsync<DomainForbiddenException>(() => sut.FindContentAsync(ctx, schemaId.Name, contentId));
        }

        [Fact]
        public async Task Should_throw_404_for_single_content_if_content_not_found()
        {
            SetupClaims();
            SetupSchema();

            A.CallTo(() => contentRepository.FindContentAsync(app, schema, new[] { Status.Published }, contentId))
                .Returns((IContentEntity)null);

            var ctx = context;

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(async () => await sut.FindContentAsync(ctx, schemaId.Name, contentId));
        }

        [Theory]
        [MemberData(nameof(SingleDataFrontend))]
        public async Task Should_return_single_content_for_frontend_without_transform(bool unpublished, params Status[] status)
        {
            var content = CreateContent(contentId);

            SetupClaims(true);
            SetupSchema();
            SetupScripting(contentId);

            A.CallTo(() => contentRepository.FindContentAsync(app, schema, A<Status[]>.That.IsSameSequenceAs(status), contentId))
                .Returns(content);

            var ctx = context.WithUnpublished(unpublished);

            var result = await sut.FindContentAsync(ctx, schemaId.Name, contentId);

            Assert.Equal(contentTransformed, result.Data);
            Assert.Equal(content.Id, result.Id);

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Theory]
        [MemberData(nameof(SingleDataApi))]
        public async Task Should_return_single_content_for_api_with_transform(bool unpublished, params Status[] status)
        {
            var content = CreateContent(contentId);

            SetupClaims();
            SetupSchema();
            SetupScripting(contentId);

            A.CallTo(() => contentRepository.FindContentAsync(app, schema, A<Status[]>.That.IsSameSequenceAs(status), contentId))
                .Returns(content);

            var ctx = context.WithUnpublished(unpublished);

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
            SetupSchema();
            SetupScripting(contentId);

            A.CallTo(() => contentVersionLoader.LoadAsync(contentId, 10))
                .Returns(content);

            var ctx = context;

            var result = await sut.FindContentAsync(ctx, schemaId.Name, contentId, 10);

            Assert.Equal(contentTransformed, result.Data);
            Assert.Equal(content.Id, result.Id);
        }

        public static IEnumerable<object[]> ManyDataFrontend = new[]
        {
            new object[] { true,  true,  new[] { Status.Archived } },
            new object[] { true,  false, new[] { Status.Archived } },
            new object[] { false, true,  new[] { Status.Draft, Status.Published } },
            new object[] { false, false, new[] { Status.Draft, Status.Published } }
        };

        public static IEnumerable<object[]> ManyDataApi = new[]
        {
            new object[] { true,  true,  new[] { Status.Draft, Status.Published } },
            new object[] { false, true,  new[] { Status.Draft, Status.Published } },
            new object[] { false, false, new[] { Status.Published } },
            new object[] { true,  false, new[] { Status.Published } }
        };

        [Fact]
        public async Task Should_throw_for_query_if_no_permission()
        {
            SetupClaims(false, false);
            SetupSchema();

            var ctx = context;

            await Assert.ThrowsAsync<DomainForbiddenException>(() => sut.QueryAsync(ctx, schemaId.Name, Q.Empty));
        }

        [Theory]
        [MemberData(nameof(ManyDataFrontend))]
        public async Task Should_query_contents_by_query_for_frontend_without_transform(bool archive, bool unpublished, params Status[] status)
        {
            const int count = 5, total = 200;

            var content = CreateContent(contentId);

            SetupClaims(true);
            SetupSchema();
            SetupScripting(contentId);
            SetupContents(status, count, total, content);

            var ctx = context.WithArchived(archive).WithUnpublished(unpublished);

            var result = await sut.QueryAsync(ctx, schemaId.Name, Q.Empty);

            Assert.Equal(contentData, result[0].Data);
            Assert.Equal(content.Id, result[0].Id);

            Assert.Equal(total, result.Total);

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Theory]
        [MemberData(nameof(ManyDataApi))]
        public async Task Should_query_contents_by_query_for_api_and_transform(bool archive, bool unpublished, params Status[] status)
        {
            const int count = 5, total = 200;

            var content = CreateContent(contentId);

            SetupClaims();
            SetupSchema();
            SetupScripting(contentId);
            SetupContents(status, count, total, content);

            var ctx = context.WithArchived(archive).WithUnpublished(unpublished);

            var result = await sut.QueryAsync(ctx, schemaId.Name, Q.Empty);

            Assert.Equal(contentData, result[0].Data);
            Assert.Equal(contentId, result[0].Id);

            Assert.Equal(total, result.Total);

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .MustHaveHappened(count, Times.Exactly);
        }

        [Fact]
        public Task Should_throw_if_query_is_invalid()
        {
            SetupClaims();
            SetupSchema();

            A.CallTo(() => modelBuilder.BuildEdmModel(schema, app))
                .Throws(new ODataException());

            return Assert.ThrowsAsync<ValidationException>(() => sut.QueryAsync(context, schemaId.Name, Q.Empty.WithODataQuery("query")));
        }

        public static IEnumerable<object[]> ManyIdDataFrontend = new[]
        {
            new object[] { true,  true,  new[] { Status.Archived } },
            new object[] { true,  false, new[] { Status.Archived } },
            new object[] { false, true,  new[] { Status.Draft, Status.Published } },
            new object[] { false, false, new[] { Status.Draft, Status.Published } }
        };

        public static IEnumerable<object[]> ManyIdDataApi = new[]
        {
            new object[] { true,  true,  new[] { Status.Draft, Status.Published } },
            new object[] { false, true,  new[] { Status.Draft, Status.Published } },
            new object[] { false, false, new[] { Status.Published } },
            new object[] { true,  false, new[] { Status.Published } }
        };

        [Theory]
        [MemberData(nameof(ManyIdDataFrontend))]
        public async Task Should_query_contents_by_id_for_frontend_and_transform(bool archive, bool unpublished, params Status[] status)
        {
            const int count = 5, total = 200;

            var ids = Enumerable.Range(0, count).Select(x => Guid.NewGuid()).ToList();

            SetupClaims(true);
            SetupSchema();
            SetupScripting(ids.ToArray());
            SetupContents(status, total, ids);

            var ctx = context.WithArchived(archive).WithUnpublished(unpublished);

            var result = await sut.QueryAsync(ctx, schemaId.Name, Q.Empty.WithIds(ids));

            Assert.Equal(ids, result.Select(x => x.Id).ToList());
            Assert.Equal(total, result.Total);

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Theory]
        [MemberData(nameof(ManyIdDataApi))]
        public async Task Should_query_contents_by_id_from_repository_and_transform(bool archive, bool unpublished, params Status[] status)
        {
            const int count = 5, total = 200;

            var ids = Enumerable.Range(0, count).Select(x => Guid.NewGuid()).ToList();

            SetupClaims();
            SetupSchema();
            SetupScripting(ids.ToArray());
            SetupContents(status, total, ids);

            var ctx = context.WithArchived(archive).WithUnpublished(unpublished);

            var result = await sut.QueryAsync(ctx, schemaId.Name, Q.Empty.WithIds(ids));

            Assert.Equal(ids, result.Select(x => x.Id).ToList());
            Assert.Equal(total, result.Total);

            A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .MustHaveHappened(count, Times.Exactly);
        }

        private void SetupClaims(bool isFrontend = false, bool allowSchema = true)
        {
            if (isFrontend)
            {
                identity.AddClaim(new Claim(OpenIdClaims.ClientId, "squidex-frontend"));
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

        private void SetupContents(Status[] status, int count, int total, IContentEntity content)
        {
            A.CallTo(() => contentRepository.QueryAsync(app, schema, A<Status[]>.That.IsSameSequenceAs(status), A<Query>.Ignored))
                .Returns(ResultList.Create(total, Enumerable.Repeat(content, count)));
        }

        private void SetupContents(Status[] status, int total, List<Guid> ids)
        {
            A.CallTo(() => contentRepository.QueryAsync(app, schema, A<Status[]>.That.IsSameSequenceAs(status), A<HashSet<Guid>>.Ignored))
                .Returns(ResultList.Create(total, ids.Select(x => CreateContent(x)).Shuffle()));
        }

        private void SetupSchema()
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

        private IContentEntity CreateContent(Guid id, Status status = Status.Published)
        {
            var content = A.Fake<IContentEntity>();

            A.CallTo(() => content.Id).Returns(id);
            A.CallTo(() => content.Data).Returns(contentData);
            A.CallTo(() => content.DataDraft).Returns(contentData);
            A.CallTo(() => content.Status).Returns(status);
            A.CallTo(() => content.SchemaId).Returns(schemaId);

            return content;
        }
    }
}