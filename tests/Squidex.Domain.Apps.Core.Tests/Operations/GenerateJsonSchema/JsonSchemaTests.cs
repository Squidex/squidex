// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
        private readonly Schema schema = TestData.MixedSchema();

        [Fact]
        public void Should_build_json_schema()
        {
            var languagesConfig = LanguagesConfig.Build(Language.DE, Language.EN);

            var jsonSchema = schema.BuildJsonSchema(languagesConfig.ToResolver(), (n, s) => new JsonSchema4 { Reference = s });

            Assert.NotNull(jsonSchema);
        }

        [Fact]
        public void Should_build_data_schema()
        {
            var languagesConfig = LanguagesConfig.Build(Language.DE, Language.EN);

            var jsonSchema = schema.BuildJsonSchema(languagesConfig.ToResolver(), (n, s) => new JsonSchema4 { Reference = s });

            Assert.NotNull(new ContentSchemaBuilder().CreateContentSchema(schema, jsonSchema));
        }
    }
}
