// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;

namespace TestSuite.Model
{
    public sealed class TestEntity : Content<TestEntityData>
    {
        public static async Task<SchemaDetailsDto> CreateSchemaAsync(ISchemasClient schemas, string appName, string name)
        {
            var schema = await schemas.PostSchemaAsync(appName, new CreateSchemaDto
            {
                Name = name,
                Fields = new List<UpsertSchemaFieldDto>
                {
                    new UpsertSchemaFieldDto
                    {
                        Name = nameof(TestEntityData.Number).ToLowerInvariant(),
                        Properties = new NumberFieldPropertiesDto
                        {
                            IsRequired = true
                        }
                    },
                    new UpsertSchemaFieldDto
                    {
                        Name = nameof(TestEntityData.String).ToLowerInvariant(),
                        Properties = new StringFieldPropertiesDto
                        {
                            IsRequired = false
                        }
                    }
                },
                IsPublished = true
            });

            return schema;
        }
    }

    public sealed class TestEntityData
    {
        [JsonConverter(typeof(InvariantConverter))]
        public int Number { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        public string String { get; set; }
    }
}
