// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Assets.Queries.Steps;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Assets.Queries;

public class CalculateTokensTests : GivenContext
{
    private readonly IJsonSerializer serializer = A.Fake<IJsonSerializer>();
    private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
    private readonly CalculateTokens sut;

    public CalculateTokensTests()
    {
        sut = new CalculateTokens(urlGenerator, serializer);
    }

    [Fact]
    public async Task Should_not_enrich_asset_edit_tokens_if_disabled()
    {
        var asset = CreateAsset();

        await sut.EnrichAsync(ApiContext.Clone(b => b.WithNoAssetEnrichment()), Enumerable.Repeat(asset, 1), default);

        Assert.Null(asset.EditToken);
    }

    [Fact]
    public async Task Should_compute_ui_tokens()
    {
        var asset = CreateAsset();

        await sut.EnrichAsync(ApiContext, [asset], CancellationToken);

        Assert.NotNull(asset.EditToken);

        A.CallTo(() => urlGenerator.Root())
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_also_compute_ui_tokens_for_frontend()
    {
        var asset = CreateAsset();

        await sut.EnrichAsync(FrontendContext, [asset], CancellationToken);

        Assert.NotNull(asset.EditToken);

        A.CallTo(() => urlGenerator.Root())
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_compute_ui_token_with_stable_format()
    {
        var asset = CreateAsset();

        A.CallTo(() => urlGenerator.Root())
            .Returns("https://squidex.io");

        var target = new CalculateTokens(urlGenerator, TestUtils.DefaultSerializer);

        await target.EnrichAsync(ApiContext, [asset], CancellationToken);

        var actual = Encoding.UTF8.GetString(Convert.FromBase64String(asset.EditToken!));

        var expected = $$"""{"a":"{{asset.AppId.Name}}","i":"{{asset.Id}}","u":"https://squidex.io"}""";

        Assert.Equal(expected, actual);
    }
}
