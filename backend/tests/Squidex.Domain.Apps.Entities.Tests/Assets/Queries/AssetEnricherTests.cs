// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.TestHelpers;

namespace Squidex.Domain.Apps.Entities.Assets.Queries;

public class AssetEnricherTests : GivenContext
{
    [Fact]
    public async Task Should_only_invoke_pre_enrich_for_empty_assets()
    {
        var assets = Array.Empty<EnrichedAsset>();

        var step1 = A.Fake<IAssetEnricherStep>();
        var step2 = A.Fake<IAssetEnricherStep>();

        var sut = new AssetEnricher(new[] { step1, step2 });

        await sut.EnrichAsync(assets, ApiContext, CancellationToken);

        A.CallTo(() => step1.EnrichAsync(ApiContext, CancellationToken))
            .MustHaveHappened();

        A.CallTo(() => step2.EnrichAsync(ApiContext, CancellationToken))
            .MustHaveHappened();

        A.CallTo(() => step1.EnrichAsync(ApiContext, A<IEnumerable<EnrichedAsset>>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => step2.EnrichAsync(ApiContext, A<IEnumerable<EnrichedAsset>>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_invoke_steps()
    {
        var source = CreateAsset();

        var step1 = A.Fake<IAssetEnricherStep>();
        var step2 = A.Fake<IAssetEnricherStep>();

        var sut = new AssetEnricher(new[] { step1, step2 });

        await sut.EnrichAsync(source, ApiContext, CancellationToken);

        A.CallTo(() => step1.EnrichAsync(ApiContext, CancellationToken))
            .MustHaveHappened();

        A.CallTo(() => step2.EnrichAsync(ApiContext, CancellationToken))
            .MustHaveHappened();

        A.CallTo(() => step1.EnrichAsync(ApiContext, A<IEnumerable<EnrichedAsset>>._, CancellationToken))
            .MustHaveHappened();

        A.CallTo(() => step2.EnrichAsync(ApiContext, A<IEnumerable<EnrichedAsset>>._, CancellationToken))
            .MustHaveHappened();
    }
}
