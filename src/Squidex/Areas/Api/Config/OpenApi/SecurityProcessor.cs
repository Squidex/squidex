// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using NSwag;
using NSwag.Generation.Processors.Security;
using Squidex.Pipeline.OpenApi;
using Squidex.Web;

namespace Squidex.Areas.Api.Config.OpenApi
{
    public sealed class SecurityProcessor : SecurityDefinitionAppender
    {
        public SecurityProcessor(IOptions<UrlsOptions> urlOptions)
            : base(Constants.SecurityDefinition, Enumerable.Empty<string>(), CreateOAuthSchema(urlOptions.Value))
        {
        }

        private static OpenApiSecurityScheme CreateOAuthSchema(UrlsOptions urlOptions)
        {
            var security = new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.OAuth2
            };

            var tokenUrl = urlOptions.BuildUrl($"{Constants.IdentityServerPrefix}/connect/token", false);

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
                [Constants.ApiScope] = "Read and write access to the API"
            };
        }

        private static void SetupDescription(OpenApiSecurityScheme securityScheme, string tokenUrl)
        {
            var securityText = NSwagHelper.SecurityDocs.Replace("<TOKEN_URL>", tokenUrl);

            securityScheme.Description = securityText;
        }
    }
}
