// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.Queries;

public class AssetEnricherTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly Context requestContext;
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");

    public AssetEnricherTests()
    {
        ct = cts.Token;

        requestContext = new Context(Mocks.ApiUser(), Mocks.App(appId));
    }

    [Fact]
    public async Task Should_only_invoke_pre_enrich_for_empty_actuals()
    {
        var source = Array.Empty<IAssetEntity>();

        var step1 = A.Fake<IAssetEnricherStep>();
        var step2 = A.Fake<IAssetEnricherStep>();

        var sut = new AssetEnricher(new[] { step1, step2 });

        await sut.EnrichAsync(source, requestContext, ct);

        A.CallTo(() => step1.EnrichAsync(requestContext, ct))
            .MustHaveHappened();

        A.CallTo(() => step2.EnrichAsync(requestContext, ct))
            .MustHaveHappened();

        A.CallTo(() => step1.EnrichAsync(requestContext, A<IEnumerable<AssetEntity>>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => step2.EnrichAsync(requestContext, A<IEnumerable<AssetEntity>>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_invoke_steps()
    {
        var source = CreateAsset();

        var step1 = A.Fake<IAssetEnricherStep>();
        var step2 = A.Fake<IAssetEnricherStep>();

        var sut = new AssetEnricher(new[] { step1, step2 });

        await sut.EnrichAsync(source, requestContext, ct);

        A.CallTo(() => step1.EnrichAsync(requestContext, ct))
            .MustHaveHappened();

        A.CallTo(() => step2.EnrichAsync(requestContext, ct))
            .MustHaveHappened();

        A.CallTo(() => step1.EnrichAsync(requestContext, A<IEnumerable<AssetEntity>>._, ct))
            .MustHaveHappened();

        A.CallTo(() => step2.EnrichAsync(requestContext, A<IEnumerable<AssetEntity>>._, ct))
            .MustHaveHappened();
    }

    private AssetEntity CreateAsset()
    {
        return new AssetEntity { Id = DomainId.NewGuid(), AppId = appId };
    }
}
