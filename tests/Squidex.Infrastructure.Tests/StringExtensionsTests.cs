// ==========================================================================
//  StringExtensionsTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Xunit;

namespace Squidex.Infrastructure
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("my", "My")]
        [InlineData("myProperty ", "MyProperty")]
        [InlineData("my property", "MyProperty")]
        [InlineData("my_property", "MyProperty")]
        [InlineData("my-property", "MyProperty")]
        [InlineData("my  property", "MyProperty")]
        [InlineData("my__property", "MyProperty")]
        [InlineData("my--property", "MyProperty")]
        public void Should_convert_to_pascal_case(string input, string output)
        {
            Assert.Equal(output, input.ToPascalCase());
        }
    }
}
