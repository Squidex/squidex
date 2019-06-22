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
using NSwag.SwaggerGeneration.Processors.Security;
using Squidex.Pipeline.Swagger;
using Squidex.Web;

namespace Squidex.Areas.Api.Config.Swagger
{
    public sealed class SecurityProcessor : SecurityDefinitionAppender
    {
        public SecurityProcessor(IOptions<UrlsOptions> urlOptions)
            : base(Constants.SecurityDefinition, Enumerable.Empty<string>(), CreateOAuthSchema(urlOptions.Value))
        {
        }

        private static SwaggerSecurityScheme CreateOAuthSchema(UrlsOptions urlOptions)
        {
            var security = new SwaggerSecurityScheme
            {
                Type = SwaggerSecuritySchemeType.OAuth2
            };

            var tokenUrl = urlOptions.BuildUrl($"{Constants.IdentityServerPrefix}/connect/token", false);

            security.TokenUrl = tokenUrl;

            SetupDescription(security, tokenUrl);

            security.Flow = SwaggerOAuth2Flow.Application;

            security.Scopes = new Dictionary<string, string>
            {
                [Constants.ApiScope] = "Read and write access to the API"
            };

            return security;
        }

        private static void SetupDescription(SwaggerSecurityScheme securityScheme, string tokenUrl)
        {
            var securityDocs = NSwagHelper.LoadDocs("security");
            var securityText = securityDocs.Replace("<TOKEN_URL>", tokenUrl);

            securityScheme.Description = securityText;
        }
    }
}
