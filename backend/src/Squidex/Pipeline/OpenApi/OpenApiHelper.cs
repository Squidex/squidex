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
    public static class OpenApiHelper
    {
        public static readonly string SecurityDocs = LoadDocs("security");

        public static readonly string SchemaBodyDocs = LoadDocs("schemabody");

        public static readonly string SchemaQueryDocs = LoadDocs("schemaquery");

        private static string LoadDocs(string name)
        {
            var assembly = typeof(OpenApiHelper).Assembly;

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
    }
}
