// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Queries.Steps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public class EnrichForCachingTests
{
    private readonly ISchemaEntity schema;
    private readonly IRequestCache requestCache = A.Fake<IRequestCache>();
    private readonly Context requestContext;
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
    private readonly ProvideSchema schemaProvider;
    private readonly EnrichForCaching sut;

    public EnrichForCachingTests()
    {
        requestContext = new Context(Mocks.ApiUser(), Mocks.App(appId));

        schema = Mocks.Schema(appId, schemaId);
        schemaProvider = x => Task.FromResult((schema, ResolvedComponents.Empty));

        sut = new EnrichForCaching(requestCache);
    }

    [Fact]
    public async Task Should_add_cache_headers()
    {
        var headers = new List<string>();

        A.CallTo(() => requestCache.AddHeader(A<string>._))
            .Invokes(new Action<string>(header => headers.Add(header)));

        await sut.EnrichAsync(requestContext, default);

        Assert.Equal(new List<string>
        {
            "X-Flatten",
            "X-Languages",
            "X-NoCleanup",
            "X-NoEnrichment",
            "X-NoResolveLanguages",
            "X-ResolveFlow",
            "X-Resolve-Urls",
            "X-Unpublished"
        }, headers);
    }

    [Fact]
    public async Task Should_add_app_version_and_schema_as_dependency()
    {
        var content = CreateContent();

        await sut.EnrichAsync(requestContext, Enumerable.Repeat(content, 1), schemaProvider, default);

        A.CallTo(() => requestCache.AddDependency(content.UniqueId, content.Version))
            .MustHaveHappened();

        A.CallTo(() => requestCache.AddDependency(schema.UniqueId, schema.Version))
            .MustHaveHappened();

        A.CallTo(() => requestCache.AddDependency(requestContext.App.UniqueId, requestContext.App.Version))
            .MustHaveHappened();
    }

    private ContentEntity CreateContent()
    {
        return new ContentEntity { AppId = appId, Id = DomainId.NewGuid(), SchemaId = schemaId, Version = 13 };
    }
}
