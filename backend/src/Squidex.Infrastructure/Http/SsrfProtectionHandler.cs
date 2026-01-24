// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Options;

namespace Squidex.Infrastructure.Http;

public class SsrfProtectionHandler(IOptions<SsrfOptions> options) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.RequestUri == null)
        {
            throw new HttpRequestException("Request URI is null");
        }

        if (!options.Value.AllowedSchemes.Contains(request.RequestUri.Scheme))
        {
            throw new HttpRequestException($"Scheme '{request.RequestUri.Scheme}' is not allowed");
        }

        var host = request.RequestUri.Host;
        try
        {
            var addresses = await Dns.GetHostAddressesAsync(host, cancellationToken);

            foreach (var address in addresses)
            {
                if (SsrfHelper.IsPrivateOrReservedIp(address, options.Value.BlockedIpAddresses))
                {
                    throw new HttpRequestException($"Request blocked: '{host}' resolves to private IP {address}");
                }
            }
        }
        catch (SocketException ex)
        {
            throw new HttpRequestException($"DNS resolution failed for '{host}'", ex);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
