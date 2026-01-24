// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using System.Net.Sockets;

#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row

namespace Squidex.Infrastructure.Http;

public static class SsrfHelper
{
    public static bool IsPrivateOrReservedIp(IPAddress ip, HashSet<IPAddress>? blackList)
    {
        if (IPAddress.IsLoopback(ip))
        {
            return true;
        }

        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = ip.GetAddressBytes();

            var isBlocked =
                (bytes[0] == 10) ||                                         // 10.0.0.0/8
                (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||    // 172.16.0.0/12
                (bytes[0] == 192 && bytes[1] == 168) ||                     // 192.168.0.0/16
                (bytes[0] == 169 && bytes[1] == 254) ||                     // link-local
                (bytes[0] == 0) ||                                          // 0.0.0.0/8
                (bytes[0] >= 224 && bytes[0] <= 239) ||                     // 224.0.0.0/4 multicast
                (bytes[0] >= 240);                                          // 240.0.0.0/4 reserved

            if (isBlocked)
            {
                return true;
            }
        }

        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
        {
            var bytes = ip.GetAddressBytes();

            var isBlocked =
                ip.IsIPv6LinkLocal ||                                       // fe80::/10
                ip.IsIPv6SiteLocal ||                                       // fec0::/10 (deprecated)
                ip.IsIPv6Multicast ||                                       // ff00::/8
                ((bytes[0] & 0xfe) == 0xfc);                                // fc00::/7 - Unique local

            if (isBlocked)
            {
                return true;
            }
        }

        if (blackList is {  Count: > 0 })
        {
            return blackList.Contains(ip);
        }

        return false;
    }
}
