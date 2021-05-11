// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using NJsonSchema;
using NSwag;
using Squidex.Infrastructure;

namespace Squidex.Areas.Api.Controllers.Contents.Generator
{
    internal sealed class OperationsBuilder
    {
        public Builder Parent { get; init; }

        public string Path { get; init; }

        public string SchemaName { get; init; }

        public string SchemaTypeName { get; init; }

        public string SchemaDisplayName { get; init; }

        public JsonSchema ContentSchema { get; init; }

        public JsonSchema ContentsSchema { get; init; }

        public JsonSchema DataSchema { get; init; }

        public string? FormatText(string text)
        {
            return text?.Replace("[schema]", $"'{SchemaDisplayName}'", StringComparison.Ordinal);
        }

        public OperationBuilder AddOperation(string method, string path)
        {
            var operation = new OpenApiOperation
            {
                Tags = new List<string>
                {
                    SchemaDisplayName
                }
            };

            var operations = Parent.Document.Paths.GetOrAddNew($"{Path}{path}");

            operations[method] = operation;

            return new OperationBuilder(this, operation);
        }
    }
}
