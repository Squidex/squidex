// ==========================================================================
//  SwaggerHelper.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NJsonSchema;
using NSwag;
using NSwag.SwaggerGeneration;
using Squidex.Config;
using Squidex.Controllers.Api;
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
                    Title = $"Suidex API for {appName} App"
                },
                BasePath = "/api"
            };

            if (!string.IsNullOrWhiteSpace(context.Request.Host.Value))
            {
                document.Host = context.Request.Host.Value;
            }

            document.SecurityDefinitions.Add("OAuth2", CreateOAuthSchema(urlOptions));

            return document;
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
                        { Constants.ApiScope, "Read and write access to the API" },
                        { SquidexRoles.AppOwner, "You get this scope / role when you are owner of the app you are accessing." },
                        { SquidexRoles.AppEditor, "You get this scope / role when you are owner of the app you are accessing or when the subject is a client." },
                        { SquidexRoles.AppDeveloper, "You get this scope / role when you are owner of the app you are accessing." }
                    },
                    Description = securityDescription
                };

            return result;
        }

        public static async Task<JsonSchema4> GetErrorDtoSchemaAsync(this SwaggerGenerator swaggerGenerator)
        {
            var errorType = typeof(ErrorDto);

            return await swaggerGenerator.GenerateAndAppendSchemaFromTypeAsync(errorType, false, null);
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
