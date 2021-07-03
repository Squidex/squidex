// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using NJsonSchema;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.GenerateJsonSchema;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.GenerateJsonSchema
{
    public class JsonSchemaTests
    {
        private readonly Schema schema = TestUtils.MixedSchema();

        [Fact]
        public void Should_build_json_schema()
        {
            var languagesConfig = LanguagesConfig.English.Set(Language.DE);

            var jsonSchema = schema.BuildJsonSchema(languagesConfig.ToResolver(), ResolvedComponents.Empty);

            CheckFields(jsonSchema);
        }

        [Fact]
        public void Should_build_json_schema_with_resolver()
        {
            var schemaResolver = new SchemaResolver((name, action) => action());

            var jsonSchema = schema.BuildDynamicJsonSchema(schemaResolver, ResolvedComponents.Empty);

            CheckFields(jsonSchema);
        }

        [Fact]
        public void Should_build_flat_json_schema()
        {
            var languagesConfig = LanguagesConfig.English.Set(Language.DE);

            var schemaResolver = new SchemaResolver((name, action) =>
            {
                return action();
            });

            var jsonSchema = schema.BuildFlatJsonSchema(schemaResolver, ResolvedComponents.Empty);

            CheckFields(jsonSchema);
        }

        private void CheckFields(JsonSchema jsonSchema)
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
            var result = new HashSet<string>();

            void AddProperties(JsonSchema current)
            {
                if (current != null)
                {
                    if (current.Properties != null)
                    {
                        foreach (var (key, value) in current.Properties)
                        {
                            result.Add(key);

                            AddProperties(value);
                        }
                    }

                    AddProperties(current.Item);
                    AddProperties(current.Reference);
                    AddProperties(current.AdditionalItemsSchema);
                    AddProperties(current.AdditionalPropertiesSchema);
                }
            }

            AddProperties(schema);

            return result;
        }
    }
}
