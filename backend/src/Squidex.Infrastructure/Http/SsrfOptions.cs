// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;

namespace Squidex.Infrastructure.Http;

public class SsrfOptions
{
    public HashSet<string> AllowedSchemes { get; set; } =
        new HashSet<string>(
            ["http", "https"],
            StringComparer.OrdinalIgnoreCase);

    public HashSet<IPAddress> BlockedIpAddresses { get; set; } =
        new HashSet<IPAddress>(
            [IPAddress.Parse("169.254.169.254")]);

    public bool AllowAutoRedirect { get; set; }

    public bool EnableDnsRebindingProtection { get; set; } = true;
}
