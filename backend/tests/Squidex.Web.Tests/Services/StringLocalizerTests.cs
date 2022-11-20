// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Translations;
using Squidex.Shared;

namespace Squidex.Web.Services;

public class StringLocalizerTests
{
    private readonly StringLocalizer sut;

    public StringLocalizerTests()
    {
        var translations = new ResourcesLocalizer(Texts.ResourceManager);

        sut = new StringLocalizer(translations);
    }

    [Fact]
    public void Should_provide_translation()
    {
        var key = "annotations_Required";

        var name = sut[key];

        Assert.Equal("The field '{0}' is required.", name);
    }

    [Fact]
    public void Should_format_translation()
    {
        var key = "annotations_Required";

        var name = sut[key, "MyField"];

        Assert.Equal("The field 'MyField' is required.", name);
    }

    [Fact]
    public void Should_translate_property_name()
    {
        var key = "annotations_Required";

        var name = sut[key, "ClientId"];

        Assert.Equal("The field 'Client ID' is required.", name);
    }
}
