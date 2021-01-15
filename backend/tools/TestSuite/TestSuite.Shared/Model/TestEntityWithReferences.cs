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
    public sealed class TestEntityWithReferences : Content<TestEntityWithReferencesData>
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
                        Name = nameof(TestEntityWithReferencesData.References).ToLowerInvariant(),
                        Properties = new ReferencesFieldPropertiesDto
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

    public sealed class TestEntityWithReferencesData
    {
        [JsonConverter(typeof(InvariantConverter))]
        public string[] References { get; set; }
    }
}
