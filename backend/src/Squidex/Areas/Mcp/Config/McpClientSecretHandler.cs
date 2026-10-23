// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure.Security;

namespace Squidex.Areas.Mcp.Config;

public sealed class McpClientSecretHandler(
    IAppProvider appProvider,
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "McpClientSecret";
    private const string BasicPrefix = "Basic ";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!TryGetCredentials(Request, out var clientName, out var secret))
        {
            return AuthenticateResult.NoResult();
        }

        var appName = Context.GetRouteValue("app")?.ToString();
        if (string.IsNullOrWhiteSpace(appName))
        {
            return AuthenticateResult.Fail("Invalid client.");
        }

        var app = await appProvider.GetAppAsync(appName, true, Context.RequestAborted);
        if (app == null)
        {
            return AuthenticateResult.Fail("Invalid client.");
        }

        // Also accept the full client ID, which is shown everywhere in the UI.
        var appPrefix = $"{app.Name}:";
        if (clientName.StartsWith(appPrefix, StringComparison.Ordinal))
        {
            clientName = clientName[appPrefix.Length..];
        }

        if (!app.Clients.TryGetValue(clientName, out var client))
        {
            return AuthenticateResult.Fail("Invalid client.");
        }

        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(client.Secret)))
        {
            return AuthenticateResult.Fail("Invalid client.");
        }

        // Same client claim as in a token, so that the app resolution assigns the role of the client.
        var identity = new ClaimsIdentity(
        [
            new Claim(OpenIdClaims.ClientId, $"{app.Name}:{clientName}"),
        ], SchemeName);

        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name));
    }

    public static bool IsClientSecret(HttpContext httpContext)
    {
        if (httpContext.GetEndpoint()?.Metadata.GetMetadata<McpEndpointMetadata>() == null)
        {
            return false;
        }

        return TryGetCredentials(httpContext.Request, out _, out _);
    }

    private static bool TryGetCredentials(HttpRequest request, [MaybeNullWhen(false)] out string clientName, [MaybeNullWhen(false)] out string secret)
    {
        clientName = null;
        secret = null;

        string? header = request.Headers[HeaderNames.Authorization];
        if (header?.StartsWith(BasicPrefix, StringComparison.OrdinalIgnoreCase) != true)
        {
            return false;
        }

        string decoded;
        try
        {
            decoded = Encoding.UTF8.GetString(Convert.FromBase64String(header[BasicPrefix.Length..].Trim()));
        }
        catch (FormatException)
        {
            return false;
        }

        // Split at the last colon, because the client ID can contain the app name and a colon.
        var separator = decoded.LastIndexOf(':');
        if (separator <= 0 || separator == decoded.Length - 1)
        {
            return false;
        }

        clientName = decoded[..separator];
        secret = decoded[(separator + 1)..];
        return true;
    }
}
