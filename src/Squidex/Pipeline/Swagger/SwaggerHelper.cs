// ==========================================================================
//  SwaggerHelper.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using NJsonSchema;
using NSwag;
using Squidex.Config;
using System.Reflection;

namespace Squidex.Pipeline.Swagger
{
    public static class SwaggerHelper
    {
        private static readonly ConcurrentDictionary<string, string> docs = new ConcurrentDictionary<string, string>();

        public static string LoadDocs(string name)
        {
            return docs.GetOrAdd(name, x =>
            {
                var assembly = typeof(SwaggerHelper).GetTypeInfo().Assembly;

                using (var resourceStream = assembly.GetManifestResourceStream($"Squidex.Docs.{name}.md"))
                {
                    var streamReader = new StreamReader(resourceStream);

                    return streamReader.ReadToEnd();
                }
            });
        }

        public static SwaggerSecurityScheme CreateOAuthSchema(MyUrlsOptions urlOptions)
        {
            var tokenUrl = urlOptions.BuildUrl($"{Constants.IdentityPrefix}/connect/token");

            var securityDocs = LoadDocs("security");
            var securityDescription = securityDocs.Replace("<TOKEN_URL>", tokenUrl);

            var result = 
                new SwaggerSecurityScheme
                {
                    TokenUrl = tokenUrl,
                    Type = SwaggerSecuritySchemeType.OAuth2,
                    Flow = SwaggerOAuth2Flow.Application,
                    Scopes = new Dictionary<string, string>
                    {
                        { Constants.ApiScope, "Read and write access to the API" }
                    },
                    Description = securityDescription
                };

            return result;
        }

        public static void AddQueryParameter(this SwaggerOperation operation, string name, JsonObjectType type, string description = null)
        {
            var parameter = new SwaggerParameter { Type = type, Name = name, Kind = SwaggerParameterKind.Query };

            if (!string.IsNullOrWhiteSpace(description))
            {
                parameter.Description = description;
            }

            operation.Parameters.Add(parameter);
        }

        public static void AddPathParameter(this SwaggerOperation operation, string name, JsonObjectType type, string description = null)
        {
            var parameter = new SwaggerParameter { Type = type, Name = name, Kind = SwaggerParameterKind.Path };

            if (!string.IsNullOrWhiteSpace(description))
            {
                parameter.Description = description;
            }

            parameter.IsRequired = true;
            parameter.IsNullableRaw = false;

            operation.Parameters.Add(parameter);
        }

        public static void AddBodyParameter(this SwaggerOperation operation, JsonSchema4 schema, string name, string description)
        {
            var parameter = new SwaggerParameter { Schema = schema, Name = name, Kind = SwaggerParameterKind.Body };

            if (!string.IsNullOrWhiteSpace(description))
            {
                parameter.Description = description;
            }

            parameter.IsRequired = true;
            parameter.IsNullableRaw = false;

            operation.Parameters.Add(parameter);
        }

        public static void AddResponse(this SwaggerOperation operation, string statusCode, string description, JsonSchema4 schema = null)
        {
            var response = new SwaggerResponse { Description = description, Schema = schema };

            operation.Responses.Add(statusCode, response);
        }
    }
}
