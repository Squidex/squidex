// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;
using NJsonSchema.Generation;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.HandleRules;

public sealed class EventJsonSchemaGenerator
{
    private readonly Lazy<Dictionary<string, JsonSchema>> schemas;
    private readonly JsonSchemaGenerator schemaGenerator;

    public IReadOnlyCollection<string> AllTypes
    {
        get => schemas.Value.Keys;
    }

    public EventJsonSchemaGenerator(JsonSchemaGenerator schemaGenerator)
    {
        this.schemaGenerator = schemaGenerator;

        schemas = new Lazy<Dictionary<string, JsonSchema>>(GenerateSchemas);
    }

    public JsonSchema? GetSchema(string typeName)
    {
        Guard.NotNull(typeName);

        return schemas.Value.GetValueOrDefault(typeName);
    }

    private Dictionary<string, JsonSchema> GenerateSchemas()
    {
        var result = new Dictionary<string, JsonSchema>(StringComparer.OrdinalIgnoreCase);

        var baseType = typeof(EnrichedEvent);

        var assembly = baseType.Assembly;

        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsAbstract && type.IsAssignableTo(baseType))
            {
                var schema = schemaGenerator.Generate(type);

                result[type.Name] = schema!;
            }
        }

        return result;
    }
}
