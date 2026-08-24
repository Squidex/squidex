// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities;

public class ContextHeadersTests
{
    private readonly Context sut = Context.Anonymous(new App { Name = "my-app" });

    [Fact]
    public void Should_return_empty_when_header_is_not_set()
    {
        Assert.Empty(sut.AsStrings("X-Fields"));
    }

    [Theory]
    [InlineData("a,b", new[] { "a", "b" })]
    [InlineData("a, b", new[] { "a", "b" })]
    [InlineData(" a ; b ", new[] { "a", "b" })]
    [InlineData("a,,b", new[] { "a", "b" })]
    [InlineData("a,a,b", new[] { "a", "b" })]
    public void Should_split_and_trim_header(string value, string[] expected)
    {
        var context = sut.Clone(b => b.SetHeader("X-Fields", value));

        Assert.Equal(expected, context.AsStrings("X-Fields").ToArray());
    }

    [Fact]
    public void Should_ignore_entries_that_are_only_whitespace()
    {
        // These used to survive as an empty string, which made Languages() throw.
        var context = sut.Clone(b => b.SetHeader("X-Fields", " , "));

        Assert.Empty(context.AsStrings("X-Fields"));
    }

    [Fact]
    public void Should_reuse_parsed_header()
    {
        var context = sut.Clone(b => b.SetHeader("X-Fields", "a,b"));

        var actual1 = context.AsStrings("X-Fields");
        var actual2 = context.AsStrings("X-Fields");

        Assert.Same(actual1, actual2);
    }

    [Fact]
    public void Should_not_share_parsed_headers_with_clone()
    {
        var context1 = sut.Clone(b => b.SetHeader("X-Fields", "a,b"));

        // Parse before cloning, so that a shared cache would be visible.
        Assert.Equal(["a", "b"], context1.AsStrings("X-Fields").ToArray());

        var context2 = context1.Clone(b => b.SetHeader("X-Fields", "c"));

        Assert.Equal(["c"], context2.AsStrings("X-Fields").ToArray());
        Assert.Equal(["a", "b"], context1.AsStrings("X-Fields").ToArray());
    }

    [Fact]
    public void Should_not_see_header_that_has_been_removed_in_clone()
    {
        var context1 = sut.Clone(b => b.SetHeader("X-Fields", "a,b"));

        Assert.NotEmpty(context1.AsStrings("X-Fields"));

        var context2 = context1.Clone(b => b.Remove("X-Fields"));

        Assert.Empty(context2.AsStrings("X-Fields"));
        Assert.NotEmpty(context1.AsStrings("X-Fields"));
    }

    [Fact]
    public void Should_return_same_context_when_nothing_is_changed()
    {
        var context = sut.Clone(_ => { });

        Assert.Same(sut, context);
    }

    [Fact]
    public void Should_parse_languages_from_header()
    {
        var context = sut.Clone(b => b.WithLanguages(["en", "de", "en"]));

        Assert.Equal([Language.EN, Language.DE], context.Languages().ToArray());
    }

    [Fact]
    public void Should_keep_headers_when_app_is_replaced()
    {
        var context1 = sut.Clone(b => b.SetHeader("X-Fields", "a,b"));

        var context2 = context1.WithApp(new App { Name = "other-app" });

        Assert.Equal("other-app", context2.App.Name);
        Assert.Equal(["a", "b"], context2.AsStrings("X-Fields").ToArray());

        // The original must not be affected.
        Assert.Equal("my-app", context1.App.Name);
    }
}
