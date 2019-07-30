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

namespace Squidex.Pipeline.OpenApi
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

        public static OpenApiDocument CreateApiDocument(HttpContext context, UrlsOptions urlOptions, string appName)
        {
            var scheme =
                string.Equals(context.Request.Scheme, "http", StringComparison.OrdinalIgnoreCase) ?
                    OpenApiSchema.Http :
                    OpenApiSchema.Https;

            var document = new OpenApiDocument
            {
                Schemes = new List<OpenApiSchema>
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
                Info = new OpenApiInfo
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

        public static void AddQueryParameter(this OpenApiOperation operation, string name, JsonObjectType type, string description = null)
        {
            var parameter = new OpenApiParameter { Type = type, Name = name, Kind = OpenApiParameterKind.Query };

            if (!string.IsNullOrWhiteSpace(description))
            {
                parameter.Description = description;
            }

            operation.Parameters.Add(parameter);
        }

        public static void AddPathParameter(this OpenApiOperation operation, string name, JsonObjectType type, string description = null)
        {
            var parameter = new OpenApiParameter { Type = type, Name = name, Kind = OpenApiParameterKind.Path };

            if (!string.IsNullOrWhiteSpace(description))
            {
                parameter.Description = description;
            }

            parameter.IsRequired = true;
            parameter.IsNullableRaw = false;

            operation.Parameters.Add(parameter);
        }

        public static void AddBodyParameter(this OpenApiOperation operation, string name, JsonSchema schema, string description)
        {
            var parameter = new OpenApiParameter { Schema = schema, Name = name, Kind = OpenApiParameterKind.Body };

            if (!string.IsNullOrWhiteSpace(description))
            {
                parameter.Description = description;
            }

            parameter.IsRequired = true;
            parameter.IsNullableRaw = false;

            operation.Parameters.Add(parameter);
        }

        public static void AddResponse(this OpenApiOperation operation, string statusCode, string description, JsonSchema schema = null)
        {
            var response = new OpenApiResponse { Description = description, Schema = schema };

            operation.Responses.Add(statusCode, response);
        }
    }
}
