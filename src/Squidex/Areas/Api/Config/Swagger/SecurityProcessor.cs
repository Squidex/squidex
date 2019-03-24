// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Microsoft.Extensions.Options;
using NSwag;
using NSwag.SwaggerGeneration.Processors.Security;
using Squidex.Pipeline.Swagger;
using Squidex.Web;

namespace Squidex.Areas.Api.Config.Swagger
{
    public class SecurityProcessor : SecurityDefinitionAppender
    {
        public SecurityProcessor(IOptions<UrlsOptions> urlOptions)
            : base(Constants.SecurityDefinition, CreateOAuthSchema(urlOptions.Value))
        {
        }

        private static SwaggerSecurityScheme CreateOAuthSchema(UrlsOptions urlOptions)
        {
            var securityScheme = new SwaggerSecurityScheme();

            var tokenUrl = urlOptions.BuildUrl($"{Constants.IdentityServerPrefix}/connect/token", false);

            securityScheme.TokenUrl = tokenUrl;

            var securityDocs = NSwagHelper.LoadDocs("security");
            var securityText = securityDocs.Replace("<TOKEN_URL>", tokenUrl);

            securityScheme.Description = securityText;

            securityScheme.Type = SwaggerSecuritySchemeType.OAuth2;
            securityScheme.Flow = SwaggerOAuth2Flow.Application;

            securityScheme.Scopes = new Dictionary<string, string>
            {
                [Constants.ApiScope] = "Read and write access to the API"
            };

            return securityScheme;
        }
    }
}
