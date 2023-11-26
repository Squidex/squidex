// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json;
using Squidex.ClientLibrary;

#pragma warning disable MA0048 // File name must match type name

namespace TestSuite.Model;

public sealed class TestEntityWithReferences : Content<TestEntityWithReferencesData>
{
    public static async Task<SchemaDto> CreateSchemaAsync(ISchemasClient schemas, string name)
    {
        var schema = await schemas.PostSchemaAsync(new CreateSchemaDto
        {
            Name = name,
            Fields =
            [
                new UpsertSchemaFieldDto
                {
                    Name = TestEntityWithReferencesData.ReferencesField,
                    Properties = new ReferencesFieldPropertiesDto
                    {
                        IsRequired = false
                    }
                },
            ],
            IsPublished = true
        });

        return schema;
    }
}

public sealed class TestEntityWithReferencesData
{
    public static readonly string ReferencesField = nameof(References).ToLowerInvariant();

    [JsonConverter(typeof(InvariantConverter))]
    public string[]? References { get; set; }
}
