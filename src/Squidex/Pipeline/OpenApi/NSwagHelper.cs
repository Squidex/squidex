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

namespace Squidex.Pipeline.OpenApi
{
    public static class NSwagHelper
    {
        public static readonly string SecurityDocs = LoadDocs("security");

        public static readonly string SchemaBodyDocs = LoadDocs("schemabody");

        public static readonly string SchemaQueryDocs = LoadDocs("schemaquery");

        private static string LoadDocs(string name)
        {
            var assembly = typeof(NSwagHelper).Assembly;

            using (var resourceStream = assembly.GetManifestResourceStream($"Squidex.Docs.{name}.md"))
            {
                using (var streamReader = new StreamReader(resourceStream!))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        public static OpenApiDocument CreateApiDocument(HttpContext context, string appName)
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
                SchemaType = SchemaType.OpenApi3
            };

            if (!string.IsNullOrWhiteSpace(context.Request.Host.Value))
            {
                document.Host = context.Request.Host.Value;
            }

            return document;
        }

        public static void AddQuery(this OpenApiOperation operation, string name, JsonObjectType type, string description)
        {
            var schema = new JsonSchema { Type = type };

            operation.AddParameter(name, schema, OpenApiParameterKind.Query, description, false);
        }

        public static void AddPathParameter(this OpenApiOperation operation, string name, JsonObjectType type, string description, string? format = null)
        {
            var schema = new JsonSchema { Type = type, Format = format };

            operation.AddParameter(name, schema, OpenApiParameterKind.Path, description, true);
        }

        public static void AddBody(this OpenApiOperation operation, string name, JsonSchema schema, string description)
        {
            operation.AddParameter(name, schema, OpenApiParameterKind.Body, description, true);
        }

        private static void AddParameter(this OpenApiOperation operation, string name, JsonSchema schema, OpenApiParameterKind kind, string description, bool isRequired)
        {
            var parameter = new OpenApiParameter { Schema = schema, Name = name, Kind = kind };

            if (!string.IsNullOrWhiteSpace(description))
            {
                parameter.Description = description;
            }

            parameter.IsRequired = isRequired;

            operation.Parameters.Add(parameter);
        }

        public static void AddResponse(this OpenApiOperation operation, string statusCode, string description, JsonSchema? schema = null)
        {
            var response = new OpenApiResponse { Description = description, Schema = schema };

            operation.Responses.Add(statusCode, response);
        }
    }
}
