// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;

namespace Squidex.Infrastructure.Http;

public class SsrfHelperTests
{
    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("::1")]
    public void Should_block_loopback_addresses(string ip)
    {
        var address = IPAddress.Parse(ip);

        var result = SsrfHelper.IsPrivateOrReservedIp(address, null);

        Assert.True(result);
    }

    [Theory]
    [InlineData("10.0.0.1")]
    [InlineData("10.255.255.255")]
    [InlineData("172.16.0.1")]
    [InlineData("172.31.255.255")]
    [InlineData("192.168.0.1")]
    [InlineData("192.168.255.255")]
    public void Should_block_private_ipv4_ranges(string ip)
    {
        var address = IPAddress.Parse(ip);

        var result = SsrfHelper.IsPrivateOrReservedIp(address, null);

        Assert.True(result);
    }

    [Theory]
    [InlineData("169.254.0.1")]
    [InlineData("169.254.169.254")]
    public void Should_block_link_local_addresses(string ip)
    {
        var address = IPAddress.Parse(ip);

        var result = SsrfHelper.IsPrivateOrReservedIp(address, null);

        Assert.True(result);
    }

    [Theory]
    [InlineData("0.0.0.0")]
    [InlineData("0.255.255.255")]
    public void Should_block_current_network_addresses(string ip)
    {
        var address = IPAddress.Parse(ip);

        var result = SsrfHelper.IsPrivateOrReservedIp(address, null);

        Assert.True(result);
    }

    [Theory]
    [InlineData("224.0.0.1")]
    [InlineData("239.255.255.255")]
    public void Should_block_multicast_addresses(string ip)
    {
        var address = IPAddress.Parse(ip);

        var result = SsrfHelper.IsPrivateOrReservedIp(address, null);

        Assert.True(result);
    }

    [Theory]
    [InlineData("240.0.0.1")]
    [InlineData("255.255.255.255")]
    public void Should_block_reserved_addresses(string ip)
    {
        var address = IPAddress.Parse(ip);

        var result = SsrfHelper.IsPrivateOrReservedIp(address, null);

        Assert.True(result);
    }

    [Theory]
    [InlineData("fe80::1")]
    [InlineData("fec0::1")]
    public void Should_block_ipv6_link_local_and_site_local(string ip)
    {
        var address = IPAddress.Parse(ip);

        var result = SsrfHelper.IsPrivateOrReservedIp(address, null);

        Assert.True(result);
    }

    [Theory]
    [InlineData("fc00::1")]
    [InlineData("fd00::1")]
    public void Should_block_ipv6_unique_local_addresses(string ip)
    {
        var address = IPAddress.Parse(ip);

        var result = SsrfHelper.IsPrivateOrReservedIp(address, null);

        Assert.True(result);
    }

    [Theory]
    [InlineData("ff00::1")]
    [InlineData("ff02::1")]
    public void Should_block_ipv6_multicast_addresses(string ip)
    {
        var address = IPAddress.Parse(ip);

        var result = SsrfHelper.IsPrivateOrReservedIp(address, null);

        Assert.True(result);
    }

    [Theory]
    [InlineData("8.8.8.8")]
    [InlineData("1.1.1.1")]
    [InlineData("203.0.113.1")]
    public void Should_allow_public_ipv4_addresses(string ip)
    {
        var address = IPAddress.Parse(ip);

        var result = SsrfHelper.IsPrivateOrReservedIp(address, null);

        Assert.False(result);
    }

    [Theory]
    [InlineData("2001:4860:4860::8888")]
    [InlineData("2606:4700:4700::1111")]
    public void Should_allow_public_ipv6_addresses(string ip)
    {
        var address = IPAddress.Parse(ip);

        var result = SsrfHelper.IsPrivateOrReservedIp(address, null);

        Assert.False(result);
    }

    [Fact]
    public void Should_block_custom_blacklisted_ip()
    {
        var address = IPAddress.Parse("1.2.3.4");
        var blacklist = new HashSet<IPAddress> { IPAddress.Parse("1.2.3.4") };

        var result = SsrfHelper.IsPrivateOrReservedIp(address, blacklist);

        Assert.True(result);
    }

    [Fact]
    public void Should_allow_ip_not_in_blacklist()
    {
        var address = IPAddress.Parse("8.8.8.8");
        var blacklist = new HashSet<IPAddress> { IPAddress.Parse("1.2.3.4") };

        var result = SsrfHelper.IsPrivateOrReservedIp(address, blacklist);

        Assert.False(result);
    }

    [Fact]
    public void Should_handle_null_blacklist()
    {
        var address = IPAddress.Parse("8.8.8.8");

        var result = SsrfHelper.IsPrivateOrReservedIp(address, null);

        Assert.False(result);
    }

    [Fact]
    public void Should_handle_empty_blacklist()
    {
        var address = IPAddress.Parse("8.8.8.8");
        var blacklist = new HashSet<IPAddress>();

        var result = SsrfHelper.IsPrivateOrReservedIp(address, blacklist);

        Assert.False(result);
    }
}
