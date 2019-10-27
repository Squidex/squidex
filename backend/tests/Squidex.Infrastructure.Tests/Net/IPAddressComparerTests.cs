// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Net;
using Xunit;

namespace Squidex.Infrastructure.Net
{
    public class IPAddressComparerTests
    {
        [Fact]
        public void Should_sort_ip_addresses()
        {
            var source = new[]
            {
                IPAddress.IPv6Loopback,
                IPAddress.Parse("127.0.0.200"),
                IPAddress.Parse("127.0.0.255"),
                IPAddress.Parse("129.0.0.1"),
                IPAddress.Parse("127.0.0.1"),
                IPAddress.Parse("127.0.0.200")
            };

            var sorted = source.OrderBy(x => x, IPAddressComparer.Instance);

            var expected = new[]
            {
                IPAddress.Parse("127.0.0.1"),
                IPAddress.Parse("127.0.0.200"),
                IPAddress.Parse("127.0.0.200"),
                IPAddress.Parse("127.0.0.255"),
                IPAddress.Parse("129.0.0.1"),
                IPAddress.IPv6Loopback
            };

            Assert.Equal(expected, sorted);
        }
    }
}
