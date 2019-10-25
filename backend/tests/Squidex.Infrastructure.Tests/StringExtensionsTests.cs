﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Xunit;

namespace Squidex.Infrastructure
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("me", false)]
        [InlineData("me@@web.com", false)]
        [InlineData("me@web.com", true)]
        public void Should_check_email(string email, bool isEmail)
        {
            Assert.Equal(isEmail, email.IsEmail());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Should_provide_fallback_if_invalid(string value)
        {
            Assert.Equal("fallback", value.WithFallback("fallback"));
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("m", "m")]
        [InlineData("m y", "m-y")]
        [InlineData("M Y", "m-y")]
        [InlineData("M_Y", "m-y")]
        [InlineData("M_Y ", "m-y")]
        public void Should_convert_to_kebap_case(string input, string output)
        {
            Assert.Equal(output, input.ToKebabCase());
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("m", "M")]
        [InlineData("m-y", "MY")]
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
        [InlineData("", "")]
        [InlineData("M", "m")]
        [InlineData("My", "my")]
        [InlineData("M-y", "mY")]
        [InlineData("MyProperty ", "myProperty")]
        [InlineData("My property", "myProperty")]
        [InlineData("My_property", "myProperty")]
        [InlineData("My-property", "myProperty")]
        public void Should_convert_to_camel_case(string input, string output)
        {
            Assert.Equal(output, input.ToCamelCase());
        }

        [Theory]
        [InlineData("Hello World", '-', "hello-world")]
        [InlineData("Hello/World", '-', "hello-world")]
        [InlineData("Hello World", '_', "hello_world")]
        [InlineData("Hello/World", '_', "hello_world")]
        [InlineData("Hello World ", '_', "hello_world")]
        [InlineData("Hello World-", '_', "hello_world")]
        [InlineData("Hello/World_", '_', "hello_world")]
        public void Should_replace_special_characters_with_sepator_when_slugifying(string input, char separator, string output)
        {
            Assert.Equal(output, input.Slugify(separator: separator));
        }

        [Theory]
        [InlineData("ö", "oe")]
        [InlineData("ü", "ue")]
        [InlineData("ä", "ae")]
        public void Should_replace_multi_char_diacritics_when_slugifying(string input, string output)
        {
            Assert.Equal(output, input.Slugify());
        }

        [Theory]
        [InlineData("ö", "o")]
        [InlineData("ü", "u")]
        [InlineData("ä", "a")]
        public void Should_not_replace_multi_char_diacritics_when_slugifying(string input, string output)
        {
            Assert.Equal(output, input.Slugify(singleCharDiactric: true));
        }

        [Theory]
        [InlineData("Físh", "fish")]
        [InlineData("źish", "zish")]
        [InlineData("żish", "zish")]
        [InlineData("fórm", "form")]
        [InlineData("fòrm", "form")]
        [InlineData("fårt", "fart")]
        public void Should_replace_single_char_diacritics_when_slugifying(string input, string output)
        {
            Assert.Equal(output, input.Slugify());
        }

        [Theory]
        [InlineData("Hello my&World ", '_', "hello_my&world")]
        [InlineData("Hello my&World-", '_', "hello_my&world")]
        [InlineData("Hello my/World_", '_', "hello_my/world")]
        public void Should_keep_characters_when_slugifying(string input, char separator, string output)
        {
            Assert.Equal(output, input.Slugify(new HashSet<char> { '&', '/' }, false, separator));
        }

        [Fact]
        public void Should_provide_value()
        {
            const string value = "value";

            Assert.Equal(value, value.WithFallback("fallback"));
        }

        [Theory]
        [InlineData("http://squidex.io/base/", "path/to/res", false, "http://squidex.io/base/path/to/res")]
        [InlineData("http://squidex.io/base/", "path/to/res", true, "http://squidex.io/base/path/to/res/")]
        [InlineData("http://squidex.io/base/", "/path/to/res", true, "http://squidex.io/base/path/to/res/")]
        public void Should_provide_full_url_without_query_or_fragment(string baseUrl, string path, bool trailingSlash, string output)
        {
            var result = baseUrl.BuildFullUrl(path, trailingSlash);

            Assert.Equal(output, result);
        }

        [Theory]
        [InlineData("http://squidex.io/base/", "path/to/res?query=1", false, "http://squidex.io/base/path/to/res?query=1")]
        [InlineData("http://squidex.io/base/", "path/to/res#query=1", true, "http://squidex.io/base/path/to/res#query=1")]
        [InlineData("http://squidex.io/base/", "path/to/res;query=1", true, "http://squidex.io/base/path/to/res;query=1")]
        public void Should_provide_full_url_wit_query_or_fragment(string baseUrl, string path, bool trailingSlash, string output)
        {
            var result = baseUrl.BuildFullUrl(path, trailingSlash);

            Assert.Equal(output, result);
        }

        [Fact]
        public void Should_join_non_empty_when_all_are_valid()
        {
            var result = StringExtensions.JoinNonEmpty("_", "1", "2", "3");

            Assert.Equal("1_2_3", result);
        }

        [Fact]
        public void Should_join_non_empty_when_first_invalid()
        {
            var result = StringExtensions.JoinNonEmpty("_", null, "2", "3");

            Assert.Equal("2_3", result);
        }

        [Fact]
        public void Should_join_non_empty_when_middle_invalid()
        {
            var result = StringExtensions.JoinNonEmpty("_", "1", null, "3");

            Assert.Equal("1_3", result);
        }

        [Fact]
        public void Should_join_non_empty_when_last_invalid()
        {
            var result = StringExtensions.JoinNonEmpty("_", "1", "2", null);

            Assert.Equal("1_2", result);
        }
    }
}
