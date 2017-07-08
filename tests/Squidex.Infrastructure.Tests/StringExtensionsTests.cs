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
        public void Should_convert_to_pascal_case(string input, string output)
        {
            Assert.Equal(output, input.ToPascalCase());
        }

        [Theory]
        [InlineData("My", "my")]
        [InlineData("MyProperty ", "myProperty")]
        [InlineData("My property", "myProperty")]
        [InlineData("My_property", "myProperty")]
        [InlineData("My-property", "myProperty")]
        public void Should_convert_to_camel_case(string input, string output)
        {
            Assert.Equal(output, input.ToCamelCase());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Should_provide_fallback_if_invalid(string value)
        {
            Assert.Equal("fallback", value.WithFallback("fallback"));
        }

        [Fact]
        public void Should_provide_value()
        {
            const string value = "value";

            Assert.Equal(value, value.WithFallback("fallback"));
        }
    }
}
