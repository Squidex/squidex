// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NSwag;
using NSwag.Generation.Processors.Security;
using Squidex.Hosting;
using Squidex.Web;

namespace Squidex.Areas.Api.Config.OpenApi;

public sealed class SecurityProcessor : SecurityDefinitionAppender
{
    public SecurityProcessor(IUrlGenerator urlGenerator)
        : base(Constants.SecurityDefinition, Enumerable.Empty<string>(), CreateOAuthSchema(urlGenerator))
    {
    }

    private static OpenApiSecurityScheme CreateOAuthSchema(IUrlGenerator urlGenerator)
    {
        var tokenUrl = urlGenerator.BuildUrl($"/{Constants.PrefixIdentityServer}/connect/token", false);

        // Just described the token URL again.
        var securityText = Properties.Resources.OpenApiSecurity.Replace("<TOKEN_URL>", tokenUrl, StringComparison.Ordinal);

        var security = new OpenApiSecurityScheme
        {
            Type = OpenApiSecuritySchemeType.OpenIdConnect,

            // The link to the endpoint where the user can request the token.
            TokenUrl = tokenUrl,

            // Basically the same like client credentials flow.
            Flow = OpenApiOAuth2Flow.Application,

            Scopes = new Dictionary<string, string>
            {
                [Constants.ScopeApi] = "Read and write access to the API"
            },
            Description = securityText,
        };

        return security;
    }
}
