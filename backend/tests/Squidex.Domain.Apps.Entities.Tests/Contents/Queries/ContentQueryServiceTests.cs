// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Options;
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
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly CancellationToken ct;
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly IContentEnricher contentEnricher = A.Fake<IContentEnricher>();
        private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
        private readonly IContentLoader contentVersionLoader = A.Fake<IContentLoader>();
        private readonly ISchemaEntity schema;
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
        private readonly ContentData contentData = new ContentData();
        private readonly ContentQueryParser queryParser = A.Fake<ContentQueryParser>();
        private readonly ContentQueryService sut;

        public ContentQueryServiceTests()
        {
            ct = cts.Token;

            var schemaDef =
                new Schema(schemaId.Name)
                    .Publish()
                    .SetScripts(new SchemaScripts { Query = "<query-script>" });

            schema = Mocks.Schema(appId, schemaId, schemaDef);

            SetupEnricher();

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Name, A<bool>._, ct))
                .Returns(schema);

            A.CallTo(() => appProvider.GetSchemasAsync(appId.Id, ct))
                .Returns(new List<ISchemaEntity> { schema });

            A.CallTo(() => queryParser.ParseAsync(A<Context>._, A<Q>._, A<ISchemaEntity?>._))
                .ReturnsLazily(c => Task.FromResult(c.GetArgument<Q>(1)!));

            var options = Options.Create(new ContentOptions());

            sut = new ContentQueryService(
                appProvider,
                contentEnricher,
                contentRepository,
                contentVersionLoader,
                options,
                queryParser);
        }

        [Fact]
        public async Task Should_get_schema_from_guid_string()
        {
            var input = schemaId.Id.ToString();

            var requestContext = CreateContext();

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Id, true, ct))
                .Returns(schema);

            var result = await sut.GetSchemaOrThrowAsync(requestContext, input, ct);

            Assert.Equal(schema, result);
        }

        [Fact]
        public async Task Should_get_schema_from_name()
        {
            var input = schemaId.Name;

            var requestContext = CreateContext();

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Name, true, ct))
                .Returns(schema);

            var result = await sut.GetSchemaOrThrowAsync(requestContext, input, ct);

            Assert.Equal(schema, result);
        }

        [Fact]
        public async Task Should_throw_notfound_exception_if_schema_to_get_not_found()
        {
            var requestContext = CreateContext();

            A.CallTo(() => appProvider.GetSchemaAsync(A<DomainId>._, A<string>._, true, ct))
                .Returns((ISchemaEntity?)null);

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.GetSchemaOrThrowAsync(requestContext, schemaId.Name, ct));
        }

        [Fact]
        public async Task Should_throw_permission_exception_if_content_to_find_is_restricted()
        {
            var requestContext = CreateContext(allowSchema: false);

            var content = CreateContent(DomainId.NewGuid());

            A.CallTo(() => contentRepository.FindContentAsync(requestContext.App, schema, content.Id, A<SearchScope>._, A<CancellationToken>._))
                .Returns(CreateContent(DomainId.NewGuid()));

            await Assert.ThrowsAsync<DomainForbiddenException>(() => sut.FindAsync(requestContext, schemaId.Name, content.Id, ct: ct));
        }

        [Fact]
        public async Task Should_return_null_if_content_by_id_dannot_be_found()
        {
            var requestContext = CreateContext();

            var content = CreateContent(DomainId.NewGuid());

            A.CallTo(() => contentRepository.FindContentAsync(requestContext.App, schema, content.Id, A<SearchScope>._, A<CancellationToken>._))
                .Returns<IContentEntity?>(null);

            var result = await sut.FindAsync(requestContext, schemaId.Name, content.Id, ct: ct);

            Assert.Null(result);
        }

        [Fact]
        public async Task Should_return_content_by_special_id()
        {
            var requestContext = CreateContext();

            var content = CreateContent(DomainId.NewGuid());

            A.CallTo(() => contentRepository.FindContentAsync(requestContext.App, schema, schema.Id, SearchScope.Published, A<CancellationToken>._))
                .Returns(content);

            var result = await sut.FindAsync(requestContext, schemaId.Name, DomainId.Create("_schemaId_"), ct: ct);

            AssertContent(content, result);
        }

        [Theory]
        [InlineData(1, 0, SearchScope.All)]
        [InlineData(1, 1, SearchScope.All)]
        [InlineData(0, 1, SearchScope.All)]
        [InlineData(0, 0, SearchScope.Published)]
        public async Task Should_return_content_by_id(int isFrontend, int unpublished, SearchScope scope)
        {
            var requestContext = CreateContext(isFrontend, isUnpublished: unpublished);

            var content = CreateContent(DomainId.NewGuid());

            A.CallTo(() => contentRepository.FindContentAsync(requestContext.App, schema, content.Id, scope, A<CancellationToken>._))
                .Returns(content);

            var result = await sut.FindAsync(requestContext, schemaId.Name, content.Id, ct: ct);

            AssertContent(content, result);
        }

        [Fact]
        public async Task Should_return_content_by_id_and_version()
        {
            var requestContext = CreateContext();

            var content = CreateContent(DomainId.NewGuid());

            A.CallTo(() => contentVersionLoader.GetAsync(appId.Id, content.Id, 13))
                .Returns(content);

            var result = await sut.FindAsync(requestContext, schemaId.Name, content.Id, 13, ct);

            AssertContent(content, result);
        }

        [Fact]
        public async Task Should_throw_exception_if_user_has_no_permission_to_query_content()
        {
            var requestContext = CreateContext(allowSchema: false);

            await Assert.ThrowsAsync<DomainForbiddenException>(() => sut.QueryAsync(requestContext, schemaId.Name, Q.Empty, ct));
        }

        [Theory]
        [InlineData(1, 0, SearchScope.All)]
        [InlineData(1, 1, SearchScope.All)]
        [InlineData(0, 1, SearchScope.All)]
        [InlineData(0, 0, SearchScope.Published)]
        public async Task Should_query_contents(int isFrontend, int unpublished, SearchScope scope)
        {
            var requestContext = CreateContext(isFrontend, isUnpublished: unpublished);

            var content1 = CreateContent(DomainId.NewGuid());
            var content2 = CreateContent(DomainId.NewGuid());

            var q = Q.Empty.WithReference(DomainId.NewGuid());

            A.CallTo(() => contentRepository.QueryAsync(requestContext.App, schema, q, scope, A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(5, content1, content2));

            var result = await sut.QueryAsync(requestContext, schemaId.Name, q, ct);

            Assert.Equal(5, result.Total);

            AssertContent(content1, result[0]);
            AssertContent(content2, result[1]);
        }

        [Theory]
        [InlineData(1, 0, SearchScope.All)]
        [InlineData(1, 1, SearchScope.All)]
        [InlineData(0, 1, SearchScope.All)]
        [InlineData(0, 0, SearchScope.Published)]
        public async Task Should_query_contents_by_ids(int isFrontend, int unpublished, SearchScope scope)
        {
            var requestContext = CreateContext(isFrontend, isUnpublished: unpublished);

            var ids = Enumerable.Range(0, 5).Select(x => DomainId.NewGuid()).ToList();

            var contents = ids.Select(CreateContent).ToList();

            var q = Q.Empty.WithIds(ids);

            A.CallTo(() => contentRepository.QueryAsync(requestContext.App,
                    A<List<ISchemaEntity>>.That.Matches(x => x.Count == 1), q, scope,
                    A<CancellationToken>._))
                .Returns(ResultList.Create(5, contents));

            var result = await sut.QueryAsync(requestContext, q, ct);

            Assert.Equal(5, result.Total);

            for (var i = 0; i < contents.Count; i++)
            {
                AssertContent(contents[i], result[i]);
            }
        }

        [Fact]
        public async Task Should_query_contents_with_matching_permissions()
        {
            var requestContext = CreateContext(allowSchema: false);

            var ids = Enumerable.Range(0, 5).Select(x => DomainId.NewGuid()).ToList();

            var q = Q.Empty.WithIds(ids);

            A.CallTo(() => contentRepository.QueryAsync(requestContext.App,
                    A<List<ISchemaEntity>>.That.Matches(x => x.Count == 0), q, SearchScope.All,
                    A<CancellationToken>._))
                .Returns(ResultList.Create(0, ids.Select(CreateContent)));

            var result = await sut.QueryAsync(requestContext, q, ct);

            Assert.Empty(result);
        }

        [Fact]
        public async Task Should_query_contents_from_user_if_user_has_only_own_permission()
        {
            var requestContext = CreateContext(permissionId: Permissions.AppContentsReadOwn);

            await sut.QueryAsync(requestContext, schemaId.Name, Q.Empty, ct);

            A.CallTo(() => contentRepository.QueryAsync(requestContext.App, schema,
                    A<Q>.That.Matches(x => Equals(x.CreatedBy, requestContext.User.Token())), SearchScope.Published, A
                    <CancellationToken>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_query_all_contents_if_user_has_read_permission()
        {
            var requestContext = CreateContext(permissionId: Permissions.AppContentsRead);

            await sut.QueryAsync(requestContext, schemaId.Name, Q.Empty, ct);

            A.CallTo(() => contentRepository.QueryAsync(requestContext.App, schema,
                    A<Q>.That.Matches(x => x.CreatedBy == null), SearchScope.Published,
                    A<CancellationToken>._))
                .MustHaveHappened();
        }

        private void SetupEnricher()
        {
            A.CallTo(() => contentEnricher.EnrichAsync(A<IEnumerable<IContentEntity>>._, A<Context>._, ct))
                .ReturnsLazily(x =>
                {
                    var input = x.GetArgument<IEnumerable<IContentEntity>>(0)!;

                    return Task.FromResult<IReadOnlyList<IEnrichedContentEntity>>(input.Select(c => SimpleMapper.Map(c, new ContentEntity())).ToList());
                });
        }

        private Context CreateContext(
            int isFrontend = 0,
            int isUnpublished = 0,
            bool allowSchema = true,
            string permissionId = Permissions.AppContentsRead)
        {
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            claimsIdentity.AddClaim(new Claim(OpenIdClaims.Subject, "user1"));

            if (isFrontend == 1)
            {
                claimsIdentity.AddClaim(new Claim(OpenIdClaims.ClientId, DefaultClients.Frontend));
            }

            if (allowSchema)
            {
                var concretePermission = Permissions.ForApp(permissionId, appId.Name, schemaId.Name).Id;

                claimsIdentity.AddClaim(new Claim(SquidexClaimTypes.Permissions, concretePermission));
            }

            return new Context(claimsPrincipal, Mocks.App(appId)).Clone(b => b.WithUnpublished(isUnpublished == 1));
        }

        private static void AssertContent(IContentEntity source, IEnrichedContentEntity? result)
        {
            Assert.NotNull(result);
            Assert.NotSame(source, result);
            Assert.Same(source.Data, result?.Data);
            Assert.Equal(source.Id, result?.Id);
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
