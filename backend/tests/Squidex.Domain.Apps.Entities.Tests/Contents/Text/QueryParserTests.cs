// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public class QueryParserTests
{
    private readonly QueryParser sut = new QueryParser(x => $"texts.{x}");

    [Fact]
    public void Should_prefix_field_query()
    {
        var source = "en:Hello";

        Assert.Equal("texts.en:Hello", sut.Parse(source)?.Text);
    }

    [Fact]
    public void Should_prefix_field_with_complex_language()
    {
        var source = "en-EN:Hello";

        Assert.Equal("texts.en-EN:Hello", sut.Parse(source)?.Text);
    }

    [Fact]
    public void Should_prefix_field_query_within_query()
    {
        var source = "Hello en:World";

        Assert.Equal("Hello texts.en:World", sut.Parse(source)?.Text);
    }

    [Fact]
    public void Should_prefix_field_query_within_complex_query()
    {
        var source = "Hallo OR (Hello en:World)";

        Assert.Equal("Hallo OR (Hello texts.en:World)", sut.Parse(source)?.Text);
    }
}
