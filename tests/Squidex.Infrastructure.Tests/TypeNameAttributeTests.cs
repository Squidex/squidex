// ==========================================================================
//  TypeNameAttributeTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Xunit;

namespace Squidex.Infrastructure
{
    public class TypeNameAttributeTests
    {
        [Fact]
        public void Should_instantiate()
        {
            var attribute = new TypeNameAttribute("MyTypeName");

            Assert.Equal("MyTypeName", attribute.TypeName);
        }
    }
}
