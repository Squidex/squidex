// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Templates;
using Squidex.Domain.Apps.Core.Templates.Extensions;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Operations.Templates;

public class FluidTemplateEngineTests
{
    private readonly FluidTemplateEngine sut;

    public FluidTemplateEngineTests()
    {
        var extensions = new IFluidExtension[]
        {
            new ContentFluidExtension(),
            new DateTimeFluidExtension(),
            new StringFluidExtension(),
            new StringWordsFluidExtension()
        };

        sut = new FluidTemplateEngine(extensions);
    }

    [Theory]
    [InlineData("{{ e.user }}", "subject:me")]
    [InlineData("{{ e.user.type }}", "subject")]
    [InlineData("{{ e.user.identifier }}", "me")]
    public async Task Should_render_ref_token(string template, string expected)
    {
        var value = new
        {
            User = RefToken.User("me")
        };

        var actual = await RenderAync(template, value);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("{{ e.id }}", "42,my-app")]
    [InlineData("{{ e.id.name }}", "my-app")]
    [InlineData("{{ e.id.id }}", "42")]
    public async Task Should_render_named_id(string template, string expected)
    {
        var value = new
        {
            Id = NamedId.Of("42", "my-app")
        };

        var actual = await RenderAync(template, value);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Should_format_domain_id()
    {
        var value = new
        {
            Id = DomainId.NewGuid()
        };

        var template = "{{ e.id }}";

        var actual = await RenderAync(template, value);

        Assert.Equal(value.Id.ToString(), actual);
    }

    [Fact]
    public async Task Should_format_enum()
    {
        var value = new
        {
            Type = EnrichedContentEventType.Created
        };

        var template = "{{ e.type }}";

        var actual = await RenderAync(template, value);

        Assert.Equal(value.Type.ToString(), actual);
    }

    [Fact]
    public async Task Should_format_date()
    {
        var now = DateTime.UtcNow;

        var value = new
        {
            Timestamp = now
        };

        var template = "{{ e.timestamp | format_date: 'yyyy-MM-dd-hh-mm-ss' }}";

        var actual = await RenderAync(template, value);

        Assert.Equal($"{now:yyyy-MM-dd-hh-mm-ss}", actual);
    }

    [Fact]
    public async Task Should_format_content_data()
    {
        var template = "{{ e.data.value.en }}";

        var value = new
        {
            Data =
                new ContentData()
                    .AddField("value",
                        new ContentFieldData()
                            .AddLocalized("en", "Hello"))
        };

        var actual = await RenderAync(template, value);

        Assert.Equal("Hello", actual);
    }

    [Fact]
    public async Task Should_format_html_to_text()
    {
        var template = "{{ e.text | html2text }}";

        var value = new
        {
            Text = "<script>Invalid</script><STYLE>Invalid</STYLE><p>Hello World</p>"
        };

        var actual = await RenderAync(template, value);

        Assert.Equal("Hello World", actual);
    }

    [Fact]
    public async Task Should_convert_markdown_to_text()
    {
        var template = "{{ e.text | markdown2text }}";

        var value = new
        {
            Text = "## Hello World"
        };

        var actual = await RenderAync(template, value);

        Assert.Equal("Hello World", actual);
    }

    [Fact]
    public async Task Should_format_word_count()
    {
        var template = "{{ e.text | word_count }}";

        var value = new
        {
            Text = "Hello World"
        };

        var actual = await RenderAync(template, value);

        Assert.Equal("2", actual);
    }

    [Fact]
    public async Task Should_format_character_count()
    {
        var template = "{{ e.text | character_count }}";

        var value = new
        {
            text = "Hello World"
        };

        var actual = await RenderAync(template, value);

        Assert.Equal("10", actual);
    }

    [Fact]
    public async Task Should_compute_md5_hash()
    {
        var template = "{{ e.text | md5 }}";

        var value = new
        {
            text = "HelloWorld"
        };

        var actual = await RenderAync(template, value);

        Assert.Equal("HelloWorld".ToMD5(), actual);
    }

    [Fact]
    public async Task Should_compute_sha256_hash()
    {
        var template = "{{ e.text | sha256 }}";

        var value = new
        {
            text = "HelloWorld"
        };

        var actual = await RenderAync(template, value);

        Assert.Equal("HelloWorld".ToSha256(), actual);
    }

    [Fact]
    public async Task Should_throw_exception_if_template_invalid()
    {
        var template = "{% for x of event %}";

        await Assert.ThrowsAsync<TemplateParseException>(() => sut.RenderAsync(template, new TemplateVars()));
    }

    private Task<string> RenderAync(string template, object value)
    {
        return sut.RenderAsync(template, new TemplateVars { ["e"] = value });
    }
}
