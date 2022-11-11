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
        : base(Constants.SecurityDefinition, new[] { Constants.ScopeApi }, CreateOAuthSchema(urlGenerator))
    {
    }

    private static OpenApiSecurityScheme CreateOAuthSchema(IUrlGenerator urlGenerator)
    {
        string BuildUrl(string endpoint)
        {
            return urlGenerator.BuildUrl($"/{Constants.PrefixIdentityServer}/{endpoint}", false);
        }

        var security = new OpenApiSecurityScheme
        {
            Type = OpenApiSecuritySchemeType.OpenIdConnect,

            // The discovery endpoint
            OpenIdConnectUrl = BuildUrl(".well-known/openid-configuration"),

            // Just described the token URL again.
            Description = Properties.Resources.OpenApiSecurity.Replace("<TOKEN_URL>", BuildUrl($"connect/token"), StringComparison.Ordinal)
        };

        return security;
    }
}
