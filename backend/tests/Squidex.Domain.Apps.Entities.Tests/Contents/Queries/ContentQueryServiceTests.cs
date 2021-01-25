// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public class ContentQueryServiceTests
    {
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly IContentEnricher contentEnricher = A.Fake<IContentEnricher>();
        private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
        private readonly IContentLoader contentVersionLoader = A.Fake<IContentLoader>();
        private readonly ISchemaEntity schema;
        private readonly DomainId contentId = DomainId.NewGuid();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
        private readonly ContentData contentData = new ContentData();
        private readonly ContentData contentTransformed = new ContentData();
        private readonly ContentQueryParser queryParser = A.Fake<ContentQueryParser>();
        private readonly ContentQueryService sut;

        public ContentQueryServiceTests()
        {
            var schemaDef =
                new Schema(schemaId.Name)
                    .SetScripts(new SchemaScripts { Query = "<query-script>" });

            schema = Mocks.Schema(appId, schemaId, schemaDef);

            SetupEnricher();

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Name, A<bool>._))
                .Returns(schema);

            A.CallTo(() => appProvider.GetSchemasAsync(appId.Id))
                .Returns(new List<ISchemaEntity> { schema });

            A.CallTo(() => queryParser.ParseAsync(A<Context>._, A<Q>._, A<ISchemaEntity?>._))
                .ReturnsLazily(c => Task.FromResult(c.GetArgument<Q>(1)!));

            sut = new ContentQueryService(
                appProvider,
                contentEnricher,
                contentRepository,
                contentVersionLoader,
                queryParser);
        }

        [Fact]
        public async Task GetSchemaOrThrowAsync_should_return_schema_from_guid_string()
        {
            var input = schemaId.Id.ToString();

            var ctx = CreateContext(isFrontend: false, allowSchema: true);

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Id, false, true))
                .Returns(schema);

            var result = await sut.GetSchemaOrThrowAsync(ctx, input);

            Assert.Equal(schema, result);
        }

        [Fact]
        public async Task GetSchemaOrThrowAsync_should_return_schema_from_name()
        {
            var input = schemaId.Name;

            var ctx = CreateContext(isFrontend: false, allowSchema: true);

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Name, true))
                .Returns(schema);

            var result = await sut.GetSchemaOrThrowAsync(ctx, input);

            Assert.Equal(schema, result);
        }

        [Fact]
        public async Task GetSchemaOrThrowAsync_should_throw_404_if_not_found()
        {
            var ctx = CreateContext(isFrontend: false, allowSchema: true);

            A.CallTo(() => appProvider.GetSchemaAsync(A<DomainId>._, A<string>._, true))
                .Returns((ISchemaEntity?)null);

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.GetSchemaOrThrowAsync(ctx, schemaId.Name));
        }

        [Fact]
        public async Task FindContentAsync_should_throw_exception_if_user_has_no_permission()
        {
            var ctx = CreateContext(isFrontend: false, allowSchema: false);

            A.CallTo(() => contentRepository.FindContentAsync(ctx.App, schema, contentId, A<SearchScope>._))
                .Returns(CreateContent(contentId));

            await Assert.ThrowsAsync<DomainForbiddenException>(() => sut.FindAsync(ctx, schemaId.Name, contentId));
        }

        [Fact]
        public async Task FindContentAsync_should_return_null_if_not_found()
        {
            var ctx = CreateContext(isFrontend: false, allowSchema: true);

            A.CallTo(() => contentRepository.FindContentAsync(ctx.App, schema, contentId, A<SearchScope>._))
                .Returns<IContentEntity?>(null);

            Assert.Null(await sut.FindAsync(ctx, schemaId.Name, contentId));
        }

        [Theory]
        [InlineData(1, 0, SearchScope.All)]
        [InlineData(1, 1, SearchScope.All)]
        [InlineData(0, 1, SearchScope.All)]
        [InlineData(0, 0, SearchScope.Published)]
        public async Task FindContentAsync_should_return_content(int isFrontend, int unpublished, SearchScope scope)
        {
            var ctx =
                CreateContext(isFrontend: isFrontend == 1, allowSchema: true)
                    .WithUnpublished(unpublished == 1);

            var content = CreateContent(contentId);

            A.CallTo(() => contentRepository.FindContentAsync(ctx.App, schema, contentId, scope))
                .Returns(content);

            var result = await sut.FindAsync(ctx, schemaId.Name, contentId);

            Assert.Equal(contentTransformed, result!.Data);
            Assert.Equal(content.Id, result.Id);
        }

        [Fact]
        public async Task FindContentAsync_should_return_content_by_version()
        {
            var ctx = CreateContext(isFrontend: false, allowSchema: true);

            var content = CreateContent(contentId);

            A.CallTo(() => contentVersionLoader.GetAsync(appId.Id, contentId, 13))
                .Returns(content);

            var result = await sut.FindAsync(ctx, schemaId.Name, contentId, 13);

            Assert.Equal(contentTransformed, result!.Data);
            Assert.Equal(content.Id, result.Id);
        }

        [Fact]
        public async Task QueryAsync_should_throw_if_user_has_no_permission()
        {
            var ctx = CreateContext(isFrontend: false, allowSchema: false);

            await Assert.ThrowsAsync<DomainForbiddenException>(() => sut.QueryAsync(ctx, schemaId.Name, Q.Empty));
        }

        [Theory]
        [InlineData(1, 0, SearchScope.All)]
        [InlineData(1, 1, SearchScope.All)]
        [InlineData(0, 1, SearchScope.All)]
        [InlineData(0, 0, SearchScope.Published)]
        public async Task QueryAsync_should_return_contents(int isFrontend, int unpublished, SearchScope scope)
        {
            var ctx =
                CreateContext(isFrontend: isFrontend == 1, allowSchema: true)
                    .WithUnpublished(unpublished == 1);

            var content = CreateContent(contentId);

            var q = Q.Empty.WithReference(DomainId.NewGuid());

            A.CallTo(() => contentRepository.QueryAsync(ctx.App, schema, q, scope))
                .Returns(ResultList.CreateFrom(5, content));

            var result = await sut.QueryAsync(ctx, schemaId.Name, q);

            Assert.Equal(contentData, result[0].Data);
            Assert.Equal(contentId, result[0].Id);

            Assert.Equal(5, result.Total);
        }

        [Fact]
        public async Task QueryAll_should_not_return_contents_if_user_has_no_permission()
        {
            var ctx = CreateContext(isFrontend: false, allowSchema: false);

            var ids = Enumerable.Range(0, 5).Select(x => DomainId.NewGuid()).ToList();

            var q = Q.Empty.WithIds(ids);

            A.CallTo(() => contentRepository.QueryAsync(ctx.App, A<List<ISchemaEntity>>.That.Matches(x => x.Count == 0), q, SearchScope.All))
                .Returns(ResultList.Create(0, ids.Select(CreateContent)));

            var result = await sut.QueryAsync(ctx, q);

            Assert.Empty(result);
        }

        [Theory]
        [InlineData(1, 0, SearchScope.All)]
        [InlineData(1, 1, SearchScope.All)]
        [InlineData(0, 1, SearchScope.All)]
        [InlineData(0, 0, SearchScope.Published)]
        public async Task QueryAll_should_return_contents(int isFrontend, int unpublished, SearchScope scope)
        {
            var ctx =
                CreateContext(isFrontend: isFrontend == 1, allowSchema: true)
                    .WithUnpublished(unpublished == 1);

            var ids = Enumerable.Range(0, 5).Select(x => DomainId.NewGuid()).ToList();

            var q = Q.Empty.WithIds(ids);

            A.CallTo(() => contentRepository.QueryAsync(ctx.App, A<List<ISchemaEntity>>.That.Matches(x => x.Count == 1), q, scope))
                .Returns(ResultList.Create(5, ids.Select(CreateContent)));

            var result = await sut.QueryAsync(ctx, q);

            Assert.Equal(ids, result.Select(x => x.Id).ToList());
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

        private IContentEntity CreateContent(DomainId id)
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