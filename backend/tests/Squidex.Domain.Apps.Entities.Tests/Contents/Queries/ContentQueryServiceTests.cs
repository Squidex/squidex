// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Security;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public class ContentQueryServiceTests : GivenContext
{
    private readonly IContentEnricher contentEnricher = A.Fake<IContentEnricher>();
    private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
    private readonly IContentLoader contentVersionLoader = A.Fake<IContentLoader>();
    private readonly ContentData contentData = [];
    private readonly ContentQueryParser queryParser = A.Fake<ContentQueryParser>();
    private readonly ContentQueryService sut;

    public ContentQueryServiceTests()
    {
        Schema = Schema with
        {
            Scripts = new SchemaScripts
            {
                Query = "<query-script>"
            },
            IsPublished = true
        };

        SetupEnricher();

        A.CallTo(() => AppProvider.GetSchemasAsync(AppId.Id, CancellationToken))
            .Returns([Schema]);

        A.CallTo(() => queryParser.ParseAsync(A<Context>._, A<Q>._, A<Schema?>._, CancellationToken))
            .ReturnsLazily(c => Task.FromResult(c.GetArgument<Q>(1)!));

        var options = Options.Create(new ContentOptions());

        sut = new ContentQueryService(
            AppProvider,
            contentEnricher,
            contentRepository,
            contentVersionLoader,
            options,
            queryParser);
    }

    [Fact]
    public async Task Should_get_schema_from_guid_string()
    {
        var input = SchemaId.Id.ToString();

        var requestContext = SetupContext();

        var actual = await sut.GetSchemaOrThrowAsync(requestContext, input, CancellationToken);

        Assert.Equal(Schema, actual);
    }

    [Fact]
    public async Task Should_get_schema_from_name()
    {
        var input = SchemaId.Name;

        var requestContext = SetupContext();

        var actual = await sut.GetSchemaOrThrowAsync(requestContext, input, CancellationToken);

        Assert.Equal(Schema, actual);
    }

    [Fact]
    public async Task Should_throw_notfound_exception_if_schema_to_get_not_found()
    {
        var requestContext = SetupContext();

        Schema = null!;

        await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.GetSchemaOrThrowAsync(requestContext, SchemaId.Name, CancellationToken));
    }

    [Fact]
    public async Task Should_throw_permission_exception_if_content_to_find_is_restricted()
    {
        var requestContext = SetupContext(allowSchema: false);

        var content = CreateContent() as Content;

        A.CallTo(() => contentRepository.FindContentAsync(App, Schema, content.Id, A<SearchScope>._, A<CancellationToken>._))
            .Returns(content);

        await Assert.ThrowsAsync<DomainForbiddenException>(() => sut.FindAsync(requestContext, SchemaId.Name, content.Id, ct: CancellationToken));
    }

    [Fact]
    public async Task Should_return_null_if_content_by_id_dannot_be_found()
    {
        var requestContext = SetupContext();

        var content = CreateContent();

        A.CallTo(() => contentRepository.FindContentAsync(App, Schema, content.Id, A<SearchScope>._, A<CancellationToken>._))
            .Returns<Content?>(null);

        var actual = await sut.FindAsync(requestContext, SchemaId.Name, content.Id, ct: CancellationToken);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_return_content_by_special_id()
    {
        var requestContext = SetupContext();

        var content = CreateContent();

        A.CallTo(() => contentRepository.FindContentAsync(App, Schema, SchemaId.Id, SearchScope.Published, A<CancellationToken>._))
            .Returns(content);

        var actual = await sut.FindAsync(requestContext, SchemaId.Name, DomainId.Create("_schemaId_"), ct: CancellationToken);

        AssertContent(content, actual);
    }

    [Theory]
    [InlineData(1, 0, SearchScope.All)]
    [InlineData(1, 1, SearchScope.All)]
    [InlineData(0, 1, SearchScope.All)]
    [InlineData(0, 0, SearchScope.Published)]
    public async Task Should_return_content_by_id(int isFrontend, int unpublished, SearchScope scope)
    {
        var requestContext = SetupContext(isFrontend, isUnpublished: unpublished);

        var content = CreateContent();

        A.CallTo(() => contentRepository.FindContentAsync(App, Schema, content.Id, scope, A<CancellationToken>._))
            .Returns(content);

        var actual = await sut.FindAsync(requestContext, SchemaId.Name, content.Id, ct: CancellationToken);

        AssertContent(content, actual);
    }

    [Fact]
    public async Task Should_return_content_by_id_and_version()
    {
        var requestContext = SetupContext();

        var content = CreateContent();

        A.CallTo(() => contentVersionLoader.GetAsync(AppId.Id, content.Id, 13, A<CancellationToken>._))
            .Returns(content);

        var actual = await sut.FindAsync(requestContext, SchemaId.Name, content.Id, 13, CancellationToken);

        AssertContent(content, actual);
    }

    [Fact]
    public async Task Should_throw_exception_if_user_has_no_permission_to_query_content()
    {
        var requestContext = SetupContext(allowSchema: false);

        await Assert.ThrowsAsync<DomainForbiddenException>(() => sut.QueryAsync(requestContext, SchemaId.Name, Q.Empty, CancellationToken));
    }

    [Theory]
    [InlineData(1, 0, SearchScope.All)]
    [InlineData(1, 1, SearchScope.All)]
    [InlineData(0, 1, SearchScope.All)]
    [InlineData(0, 0, SearchScope.Published)]
    public async Task Should_query_contents(int isFrontend, int unpublished, SearchScope scope)
    {
        var requestContext = SetupContext(isFrontend, isUnpublished: unpublished);

        var content1 = CreateContent();
        var content2 = CreateContent();

        var q = Q.Empty.WithReference(DomainId.NewGuid());

        A.CallTo(() => contentRepository.QueryAsync(App, Schema, q, scope, A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(5, content1, content2));

        var actual = await sut.QueryAsync(requestContext, SchemaId.Name, q, CancellationToken);

        Assert.Equal(5, actual.Total);

        AssertContent(content1, actual[0]);
        AssertContent(content2, actual[1]);
    }

    [Theory]
    [InlineData(1, 0, SearchScope.All)]
    [InlineData(1, 1, SearchScope.All)]
    [InlineData(0, 1, SearchScope.All)]
    [InlineData(0, 0, SearchScope.Published)]
    public async Task Should_query_contents_by_ids(int isFrontend, int unpublished, SearchScope scope)
    {
        var requestContext = SetupContext(isFrontend, isUnpublished: unpublished);

        var contentIds = Enumerable.Range(0, 5).Select(x => DomainId.NewGuid()).ToList();
        var contents = contentIds.Select(x => CreateContent().WithId(x)).ToList();

        var q = Q.Empty.WithIds(contentIds);

        A.CallTo(() => contentRepository.QueryAsync(App,
                A<List<Schema>>.That.Matches(x => x.Count == 1), q, scope,
                A<CancellationToken>._))
            .Returns(ResultList.Create(5, contents));

        var actual = await sut.QueryAsync(requestContext, q, CancellationToken);

        Assert.Equal(5, actual.Total);

        for (var i = 0; i < contents.Count; i++)
        {
            AssertContent(contents[i], actual[i]);
        }
    }

    [Fact]
    public async Task Should_query_contents_with_matching_permissions()
    {
        var requestContext = SetupContext(allowSchema: false);

        var contentIds = Enumerable.Range(0, 5).Select(x => DomainId.NewGuid()).ToList();
        var contents = contentIds.Select(x => CreateContent().WithId(x)).ToList();

        var q = Q.Empty.WithIds(contentIds);

        A.CallTo(() => contentRepository.QueryAsync(App,
                A<List<Schema>>.That.Matches(x => x.Count == 0), q, SearchScope.All,
                A<CancellationToken>._))
            .Returns(ResultList.Create(0, contents));

        var actual = await sut.QueryAsync(requestContext, q, CancellationToken);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_query_contents_from_user_if_user_has_only_own_permission()
    {
        var requestContext = SetupContext(permissionId: PermissionIds.AppContentsReadOwn);

        await sut.QueryAsync(requestContext, SchemaId.Name, Q.Empty, CancellationToken);

        A.CallTo(() => contentRepository.QueryAsync(App, Schema,
                A<Q>.That.Matches(x => Equals(x.CreatedBy, requestContext.UserPrincipal.Token())), SearchScope.Published, A
                <CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_query_all_contents_if_user_has_read_permission()
    {
        var requestContext = SetupContext(permissionId: PermissionIds.AppContentsRead);

        await sut.QueryAsync(requestContext, SchemaId.Name, Q.Empty, CancellationToken);

        A.CallTo(() => contentRepository.QueryAsync(App, Schema,
                A<Q>.That.Matches(x => x.CreatedBy == null), SearchScope.Published,
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    private void SetupEnricher()
    {
        A.CallTo(() => contentEnricher.EnrichAsync(A<IEnumerable<Content>>._, A<Context>._, CancellationToken))
            .ReturnsLazily(x =>
            {
                var input = x.GetArgument<IEnumerable<Content>>(0)!;

                return Task.FromResult<IReadOnlyList<EnrichedContent>>(input.Select(c => SimpleMapper.Map(c, new EnrichedContent())).ToList());
            });
    }

    private Context SetupContext(
        int isFrontend = 0,
        int isUnpublished = 0,
        bool allowSchema = true,
        string permissionId = PermissionIds.AppContentsRead)
    {
        var permissions = new List<string>();

        if (allowSchema)
        {
            var concretePermission = PermissionIds.ForApp(permissionId, AppId.Name, SchemaId.Name).Id;

            permissions.Add(concretePermission);
        }

        return CreateContext(isFrontend == 1, permissions.ToArray()).Clone(b => b.WithUnpublished(isUnpublished == 1));
    }

    private static void AssertContent(EnrichedContent source, EnrichedContent? actual)
    {
        Assert.NotNull(actual);
        Assert.NotSame(source, actual);
        Assert.Same(source.Data, actual?.Data);
        Assert.Equal(source.Id, actual?.Id);
    }
}
