// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.GenerateJsonSchema;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Operations.GenerateJsonSchema;

public class JsonSchemaTests
{
    [Fact]
    public void Should_build_json_schema()
    {
        var languagesConfig = LanguagesConfig.English.Set(Language.DE);

        var (schema, components) = TestSchema.MixedSchema();

        var jsonSchema = schema.BuildJsonSchema(languagesConfig.ToResolver(), components);

        CheckFields(jsonSchema, schema);
    }

    [Fact]
    public void Should_build_json_dynamic_json_schema()
    {
        var languagesConfig = LanguagesConfig.English.Set(Language.DE);

        var (schema, components) = TestSchema.MixedSchema();

        var jsonSchema = schema.BuildJsonSchemaDynamic(languagesConfig.ToResolver(), components);

        CheckFields(jsonSchema, schema);
    }

    [Fact]
    public void Should_build_flat_json_schema()
    {
        var languagesConfig = LanguagesConfig.English.Set(Language.DE);

        var (schema, components) = TestSchema.MixedSchema();

        var jsonSchema = schema.BuildJsonSchemaFlat(languagesConfig.ToResolver(), components);

        CheckFields(jsonSchema, schema);
    }

    private static void CheckFields(JsonSchema jsonSchema, Schema schema)
    {
        var jsonProperties = AllPropertyNames(jsonSchema);

        void CheckField(IField field)
        {
            if (!field.IsForApi())
            {
                Assert.DoesNotContain(field.Name, jsonProperties);
            }
            else
            {
                Assert.Contains(field.Name, jsonProperties);
            }

            if (field is IArrayField array)
            {
                foreach (var nested in array.Fields)
                {
                    CheckField(nested);
                }
            }
        }

        foreach (var field in schema.Fields)
        {
            CheckField(field);
        }
    }

    private static HashSet<string> AllPropertyNames(JsonSchema schema)
    {
        var actual = new HashSet<string>();

        void AddProperties(JsonSchema current)
        {
            if (current == null)
            {
                return;
            }

            foreach (var (key, value) in current.Properties.OrEmpty())
            {
                actual.Add(key);

                AddProperties(value);
            }

            AddProperties(current.Item);
            AddProperties(current.Reference);
            AddProperties(current.AdditionalItemsSchema);
            AddProperties(current.AdditionalPropertiesSchema);
        }

        AddProperties(schema);

        return actual;
    }
}
