// ==========================================================================
//  SwaggerIdentityUsage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Globalization;
using NSwag;
using NSwag.AspNetCore;
using NSwag.CodeGeneration.SwaggerGenerators.WebApi.Processors.Security;

namespace Squidex.Configurations.Identity
{
    public static class SwaggerIdentityUsage
    {
        private const string DescriptionPattern =
@"To retrieve an access token, the client id must make a request to the token url. For example:

    $ curl
        -X POST '{0}' 
        -H 'Content-Type: application/x-www-form-urlencoded' 
        -d 'grant_type=client_credentials&client_id=[APP_NAME]&client_secret=[CLIENT_KEY]'";

        public static SwaggerOwinSettings ConfigureIdentity(this SwaggerOwinSettings settings, MyUrlsOptions options)
        {
            var tokenUrl = options.BuildUrl($"{Constants.IdentityPrefix}/connect/token");

            var description = string.Format(CultureInfo.InvariantCulture, DescriptionPattern, tokenUrl);

            settings.DocumentProcessors.Add(
                new SecurityDefinitionAppender("OAuth2", new SwaggerSecurityScheme
                {
                    TokenUrl = tokenUrl,
                    Type = SwaggerSecuritySchemeType.OAuth2,
                    Flow = SwaggerOAuth2Flow.Application,
                    Scopes = new Dictionary<string, string>
                    {
                        { Constants.ApiScope, "Read and write access to the API" }
                    },
                    Description = description
                }));

            settings.OperationProcessors.Add(new OperationSecurityScopeProcessor("roles"));

            return settings;
        }
    }
}
