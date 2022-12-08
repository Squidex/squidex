// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public class ContentEnricherTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
    private readonly ISchemaEntity schema;
    private readonly Context requestContext;
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");

    private sealed class ResolveSchema : IContentEnricherStep
    {
        public ISchemaEntity Schema { get; private set; }

        public async Task EnrichAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas,
            CancellationToken ct)
        {
            foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
            {
                Schema = (await schemas(group.Key)).Schema;
            }
        }
    }

    public ContentEnricherTests()
    {
        ct = cts.Token;

        requestContext = new Context(Mocks.ApiUser(), Mocks.App(appId));

        schema = Mocks.Schema(appId, schemaId);

        A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Id, false, ct))
            .Returns(schema);
    }

    [Fact]
    public async Task Should_only_invoke_pre_enrich_for_empty_actuals()
    {
        var source = Array.Empty<IContentEntity>();

        var step1 = A.Fake<IContentEnricherStep>();
        var step2 = A.Fake<IContentEnricherStep>();

        var sut = new ContentEnricher(new[] { step1, step2 }, appProvider);

        await sut.EnrichAsync(source, requestContext, ct);

        A.CallTo(() => step1.EnrichAsync(requestContext, ct))
            .MustHaveHappened();

        A.CallTo(() => step2.EnrichAsync(requestContext, ct))
            .MustHaveHappened();

        A.CallTo(() => step1.EnrichAsync(requestContext, A<IEnumerable<ContentEntity>>._, A<ProvideSchema>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => step2.EnrichAsync(requestContext, A<IEnumerable<ContentEntity>>._, A<ProvideSchema>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_invoke_steps()
    {
        var source = CreateContent();

        var step1 = A.Fake<IContentEnricherStep>();
        var step2 = A.Fake<IContentEnricherStep>();

        var sut = new ContentEnricher(new[] { step1, step2 }, appProvider);

        await sut.EnrichAsync(source, false, requestContext, ct);

        A.CallTo(() => step1.EnrichAsync(requestContext, ct))
            .MustHaveHappened();

        A.CallTo(() => step2.EnrichAsync(requestContext, ct))
            .MustHaveHappened();

        A.CallTo(() => step1.EnrichAsync(requestContext, A<IEnumerable<ContentEntity>>._, A<ProvideSchema>._, ct))
            .MustHaveHappened();

        A.CallTo(() => step2.EnrichAsync(requestContext, A<IEnumerable<ContentEntity>>._, A<ProvideSchema>._, ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_provide_and_cache_schema()
    {
        var source = CreateContent();

        var step1 = new ResolveSchema();
        var step2 = new ResolveSchema();

        var sut = new ContentEnricher(new[] { step1, step2 }, appProvider);

        await sut.EnrichAsync(source, false, requestContext, ct);

        Assert.Same(schema, step1.Schema);
        Assert.Same(schema, step1.Schema);

        A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Id, false, ct))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_clone_data_if_requested()
    {
        var source = CreateContent(new ContentData());

        var sut = new ContentEnricher(Enumerable.Empty<IContentEnricherStep>(), appProvider);

        var actual = await sut.EnrichAsync(source, true, requestContext, ct);

        Assert.NotSame(source.Data, actual.Data);
    }

    [Fact]
    public async Task Should_not_clone_data_if_not_requested()
    {
        var source = CreateContent(new ContentData());

        var sut = new ContentEnricher(Enumerable.Empty<IContentEnricherStep>(), appProvider);

        var actual = await sut.EnrichAsync(source, false, requestContext, ct);

        Assert.Same(source.Data, actual.Data);
    }

    private ContentEntity CreateContent(ContentData? data = null)
    {
        return new ContentEntity { SchemaId = schemaId, Data = data! };
    }
}
