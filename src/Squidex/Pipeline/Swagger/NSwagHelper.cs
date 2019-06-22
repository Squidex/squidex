// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using NJsonSchema;
using NSwag;
using Squidex.Web;

namespace Squidex.Pipeline.Swagger
{
    public static class NSwagHelper
    {
        public static string LoadDocs(string name)
        {
            var assembly = typeof(NSwagHelper).Assembly;

            using (var resourceStream = assembly.GetManifestResourceStream($"Squidex.Docs.{name}.md"))
            {
                using (var streamReader = new StreamReader(resourceStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        public static SwaggerDocument CreateApiDocument(HttpContext context, UrlsOptions urlOptions, string appName)
        {
            var scheme =
                string.Equals(context.Request.Scheme, "http", StringComparison.OrdinalIgnoreCase) ?
                    SwaggerSchema.Http :
                    SwaggerSchema.Https;

            var document = new SwaggerDocument
            {
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
                    Title = $"Squidex API for {appName} App"
                },
                BasePath = Constants.ApiPrefix
            };

            if (!string.IsNullOrWhiteSpace(context.Request.Host.Value))
            {
                document.Host = context.Request.Host.Value;
            }

            return document;
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
