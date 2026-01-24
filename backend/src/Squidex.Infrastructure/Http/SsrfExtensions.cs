// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Squidex.Infrastructure.Http;

public static class SsrfExtensions
{
    public static IHttpClientBuilder EnableSsrfProtection(this IHttpClientBuilder builder )
    {
        builder.AddHttpMessageHandler<SsrfProtectionHandler>();
        builder.ConfigurePrimaryHttpMessageHandler(services =>
        {
            var options = services.GetService<IOptions<SsrfOptions>>()?.Value ?? new ();

            return new SocketsHttpHandler
            {
                ConnectCallback = options.EnableDnsRebindingProtection
                    ? CreateSecureConnectCallback(options)
                    : null,
                AllowAutoRedirect = options.AllowAutoRedirect,
            };
        });

        return builder;
    }

    private static Func<SocketsHttpConnectionContext, CancellationToken, ValueTask<Stream>> CreateSecureConnectCallback(SsrfOptions options)
    {
        return async (context, cancellationToken) =>
        {
            var host = context.DnsEndPoint.Host;

            // Re-validate DNS to prevent DNS rebinding attacks
            var addresses = await Dns.GetHostAddressesAsync(host, cancellationToken);

            foreach (var address in addresses)
            {
                if (SsrfHelper.IsPrivateOrReservedIp(address, options.BlockedIpAddresses))
                {
                    throw new HttpRequestException($"Connection to private IP blocked: {address}");
                }
            }

            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(context.DnsEndPoint, cancellationToken);

            return new NetworkStream(socket, ownsSocket: true);
        };
    }
}
