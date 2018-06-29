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
using Microsoft.OData;
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
        private readonly Guid appId = Guid.NewGuid();
        private readonly Guid schemaId = Guid.NewGuid();
        private readonly string appName = "my-app";
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

            A.CallTo(() => app.Id).Returns(appId);
            A.CallTo(() => app.Name).Returns(appName);
            A.CallTo(() => app.LanguagesConfig).Returns(LanguagesConfig.English);

            A.CallTo(() => schema.SchemaDef).Returns(new Schema("my-schema"));

            context = QueryContext.Create(app, user);

            sut = new ContentQueryService(contentRepository, contentVersionLoader, appProvider, scriptEngine, modelBuilder);
        }

        [Fact]
        public async Task Should_return_schema_from_id_if_string_is_guid()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(appId, schemaId, false))
                .Returns(schema);

            var result = await sut.GetSchemaAsync(context.WithSchemaId(schemaId));

            Assert.Equal(schema, result);
        }

        [Fact]
        public async Task Should_return_schema_from_name_if_string_not_guid()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(appId, "my-schema"))
                .Returns(schema);

            var result = await sut.GetSchemaAsync(context.WithSchemaName("my-schema"));

            Assert.Equal(schema, result);
        }

        [Fact]
        public async Task Should_throw_if_schema_not_found()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(appId, "my-schema"))
                .Returns((ISchemaEntity)null);

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.GetSchemaAsync(context.WithSchemaName("my-schema")));
        }

        [Fact]
        public async Task Should_throw_if_schema_not_found_in_check()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(appId, "my-schema"))
                .Returns((ISchemaEntity)null);

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.ThrowIfSchemaNotExistsAsync(context.WithSchemaName("my-schema")));
        }

        public static IEnumerable<object[]> SingleRequestData = new[]
        {
            new object[] { true,  new[] { Status.Archived, Status.Draft, Status.Published } },
            new object[] { false, new[] { Status.Published } }
        };

        [Theory]
        [MemberData(nameof(SingleRequestData))]
        public async Task Should_return_content_from_repository_and_transform(bool isFrontend, params Status[] status)
        {
            var contentId = Guid.NewGuid();
            var content = CreateContent(contentId);

            SetupClaims(isFrontend);
            SetupScripting(contentId);

            A.CallTo(() => appProvider.GetSchemaAsync(appId, schemaId, false))
                .Returns(schema);
            A.CallTo(() => contentRepository.FindContentAsync(app, schema, A<Status[]>.That.IsSameSequenceAs(status), contentId))
                .Returns(content);

            var result = await sut.FindContentAsync(context.WithSchemaId(schemaId), contentId);

            Assert.Equal(contentTransformed, result.Data);
            Assert.Equal(content.Id, result.Id);
        }

        [Fact]
        public async Task Should_return_versioned_content_from_repository_and_transform()
        {
            var contentId = Guid.NewGuid();
            var content = CreateContent(contentId);

            SetupScripting(contentId);

            A.CallTo(() => appProvider.GetSchemaAsync(appId, schemaId, false))
                .Returns(schema);
            A.CallTo(() => contentVersionLoader.LoadAsync(contentId, 10))
                .Returns(content);

            var result = await sut.FindContentAsync(context.WithSchemaId(schemaId), contentId, 10);

            Assert.Equal(contentTransformed, result.Data);
            Assert.Equal(content.Id, result.Id);
        }

        [Fact]
        public async Task Should_throw_if_content_to_find_does_not_exist()
        {
            var contentId = Guid.NewGuid();

            A.CallTo(() => appProvider.GetSchemaAsync(appId, schemaId, false))
                .Returns(schema);

            A.CallTo(() => contentRepository.FindContentAsync(app, schema, new[] { Status.Published }, contentId))
                .Returns((IContentEntity)null);

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(async () => await sut.FindContentAsync(context.WithSchemaId(schemaId), contentId));
        }

        public static IEnumerable<object[]> ManyRequestData = new[]
        {
            new object[] { 5, 200, false, true,  new[] { Status.Published } },
            new object[] { 5, 200, false, false, new[] { Status.Published } },
            new object[] { 5, 200, true,  false, new[] { Status.Draft, Status.Published } },
            new object[] { 5, 200, true,  true,  new[] { Status.Archived } }
        };

        [Theory]
        [MemberData(nameof(ManyRequestData))]
        public async Task Should_query_contents_by_query_from_repository_and_transform(int count, int total, bool isFrontend, bool archive, params Status[] status)
        {
            var contentId = Guid.NewGuid();
            var content = CreateContent(contentId);

            SetupClaims(isFrontend);
            SetupScripting(contentId);

            A.CallTo(() => appProvider.GetSchemaAsync(appId, schemaId, false))
                .Returns(schema);

            A.CallTo(() => contentRepository.QueryAsync(app, schema, A<Status[]>.That.IsSameSequenceAs(status), A<ODataUriParser>.Ignored))
                .Returns(ResultList.Create(Enumerable.Repeat(content, count), total));

            var result = await sut.QueryAsync(context.WithSchemaId(schemaId).WithArchived(archive), string.Empty);

            Assert.Equal(contentData, result[0].Data);
            Assert.Equal(content.Id, result[0].Id);

            Assert.Equal(total, result.Total);

            if (!isFrontend)
            {
                A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                    .MustHaveHappened(Repeated.Exactly.Times(count));
            }
            else
            {
                A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                    .MustNotHaveHappened();
            }
        }

        [Fact]
        public Task Should_throw_if_query_is_invalid()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(appId, schemaId, false))
                .Returns(schema);

            A.CallTo(() => modelBuilder.BuildEdmModel(schema, app))
                .Throws(new ODataException());

            return Assert.ThrowsAsync<ValidationException>(() => sut.QueryAsync(context.WithSchemaId(schemaId), "query"));
        }

        public static IEnumerable<object[]> ManyIdRequestData = new[]
        {
            new object[] { 5, 200, false, true,  new[] { Status.Published } },
            new object[] { 5, 200, false, false, new[] { Status.Published } },
            new object[] { 5, 200, true,  false, new[] { Status.Draft, Status.Published } },
            new object[] { 5, 200, true,  true,  new[] { Status.Archived } }
        };

        [Theory]
        [MemberData(nameof(ManyIdRequestData))]
        public async Task Should_query_contents_by_id_from_repository_and_transform(int count, int total, bool isFrontend, bool archive, params Status[] status)
        {
            var ids = Enumerable.Range(0, count).Select(x => Guid.NewGuid()).ToList();

            SetupClaims(isFrontend);
            SetupScripting(ids.ToArray());

            A.CallTo(() => appProvider.GetSchemaAsync(appId, schemaId, false))
                .Returns(schema);

            A.CallTo(() => contentRepository.QueryAsync(app, schema, A<Status[]>.That.IsSameSequenceAs(status), A<HashSet<Guid>>.Ignored))
                .Returns(ResultList.Create(ids.Select(x => CreateContent(x)).Shuffle(), total));

            var result = await sut.QueryAsync(context.WithSchemaId(schemaId).WithArchived(archive), ids);

            Assert.Equal(ids, result.Select(x => x.Id).ToList());
            Assert.Equal(total, result.Total);

            if (!isFrontend)
            {
                A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.Ignored, A<string>.Ignored))
                    .MustHaveHappened(Repeated.Exactly.Times(count));
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

        private void SetupScripting(params Guid[] contentId)
        {
            var script = "<script-query>";

            A.CallTo(() => schema.ScriptQuery)
                .Returns(script);

            foreach (var id in contentId)
            {
                A.CallTo(() => scriptEngine.Transform(A<ScriptContext>.That.Matches(x => x.User == user && x.ContentId == id && x.Data == contentData), script))
                    .Returns(contentTransformed);
            }
        }

        private IContentEntity CreateContent(Guid id, Status status = Status.Published)
        {
            var content = A.Fake<IContentEntity>();

            A.CallTo(() => content.Id).Returns(id);
            A.CallTo(() => content.Data).Returns(contentData);
            A.CallTo(() => content.DataDraft).Returns(contentData);
            A.CallTo(() => content.Status).Returns(status);

            return content;
        }
    }
}