// ==========================================================================
//  SwaggerHelper.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Globalization;
using NJsonSchema;
using NSwag;
using Squidex.Config;

namespace Squidex.Pipeline.Swagger
{
    public static class SwaggerHelper
    {
        private const string SecurityDescription =
@"To retrieve an access token, the client id must make a request to the token url. For example:

    $ curl
        -X POST '{0}' 
        -H 'Content-Type: application/x-www-form-urlencoded' 
        -d 'grant_type=client_credentials&
              client_id=[APP_NAME]:[CLIENT_NAME]&
              client_secret=[CLIENT_SECRET]'";

        public static SwaggerSecurityScheme CreateOAuthSchema(MyUrlsOptions urlOptions)
        {
            var tokenUrl = urlOptions.BuildUrl($"{Constants.IdentityPrefix}/connect/token");

            var description = string.Format(CultureInfo.InvariantCulture, SecurityDescription, tokenUrl);

            return 
                new SwaggerSecurityScheme
                {
                    TokenUrl = tokenUrl,
                    Type = SwaggerSecuritySchemeType.OAuth2,
                    Flow = SwaggerOAuth2Flow.Application,
                    Scopes = new Dictionary<string, string>
                    {
                        { Constants.ApiScope, "Read and write access to the API" }
                    },
                    Description = description
                };
        }

        public static void AddQueryParameter(this SwaggerOperation operation, string name, JsonObjectType type, string description)
        {
            operation.Parameters.Add(
                new SwaggerParameter
                {
                    Type = type,
                    Name = name,
                    Kind = SwaggerParameterKind.Query,
                    Description = description
                });
        }

        public static void AddPathParameter(this SwaggerOperation operation, string name, JsonObjectType type, string description)
        {
            operation.Parameters.Add(
                new SwaggerParameter
                {
                    Type = type,
                    Name = name,
                    Kind = SwaggerParameterKind.Path,
                    IsRequired = true,
                    IsNullableRaw = false,
                    Description = description
                });
        }

        public static void AddBodyParameter(this SwaggerOperation operation, JsonSchema4 schema, string name, string description)
        {
            operation.Parameters.Add(
                new SwaggerParameter
                {
                    Name = name,
                    Kind = SwaggerParameterKind.Body,
                    Schema = schema,
                    IsRequired = true,
                    IsNullableRaw = false,
                    Description = description
                });
        }

        public static void AddResponse(this SwaggerOperation operation, string statusCode, string description, JsonSchema4 schema = null)
        {
            operation.Responses.Add(statusCode, new SwaggerResponse { Description = description, Schema = schema });
        }
    }
}
