// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Model.Contents;

public class RichTextTests
{
    private readonly RichTextNode node = new RichTextNode();

    public RichTextTests()
    {
        var json = TestUtils.DefaultSerializer.Deserialize<JsonValue>(File.ReadAllText("Model/Contents/ComplexText.json"));

        node.TryUse(json);
    }

    [Fact]
    public void Should_format_to_html()
    {
        var expected = File.ReadAllText("Model/Contents/ComplexText.html");

        Assert.Equal(expected.Trim(), node.ToHtml().Trim());
    }

    [Fact]
    public void Should_format_to_minimized_html()
    {
        var expected = File.ReadAllText("Model/Contents/ComplexText.min.html");

        Assert.Equal(expected.Trim(), node.ToHtml(0).Trim());
    }

    [Fact]
    public void Should_format_to_markdown()
    {
        var expected = File.ReadAllText("Model/Contents/ComplexText.md");

        Assert.Equal(expected.Trim(), node.ToMarkdown().Trim());
    }

    [Fact]
    public void Should_format_to_text()
    {
        var expected = File.ReadAllText("Model/Contents/ComplexText.txt");

        Assert.Equal(expected.Trim(), node.ToText().Trim());
    }
}
