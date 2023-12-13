// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Queries.Steps;
using Squidex.Domain.Apps.Entities.TestHelpers;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public class EnrichWithSchemaTests : GivenContext
{
    private readonly EnrichWithSchema sut;

    public EnrichWithSchemaTests()
    {
        sut = new EnrichWithSchema();
    }

    [Fact]
    public async Task Should_enrich_with_reference_fields()
    {
        var content = CreateContent();

        await sut.EnrichAsync(FrontendContext, new[] { content }, SchemaProvider(), CancellationToken);

        Assert.NotNull(content.ReferenceFields);
    }

    [Fact]
    public async Task Should_not_enrich_with_reference_fields_if_not_frontend()
    {
        var content = CreateContent();

        await sut.EnrichAsync(ApiContext, new[] { content }, SchemaProvider(), CancellationToken);

        Assert.Null(content.ReferenceFields);
    }

    [Fact]
    public async Task Should_enrich_with_schema_names()
    {
        var content = CreateContent();

        await sut.EnrichAsync(ApiContext, new[] { content }, SchemaProvider(), CancellationToken);

        Assert.Equal("my-schema", content.SchemaDisplayName);
    }

    private ProvideSchema SchemaProvider()
    {
        return x => Task.FromResult((Schema, ResolvedComponents.Empty));
    }
}
