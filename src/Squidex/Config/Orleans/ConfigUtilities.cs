// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Orleans.Runtime;

namespace Squidex.Config.Orleans
{
    public static class ConfigUtilities
    {
        public static IPAddress SiloAddress { get; } = GetBestIPAddressAsync().Result;

        private static Task<IPAddress> GetBestIPAddressAsync()
        {
            return ResolveIPAddressAsync(Dns.GetHostName(), null, AddressFamily.InterNetwork);
        }

        internal static async Task<IPAddress> ResolveIPAddressAsync(string addressOrHost, byte[] subnet, AddressFamily family)
        {
            var loopback = family == AddressFamily.InterNetwork ? IPAddress.Loopback : IPAddress.IPv6Loopback;

            IList<IPAddress> nodeIps;

            if (string.IsNullOrEmpty(addressOrHost))
            {
                nodeIps =
                    NetworkInterface.GetAllNetworkInterfaces()
                        .SelectMany(i => i.GetIPProperties().UnicastAddresses)
                        .Select(a => a.Address)
                        .Where(a => a.AddressFamily == family && !IPAddress.IsLoopback(a))
                        .ToList();
            }
            else
            {
                if (addressOrHost.Equals("loopback", StringComparison.OrdinalIgnoreCase))
                {
                    return loopback;
                }

                if (IPAddress.TryParse(addressOrHost, out IPAddress address))
                {
                    return address;
                }

                nodeIps = await Dns.GetHostAddressesAsync(addressOrHost);
            }

            var candidates = new List<IPAddress>();

            foreach (var nodeIp in nodeIps.Where(x => x.AddressFamily == family))
            {
                if (subnet == null)
                {
                    candidates.Add(nodeIp);
                }
                else
                {
                    var ip = nodeIp;

                    if (subnet.Select((b, i) => ip.GetAddressBytes()[i] == b).All(x => x))
                    {
                        candidates.Add(nodeIp);
                    }
                }
            }

            if (candidates.Count > 0)
            {
                return PickIPAddress(candidates);
            }

            var subnetStr = Utils.EnumerableToString(subnet, null, ".", false);

            throw new ArgumentException($"Hostname '{addressOrHost}' with subnet {subnetStr} and family {family} is not a valid IP address or DNS name");
        }

        internal static IPAddress PickIPAddress(IReadOnlyList<IPAddress> candidates)
        {
            IPAddress result = null;

            foreach (IPAddress address in candidates)
            {
                if (result == null)
                {
                    result = address;
                }
                else
                {
                    if (CompareIPAddresses(address, result))
                    {
                        result = address;
                    }
                }
            }

            return result;
        }

        private static bool CompareIPAddresses(IPAddress lhs, IPAddress rhs)
        {
            var lbytes = lhs.GetAddressBytes();
            var rbytes = rhs.GetAddressBytes();

            if (lbytes.Length != rbytes.Length)
            {
                return lbytes.Length < rbytes.Length;
            }

            for (int i = 0; i < lbytes.Length; i++)
            {
                if (lbytes[i] != rbytes[i])
                {
                    return lbytes[i] < rbytes[i];
                }
            }

            return false;
        }
    }
}
