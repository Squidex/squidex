// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NJsonSchema;
using NJsonSchema.Generation;
using NSwag;
using Squidex.Config;
using Squidex.Shared.Identity;

namespace Squidex.Pipeline.Swagger
{
    public static class SwaggerHelper
    {
        public static string LoadDocs(string name)
        {
            var assembly = typeof(SwaggerHelper).GetTypeInfo().Assembly;

            using (var resourceStream = assembly.GetManifestResourceStream($"Squidex.Docs.{name}.md"))
            {
                var streamReader = new StreamReader(resourceStream);

                return streamReader.ReadToEnd();
            }
        }

        public static SwaggerDocument CreateApiDocument(HttpContext context, MyUrlsOptions urlOptions, string appName)
        {
            var scheme =
                string.Equals(context.Request.Scheme, "http", StringComparison.OrdinalIgnoreCase) ?
                    SwaggerSchema.Http :
                    SwaggerSchema.Https;

            var document = new SwaggerDocument
            {
                Tags = new List<SwaggerTag>(),
                Schemes = new List<SwaggerSchema>
                {
                    scheme
                },
                Consumes = new List<string>
                {
                    "application/json"
                },
                Produces = new List<string>
                {
                    "application/json"
                },
                Info = new SwaggerInfo
                {
                    ExtensionData = new Dictionary<string, object>
                    {
                        ["x-logo"] = new { url = urlOptions.BuildUrl("images/logo-white.png", false), backgroundColor = "#3f83df" }
                    },
                    Title = $"Squidex API for {appName} App", Version = "1.0"
                },
                BasePath = "/api"
            };

            if (!string.IsNullOrWhiteSpace(context.Request.Host.Value))
            {
                document.Host = context.Request.Host.Value;
            }

            document.SecurityDefinitions.Add(Constants.SecurityDefinition, CreateOAuthSchema(urlOptions));

            return document;
        }

        public static SwaggerSecurityScheme CreateOAuthSchema(MyUrlsOptions urlOptions)
        {
            var tokenUrl = urlOptions.BuildUrl($"{Constants.IdentityServerPrefix}/connect/token", false);

            var securityDocs = LoadDocs("security");
            var securityText = securityDocs.Replace("<TOKEN_URL>", tokenUrl);

            var result =
                new SwaggerSecurityScheme
                {
                    TokenUrl = tokenUrl,
                    Type = SwaggerSecuritySchemeType.OAuth2,
                    Flow = SwaggerOAuth2Flow.Application,
                    Scopes = new Dictionary<string, string>
                    {
                        { Constants.ApiScope, "Read and write access to the API" },
                        { SquidexRoles.AppOwner, "App contributor with Owner permission." },
                        { SquidexRoles.AppEditor, "Client (writer) or App contributor with Editor permission." },
                        { SquidexRoles.AppReader, "Client (readonly) or App contributor with Editor permission." },
                        { SquidexRoles.AppDeveloper, "App contributor with Developer permission." }
                    },
                    Description = securityText
                };

            return result;
        }

        public static async Task<JsonSchema4> GetErrorDtoSchemaAsync(this JsonSchemaGenerator schemaGenerator, JsonSchemaResolver resolver)
        {
            var errorType = typeof(ErrorDto);

            return await schemaGenerator.GenerateWithReference<JsonSchema4>(errorType, Enumerable.Empty<Attribute>(), resolver);
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

        public static void AddBodyParameter(this SwaggerOperation operation, string name, JsonSchema4 schema, string description)
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
