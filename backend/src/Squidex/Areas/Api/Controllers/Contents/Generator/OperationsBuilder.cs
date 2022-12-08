// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;
using NSwag;
using Squidex.Infrastructure;

namespace Squidex.Areas.Api.Controllers.Contents.Generator;

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

    public void AddTag(string description)
    {
        var tag = new OpenApiTag { Name = SchemaTypeName, Description = FormatText(description) };

        Parent.OpenApiDocument.Tags.Add(tag);
    }

    public OperationBuilder AddOperation(string method, string path)
    {
        var tag = SchemaTypeName;

        var operation = new OpenApiOperation
        {
            Tags = new List<string>
            {
                tag
            }
        };

        var operations = Parent.OpenApiDocument.Paths.GetOrAddNew($"{Path}{path}");

        operations[method] = operation;

        return new OperationBuilder(this, operation);
    }
}
