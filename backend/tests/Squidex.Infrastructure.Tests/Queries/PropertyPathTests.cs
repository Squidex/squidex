// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Queries;

public class PropertyPathTests
{
    [Fact]
    public void Should_create()
    {
        var path = new PropertyPath(new[] { "path", "to", "property" });

        Assert.Equal(new[] { "path", "to", "property" }, path.ToArray());
    }

    [Fact]
    public void Should_convert_to_string()
    {
        var path = new PropertyPath(new[] { "path", "to", "property" });

        Assert.Equal("path.to.property", path.ToString());
    }

    [Fact]
    public void Should_throw_exception_for_empty_path()
    {
        Assert.Throws<ArgumentException>(() => new PropertyPath(Array.Empty<string>()));
    }

    [Fact]
    public void Should_throw_exception_for_empty_path_from_string()
    {
        Assert.Throws<ArgumentException>(() => { PropertyPath p = string.Empty; });
    }

    [Fact]
    public void Should_throw_exception_for_empty_path_from_null_string()
    {
        Assert.Throws<ArgumentException>(() => { PropertyPath p = (string)null!; });
    }

    [Fact]
    public void Should_throw_exception_for_empty_path_from_list()
    {
        Assert.Throws<ArgumentException>(() => { PropertyPath p = new List<string>(); });
    }

    [Fact]
    public void Should_create_from_dot_string()
    {
        PropertyPath path = "path.to.property";

        Assert.Equal(new[] { "path", "to", "property" }, path.ToArray());
    }

    [Fact]
    public void Should_create_from_broken_dot_string()
    {
        PropertyPath path = ".path...to...property.";

        Assert.Equal(new[] { "path", "to", "property" }, path.ToArray());
    }

    [Fact]
    public void Should_create_from_slash_string()
    {
        PropertyPath path = "path/to/property";

        Assert.Equal(new[] { "path", "to", "property" }, path.ToArray());
    }

    [Fact]
    public void Should_create_from_broken_slash_string()
    {
        PropertyPath path = "/path///to///property/";

        Assert.Equal(new[] { "path", "to", "property" }, path.ToArray());
    }

    [Fact]
    public void Should_create_from_dot_string_and_escape()
    {
        PropertyPath path = "path.to.complex\\.property";

        Assert.Equal(new[] { "path", "to", "complex.property" }, path.ToArray());
    }

    [Fact]
    public void Should_create_from_slash_string_and_escape()
    {
        PropertyPath path = "path.to.complex\\/property";

        Assert.Equal(new[] { "path", "to", "complex/property" }, path.ToArray());
    }
}
