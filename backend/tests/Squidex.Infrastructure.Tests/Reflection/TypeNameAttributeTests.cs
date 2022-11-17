// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Reflection;

public class TypeNameAttributeTests
{
    [Fact]
    public void Should_instantiate()
    {
        var attribute = new TypeNameAttribute("MyTypeName");

        Assert.Equal("MyTypeName", attribute.TypeName);
    }
}
