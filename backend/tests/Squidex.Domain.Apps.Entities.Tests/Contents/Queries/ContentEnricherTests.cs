// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public class ContentEnricherTests : GivenContext
{
    private sealed class ResolveSchema : IContentEnricherStep
    {
        public Schema Schema { get; private set; }

        public async Task EnrichAsync(Context context, IEnumerable<EnrichedContent> contents, ProvideSchema schemas,
            CancellationToken ct)
        {
            foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
            {
                Schema = (await schemas(group.Key)).Schema;
            }
        }
    }

    [Fact]
    public async Task Should_only_invoke_pre_enrich_for_empty_contents()
    {
        var source = Array.Empty<Content>();

        var step1 = A.Fake<IContentEnricherStep>();
        var step2 = A.Fake<IContentEnricherStep>();

        var sut = new ContentEnricher(new[] { step1, step2 }, AppProvider);

        await sut.EnrichAsync(source, ApiContext, CancellationToken);

        A.CallTo(() => step1.EnrichAsync(ApiContext, CancellationToken))
            .MustHaveHappened();

        A.CallTo(() => step2.EnrichAsync(ApiContext, CancellationToken))
            .MustHaveHappened();

        A.CallTo(() => step1.EnrichAsync(ApiContext, A<IEnumerable<EnrichedContent>>._, A<ProvideSchema>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => step2.EnrichAsync(ApiContext, A<IEnumerable<EnrichedContent>>._, A<ProvideSchema>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_invoke_steps()
    {
        var source = CreateContent();

        var step1 = A.Fake<IContentEnricherStep>();
        var step2 = A.Fake<IContentEnricherStep>();

        var sut = new ContentEnricher(new[] { step1, step2 }, AppProvider);

        await sut.EnrichAsync(source, false, ApiContext, CancellationToken);

        A.CallTo(() => step1.EnrichAsync(ApiContext, CancellationToken))
            .MustHaveHappened();

        A.CallTo(() => step2.EnrichAsync(ApiContext, CancellationToken))
            .MustHaveHappened();

        A.CallTo(() => step1.EnrichAsync(ApiContext, A<IEnumerable<EnrichedContent>>._, A<ProvideSchema>._, CancellationToken))
            .MustHaveHappened();

        A.CallTo(() => step2.EnrichAsync(ApiContext, A<IEnumerable<EnrichedContent>>._, A<ProvideSchema>._, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_provide_and_cache_schema()
    {
        var source = CreateContent();

        var step1 = new ResolveSchema();
        var step2 = new ResolveSchema();

        var sut = new ContentEnricher(new[] { step1, step2 }, AppProvider);

        await sut.EnrichAsync(source, false, ApiContext, CancellationToken);

        Assert.Same(Schema, step1.Schema);
        Assert.Same(Schema, step1.Schema);

        A.CallTo(() => AppProvider.GetSchemaAsync(AppId.Id, SchemaId.Id, false, CancellationToken))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_clone_data_if_requested()
    {
        var source = CreateContent();

        var sut = new ContentEnricher(Enumerable.Empty<IContentEnricherStep>(), AppProvider);

        var actual = await sut.EnrichAsync(source, true, ApiContext, CancellationToken);

        Assert.NotSame(source.Data, actual.Data);
    }

    [Fact]
    public async Task Should_not_clone_data_if_not_requested()
    {
        var source = CreateContent();

        var sut = new ContentEnricher(Enumerable.Empty<IContentEnricherStep>(), AppProvider);

        var actual = await sut.EnrichAsync(source, false, ApiContext, CancellationToken);

        Assert.Same(source.Data, actual.Data);
    }
}
