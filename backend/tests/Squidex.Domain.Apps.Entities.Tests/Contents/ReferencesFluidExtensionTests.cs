// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Templates;
using Squidex.Domain.Apps.Core.Templates.Extensions;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents;

public class ReferencesFluidExtensionTests : GivenContext
{
    private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
    private readonly FluidTemplateEngine sut;

    public ReferencesFluidExtensionTests()
    {
        var serviceProvider =
            new ServiceCollection()
                .AddSingleton(AppProvider)
                .AddSingleton(contentQuery)
                .BuildServiceProvider();

        var extensions = new IFluidExtension[]
        {
            new ContentFluidExtension(),
            new ReferencesFluidExtension(serviceProvider)
        };

        sut = new FluidTemplateEngine(extensions);
    }

    [Fact]
    public async Task Should_resolve_references_in_loop()
    {
        var template = @"
                {% for id in event.data.references.iv %}
                    {% reference 'ref', id %}
                    Text: {{ ref.data.field1.iv }} {{ ref.data.field2.iv }} {{ ref.id }}
                {% endfor %}
            ";

        await ResolveReferencesAsync(template);
    }

    [Fact]
    public async Task Should_resolve_references_in_loop_without_commata()
    {
        var template = @"
                {% for id in event.data.references.iv %}
                    {% reference 'ref' id %}
                    Text: {{ ref.data.field1.iv }} {{ ref.data.field2.iv }} {{ ref.id }}
                {% endfor %}
            ";

        await ResolveReferencesAsync(template);
    }

    [Fact]
    public async Task Should_resolve_references_in_loop_with_filter()
    {
        var template = @"
                {% for id in event.data.references.iv %}
                    {% assign ref = id | reference %}
                    Text: {{ ref.data.field1.iv }} {{ ref.data.field2.iv }} {{ ref.id }}
                {% endfor %}
            ";

        await ResolveReferencesAsync(template);
    }

    private async Task ResolveReferencesAsync(string template)
    {
        var referenceId1 = DomainId.NewGuid();
        var reference1 = CreateReference(referenceId1, 1);
        var referenceId2 = DomainId.NewGuid();
        var reference2 = CreateReference(referenceId2, 2);

        var @event = new EnrichedContentEvent
        {
            Data =
                new ContentData()
                    .AddField("references",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(referenceId1, referenceId2))),
            AppId = AppId
        };

        A.CallTo(() => contentQuery.QueryAsync(A<Context>._, A<Q>.That.HasIds(referenceId1), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, reference1));

        A.CallTo(() => contentQuery.QueryAsync(A<Context>._, A<Q>.That.HasIds(referenceId2), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, reference2));

        var vars = new TemplateVars
        {
            ["event"] = @event
        };

        var expected = $@"
                Text: Hello 1 World 1 {referenceId1}
                Text: Hello 2 World 2 {referenceId2}
            ";

        var actual = await sut.RenderAsync(template, vars);

        Assert.Equal(Cleanup(expected), Cleanup(actual));
    }

    private EnrichedContent CreateReference(DomainId referenceId, int index)
    {
        return CreateContent() with
        {
            Data =
                new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Create($"Hello {index}")))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Create($"World {index}"))),
            Id = referenceId
        };
    }

    private static string Cleanup(string text)
    {
        return text
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("\n", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal);
    }
}
