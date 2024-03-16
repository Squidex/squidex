// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
        [InlineData("Me@web.com", true)]
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
            Assert.Equal("fallback", value.Or("fallback"));
        }

        [Fact]
        public void Should_provide_value()
        {
            const string value = "value";

            Assert.Equal(value, value.Or("fallback"));
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
        public void Should_join_non_empty_if_all_are_valid()
        {
            var result = StringExtensions.JoinNonEmpty("_", "1", "2", "3");

            Assert.Equal("1_2_3", result);
        }

        [Fact]
        public void Should_join_non_empty_if_first_invalid()
        {
            var result = StringExtensions.JoinNonEmpty("_", null, "2", "3");

            Assert.Equal("2_3", result);
        }

        [Fact]
        public void Should_join_non_empty_if_middle_invalid()
        {
            var result = StringExtensions.JoinNonEmpty("_", "1", null, "3");

            Assert.Equal("1_3", result);
        }

        [Fact]
        public void Should_join_non_empty_if_last_invalid()
        {
            var result = StringExtensions.JoinNonEmpty("_", "1", "2", null);

            Assert.Equal("1_2", result);
        }
    }
}
