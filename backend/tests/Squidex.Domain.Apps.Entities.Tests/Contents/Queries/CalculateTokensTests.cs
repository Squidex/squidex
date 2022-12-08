// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Queries.Steps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public class CalculateTokensTests
{
    private readonly ISchemaEntity schema;
    private readonly IJsonSerializer serializer = A.Fake<IJsonSerializer>();
    private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
    private readonly Context requestContext;
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
    private readonly ProvideSchema schemaProvider;
    private readonly CalculateTokens sut;

    public CalculateTokensTests()
    {
        requestContext = new Context(Mocks.ApiUser(), Mocks.App(appId));

        schema = Mocks.Schema(appId, schemaId);
        schemaProvider = x => Task.FromResult((schema, ResolvedComponents.Empty));

        sut = new CalculateTokens(urlGenerator, serializer);
    }

    [Fact]
    public async Task Should_compute_ui_tokens()
    {
        var source = CreateContent();

        await sut.EnrichAsync(requestContext, new[] { source }, schemaProvider, default);

        Assert.NotNull(source.EditToken);

        A.CallTo(() => urlGenerator.Root())
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_also_compute_ui_tokens_for_frontend()
    {
        var source = CreateContent();

        await sut.EnrichAsync(new Context(Mocks.FrontendUser(), Mocks.App(appId)), new[] { source }, schemaProvider, default);

        Assert.NotNull(source.EditToken);

        A.CallTo(() => urlGenerator.Root())
            .MustHaveHappened();
    }

    private ContentEntity CreateContent()
    {
        return new ContentEntity { AppId = appId, SchemaId = schemaId };
    }
}
