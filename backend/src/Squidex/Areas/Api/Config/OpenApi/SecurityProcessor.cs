// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using NSwag;
using NSwag.Generation.Processors.Security;
using Squidex.Hosting;
using Squidex.Web;

namespace Squidex.Areas.Api.Config.OpenApi
{
    public sealed class SecurityProcessor : SecurityDefinitionAppender
    {
        public SecurityProcessor(IUrlGenerator urlGenerator)
            : base(Constants.SecurityDefinition, Enumerable.Empty<string>(), CreateOAuthSchema(urlGenerator))
        {
        }

        private static OpenApiSecurityScheme CreateOAuthSchema(IUrlGenerator urlGenerator)
        {
            var security = new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.OAuth2
            };

            var tokenUrl = urlGenerator.BuildUrl($"{Constants.PrefixIdentityServer}/connect/token", false);

            security.TokenUrl = tokenUrl;

            SetupDescription(security, tokenUrl);
            SetupFlow(security);
            SetupScopes(security);

            return security;
        }

        private static void SetupFlow(OpenApiSecurityScheme security)
        {
            security.Flow = OpenApiOAuth2Flow.Application;
        }

        private static void SetupScopes(OpenApiSecurityScheme security)
        {
            security.Scopes = new Dictionary<string, string>
            {
                [Constants.ScopeApi] = "Read and write access to the API"
            };
        }

        private static void SetupDescription(OpenApiSecurityScheme securityScheme, string tokenUrl)
        {
            var securityText = Properties.Resources.OpenApiSecurity.Replace("<TOKEN_URL>", tokenUrl, StringComparison.Ordinal);

            securityScheme.Description = securityText;
        }
    }
}
