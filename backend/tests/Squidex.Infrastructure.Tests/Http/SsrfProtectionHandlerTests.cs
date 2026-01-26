// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using Microsoft.Extensions.Options;

namespace Squidex.Infrastructure.Http;

public class SsrfProtectionHandlerTests
{
    private readonly SsrfCustomHandler sut;
    private readonly SsrfOptions options = new ();

    private sealed class SsrfCustomHandler(IOptions<SsrfOptions> options) : SsrfProtectionHandler(options)
    {
        public new async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return await base.SendAsync(request, cancellationToken);
        }
    }

    private sealed class TestHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    public SsrfProtectionHandlerTests()
    {
        sut = new SsrfCustomHandler(Options.Create(options))
        {
            InnerHandler = new TestHttpMessageHandler(),
        };
    }

    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com")]
    public async Task Should_allow_http_and_https_schemes(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        await sut.SendAsync(request, CancellationToken.None);
    }

    [Theory]
    [InlineData("ftp://example.com")]
    [InlineData("file:///etc/passwd")]
    public async Task Should_block_non_http_schemes(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            sut.SendAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task Should_throw_exception_if_request_uri_is_null()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, (Uri?)null);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            sut.SendAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task Should_block_request_to_localhost()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            sut.SendAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task Should_block_request_to_loopback_ip()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "http://127.0.0.1");

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            sut.SendAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task Should_not_block_request_to_localhost_if_whitelisted()
    {
        options.WhitelistedHosts.Add("localhost");

        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");

        await sut.SendAsync(request, CancellationToken.None);
    }

    [Fact]
    public async Task Should_not_block_request_to_localhost_if_all_hosts_are_whitelisted()
    {
        options.WhitelistedHosts.Add("*");

        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost");

        await sut.SendAsync(request, CancellationToken.None);
    }

    [Fact]
    public async Task Should_allow_custom_scheme_when_configured()
    {
        options.AllowedSchemes.Add("custom");

        var request = new HttpRequestMessage(HttpMethod.Get, "custom://example.com");

        await sut.SendAsync(request, CancellationToken.None);
    }

    [Fact]
    public async Task Should_throw_exception_on_dns_resolution_failure()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "http://invalid.domain.that.does.not.exist.local");

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            sut.SendAsync(request, CancellationToken.None));
    }
}
