﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using NJsonSchema;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.GenerateJsonSchema;
using Squidex.Domain.Apps.Core.Schemas;
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
            var languagesConfig = LanguagesConfig.Build(Language.DE, Language.EN);

            var jsonSchema = schema.BuildJsonSchema(languagesConfig.ToResolver(), (n, s) => new JsonSchema4 { Reference = s });
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

        [Fact]
        public void Should_build_data_schema()
        {
            var languagesConfig = LanguagesConfig.Build(Language.DE, Language.EN);

            var jsonSchema = schema.BuildJsonSchema(languagesConfig.ToResolver(), (n, s) => new JsonSchema4 { Reference = s });

            Assert.NotNull(new ContentSchemaBuilder().CreateContentSchema(schema, jsonSchema));
        }

        private static HashSet<string> AllPropertyNames(JsonSchema4 schema)
        {
            var result = new HashSet<string>();

            void AddProperties(JsonSchema4 current)
            {
                if (current != null)
                {
                    if (current.Properties != null)
                    {
                        foreach (var property in current.Properties)
                        {
                            result.Add(property.Key);

                            AddProperties(property.Value);
                        }
                    }

                    AddProperties(current.Item);
                    AddProperties(current.Reference);
                }
            }

            AddProperties(schema);

            return result;
        }
    }
}
