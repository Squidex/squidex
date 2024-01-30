// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Model.Assets;

public class AssetMetadataTests
{
    [Fact]
    public void Should_return_focus_infos_if_found()
    {
        var sut =
            new AssetMetadata
            {
                [KnownMetadataKeys.FocusX] = .5f,
                [KnownMetadataKeys.FocusY] = .2f,
            };

        Assert.Equal(.5f, sut.GetSingle(KnownMetadataKeys.FocusX));
        Assert.Equal(.2f, sut.GetSingle(KnownMetadataKeys.FocusY));
    }

    [Fact]
    public void Should_return_null_if_focus_infos_not_found()
    {
        var sut = new AssetMetadata();

        Assert.Null(sut.GetSingle(KnownMetadataKeys.FocusX));
        Assert.Null(sut.GetSingle(KnownMetadataKeys.FocusY));
    }

    [Fact]
    public void Should_return_pixel_infos_if_found()
    {
        var sut =
            new AssetMetadata
            {
                [KnownMetadataKeys.PixelWidth] = 800,
                [KnownMetadataKeys.PixelHeight] = 600,
            };

        Assert.Equal(800, sut.GetInt32(KnownMetadataKeys.PixelWidth));
        Assert.Equal(600, sut.GetInt32(KnownMetadataKeys.PixelHeight));
    }

    [Fact]
    public void Should_return_null_if_pixel_infos_not_found()
    {
        var sut = new AssetMetadata();

        Assert.Null(sut.GetInt32(KnownMetadataKeys.PixelWidth));
        Assert.Null(sut.GetInt32(KnownMetadataKeys.PixelHeight));
    }

    [Fact]
    public void Should_return_self_if_path_is_empty()
    {
        var sut = new AssetMetadata();

        var found = sut.TryGetByPath(string.Empty, out var actual);

        Assert.False(found);
        Assert.Same(sut, actual);
    }

    [Fact]
    public void Should_return_plain_value_if_found()
    {
        var sut = new AssetMetadata
        {
            ["someValue"] = JsonValue.Create(800)
        };

        var found = sut.TryGetByPath("someValue", out var actual);

        Assert.True(found);
        Assert.Equal(JsonValue.Create(800), actual);
    }

    [Fact]
    public void Should_return_null_if_not_found()
    {
        var sut = new AssetMetadata();

        var found = sut.TryGetByPath("notFound", out var actual);

        Assert.False(found);
        Assert.Null(actual);
    }

    [Fact]
    public void Should_return_nested_value_if_found()
    {
        var sut = new AssetMetadata
        {
            ["meta"] =
                JsonValue.Object()
                    .Add("nested1",
                        JsonValue.Object()
                            .Add("nested2", 12))
        };

        var found = sut.TryGetByPath("meta.nested1.nested2", out var actual);

        Assert.True(found);
        Assert.Equal(JsonValue.Create(12), actual);
    }

    [Fact]
    public void Should_get_string_if_found()
    {
        var value = "Hello";

        var sut = new AssetMetadata
        {
            ["string"] = JsonValue.Create(value)
        };

        var found = sut.TryGetString("string", out var actual);

        Assert.True(found);
        Assert.Equal(value, actual);
    }

    [Fact]
    public void Should_get_null_if_property_is_not_string()
    {
        var sut = new AssetMetadata
        {
            ["string"] = JsonValue.Create(12)
        };

        var found = sut.TryGetString("string", out var actual);

        Assert.False(found);
        Assert.Null(actual);
    }

    [Fact]
    public void Should_get_null_if_string_property_not_found()
    {
        var sut = new AssetMetadata();

        var found = sut.TryGetString("other", out var actual);

        Assert.False(found);
        Assert.Null(actual);
    }

    [Fact]
    public void Should_get_number_if_found()
    {
        var value = 12.5;

        var sut = new AssetMetadata
        {
            ["number"] = JsonValue.Create(value)
        };

        var found = sut.TryGetNumber("number", out var actual);

        Assert.True(found);
        Assert.Equal(value, actual);
    }

    [Fact]
    public void Should_get_null_if_property_is_not_number()
    {
        var sut = new AssetMetadata
        {
            ["number"] = JsonValue.Create(true)
        };

        var found = sut.TryGetNumber("number", out var actual);

        Assert.False(found);
        Assert.Equal(0, actual);
    }

    [Fact]
    public void Should_get_null_if_number_property_not_found()
    {
        var sut = new AssetMetadata();

        var found = sut.TryGetNumber("other", out var actual);

        Assert.False(found);
        Assert.Equal(0, actual);
    }
}
