// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text;

namespace Squidex.Web.Pipeline;

public sealed class ApiKeyHandler(
    IAppProvider appProvider,
    ITextIndex textIndex,
    IOptionsMonitor<ApiKeyOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<ApiKeyOptions>(options, logger, encoder)
{
    private const string ApiKeyPrefix = "ApiKey ";
    private const string ApiKeyHeader = "ApiKey";
    private const string ApiKeyHeaderX = "X-ApiKey";
    private const string ApiKeyQuery = "api_key";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            if (!IsApiKey(Request, out var apiKey))
            {
                return AuthenticateResult.NoResult();
            }

            var keyParts = apiKey.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (keyParts.Length != 2)
            {
                return AuthenticateResult.Fail("Invalid API Key");
            }

            var app = await appProvider.GetAppAsync(keyParts[0], true, Context.RequestAborted);
            if (app == null)
            {
                return AuthenticateResult.Fail("Invalid API Key");
            }

            var user =
                await textIndex.FindUserInfo(
                    app,
                    new ApiKeyQuery(keyParts[1]),
                    SearchScope.Published,
                    Context.RequestAborted);
            if (user == null)
            {
                return AuthenticateResult.Fail("Invalid API Key");
            }

            var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.ContentId.ToString()),
                new Claim(Constants.ClaimTypeApp, app.Name),
                new Claim(Constants.ClaimTypeRole, user.Role),
            ], ApiKeyDefaults.AuthenticationScheme);

            return Success(identity);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while handling api key.");

            throw;
        }
    }

    private AuthenticateResult Success(ClaimsIdentity identity)
    {
        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    public static bool IsApiKey(HttpRequest request, [MaybeNullWhen(false)] out string apiKey)
    {
        apiKey = null!;
        string? authorizationHeader = request.Headers[HeaderNames.Authorization];

        if (authorizationHeader?.StartsWith(ApiKeyPrefix, StringComparison.OrdinalIgnoreCase) == true)
        {
            var key = authorizationHeader[ApiKeyPrefix.Length..].Trim();
            if (!string.IsNullOrWhiteSpace(key))
            {
                apiKey = key;
                return true;
            }
        }

        string? apiKeyHeader = request.Headers[ApiKeyHeader];
        if (!string.IsNullOrWhiteSpace(apiKeyHeader))
        {
            apiKey = apiKeyHeader;
            return true;
        }

        string? apiKeyHeaderX = request.Headers[ApiKeyHeaderX];
        if (!string.IsNullOrWhiteSpace(apiKeyHeaderX))
        {
            apiKey = apiKeyHeaderX;
            return true;
        }

        string? apiKeyQuery = request.Query[ApiKeyQuery];
        if (!string.IsNullOrWhiteSpace(apiKeyQuery))
        {
            apiKey = apiKeyQuery;
            return true;
        }

        return false;
    }
}
