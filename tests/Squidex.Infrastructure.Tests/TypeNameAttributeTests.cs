// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
