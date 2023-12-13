// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Queries.Steps;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

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
    public async Task Should_compute_ui_tokens()
    {
        var content = CreateContent();

        await sut.EnrichAsync(ApiContext, new[] { content }, SchemaProvider(), CancellationToken);

        Assert.NotNull(content.EditToken);

        A.CallTo(() => urlGenerator.Root())
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_also_compute_ui_tokens_for_frontend()
    {
        var content = CreateContent();

        await sut.EnrichAsync(FrontendContext, new[] { content }, SchemaProvider(), CancellationToken);

        Assert.NotNull(content.EditToken);

        A.CallTo(() => urlGenerator.Root())
            .MustHaveHappened();
    }

    private ProvideSchema SchemaProvider()
    {
        return x => Task.FromResult((Schema, ResolvedComponents.Empty));
    }
}
