// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

namespace Squidex.Web
{
    public sealed class UrlsOptionsTests
    {
        private readonly UrlsOptions sut = new UrlsOptions
        {
            BaseUrl = "http://localhost"
        };

        [Theory]
        [InlineData("/url")]
        [InlineData("/url/")]
        [InlineData("url")]
        public void Should_build_url_with_leading_slash(string path)
        {
            var url = sut.BuildUrl(path);

            Assert.Equal("http://localhost/url/", url);
        }

        [Theory]
        [InlineData("/url")]
        [InlineData("/url/")]
        [InlineData("url")]
        public void Should_build_url_without_leading_slash(string path)
        {
            var url = sut.BuildUrl(path, false);

            Assert.Equal("http://localhost/url", url);
        }

        [Fact]
        public void Should_allow_same_host()
        {
            Assert.True(sut.IsAllowedHost("http://localhost"));
        }

        [Fact]
        public void Should_allow_https_port()
        {
            Assert.True(sut.IsAllowedHost("https://localhost"));
        }

        [Fact]
        public void Should_not_allow_other_host()
        {
            Assert.False(sut.IsAllowedHost("https://other:5000"));
        }

        [Fact]
        public void Should_not_allow_same_host_with_other_port()
        {
            Assert.False(sut.IsAllowedHost("https://localhost:3000"));
        }
    }
}
