// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Queries;

public class PascalCasePathConverterTests
{
    [Fact]
    public void Should_convert_property()
    {
        var source = ClrFilter.Eq("property", 1);
        var actual = PascalCasePathConverter<ClrValue>.Transform(source);

        Assert.Equal("Property == 1", actual!.ToString());
    }

    [Fact]
    public void Should_convert_properties()
    {
        var source = ClrFilter.Eq("root.child", 1);
        var actual = PascalCasePathConverter<ClrValue>.Transform(source);

        Assert.Equal("Root.Child == 1", actual!.ToString());
    }
}
