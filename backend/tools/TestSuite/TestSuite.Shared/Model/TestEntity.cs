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
        public const int ScriptTrigger = -99;

        public static async Task<SchemaDetailsDto> CreateSchemaAsync(ISchemasClient schemas, string appName, string name)
        {
            var schema = await schemas.PostSchemaAsync(appName, new CreateSchemaDto
            {
                Name = name,
                Fields = new List<UpsertSchemaFieldDto>
                {
                    new UpsertSchemaFieldDto
                    {
                        Name = TestEntityData.NumberField,
                        Properties = new NumberFieldPropertiesDto
                        {
                            IsRequired = true
                        }
                    },
                    new UpsertSchemaFieldDto
                    {
                        Name = TestEntityData.StringField,
                        Properties = new StringFieldPropertiesDto
                        {
                            IsRequired = false
                        }
                    },
                    new UpsertSchemaFieldDto
                    {
                        Name = TestEntityData.GeoField,
                        Properties = new GeolocationFieldPropertiesDto
                        {
                            IsRequired = false
                        }
                    },
                    new UpsertSchemaFieldDto
                    {
                        Name = TestEntityData.LocalizedField,
                        Partitioning = "language",
                        Properties = new StringFieldPropertiesDto
                        {
                            DefaultValue = "default"
                        }
                    }
                },
                Scripts = new SchemaScriptsDto
                {
                    Create = $@"
                        if (ctx.data.{TestEntityData.NumberField}.iv === {ScriptTrigger}) {{
                            ctx.data.{TestEntityData.NumberField}.iv = incrementCounter('my');
                            replace();
                        }}"
                },
                IsPublished = true
            });

            return schema;
        }
    }

    public sealed class TestEntityData
    {
        public static readonly string LocalizedField = nameof(Localized).ToLowerInvariant();

        public static readonly string StringField = nameof(String).ToLowerInvariant();

        public static readonly string NumberField = nameof(Number).ToLowerInvariant();

        public static readonly string GeoField = nameof(Geo).ToLowerInvariant();

        public Dictionary<string, string> Localized { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        public int Number { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        public string String { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        public object Geo { get; set; }
    }
}
