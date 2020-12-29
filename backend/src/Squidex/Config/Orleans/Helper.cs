// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Squidex.Hosting.Configuration;
using Squidex.Infrastructure.Net;

namespace Squidex.Config.Orleans
{
    public static class Helper
    {
        internal static async Task<IPAddress> ResolveIPAddressAsync(string addressOrHost, AddressFamily family)
        {
            var loopback = family == AddressFamily.InterNetwork ? IPAddress.Loopback : IPAddress.IPv6Loopback;

            if (addressOrHost.Equals("loopback", StringComparison.OrdinalIgnoreCase))
            {
                return loopback;
            }

            if (IPAddress.TryParse(addressOrHost, out var address))
            {
                return address;
            }

            var candidates = await Dns.GetHostAddressesAsync(addressOrHost);

            var chosen = candidates.OrderBy(x => x, IPAddressComparer.Instance).FirstOrDefault();

            if (chosen == null)
            {
                var error = new ConfigurationError($"Hostname {addressOrHost} with family {family} is not a valid IP address or DNS name");

                throw new ConfigurationException(error);
            }

            return chosen;
        }
    }
}
