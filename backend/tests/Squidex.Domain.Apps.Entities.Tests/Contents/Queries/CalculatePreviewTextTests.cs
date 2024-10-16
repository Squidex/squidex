// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Queries.Steps;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public class CalculatePreviewTextTests : GivenContext
{
    private readonly CalculatePreviewText sut;

    public CalculatePreviewTextTests()
    {
        sut = new CalculatePreviewText();

        Schema = Schema.AddRichText(1, "richText", Partitioning.Language);
    }

    [Fact]
    public async Task Should_compute_texts()
    {
        var content = CreateContentWithData();

        await sut.EnrichAsync(ApiContext, [content], SchemaProvider(), CancellationToken);

        Assert.Equal("Text1Text2", content.ReferenceData!["richText"]!["en"]);
        Assert.Equal("Text3Text4", content.ReferenceData!["richText"]!["de"]);
    }

    [Fact]
    public async Task Should_not_compute_texts_if_field_not_found()
    {
        var content = CreateContent();

        await sut.EnrichAsync(ApiContext, [content], SchemaProvider(), CancellationToken);

        Assert.Null(content.ReferenceData);
    }

    private EnrichedContent CreateContentWithData()
    {
        var content = CreateContent() with
        {
            Data =
                new ContentData()
                    .AddField("richText",
                        new ContentFieldData()
                            .AddLocalized("en",
                                JsonValue.Object()
                                    .Add("type", "paragraph")
                                    .Add("content", JsonValue.Array(
                                        JsonValue.Object()
                                            .Add("type", "text")
                                            .Add("text", "Text1"),
                                        JsonValue.Object()
                                            .Add("type", "text")
                                            .Add("text", "Text2"))))
                            .AddLocalized("de",
                                JsonValue.Object()
                                    .Add("type", "paragraph")
                                    .Add("content", JsonValue.Array(
                                        JsonValue.Object()
                                            .Add("type", "text")
                                            .Add("text", "Text3"),
                                        JsonValue.Object()
                                            .Add("type", "text")
                                            .Add("text", "Text4")))))
        };

        return content;
    }

    private ProvideSchema SchemaProvider()
    {
        return x => Task.FromResult((Schema, ResolvedComponents.Empty));
    }
}
