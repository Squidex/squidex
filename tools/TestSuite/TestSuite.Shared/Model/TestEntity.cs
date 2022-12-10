// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;

#pragma warning disable MA0048 // File name must match type name

namespace TestSuite.Model;

public sealed class TestEntity : Content<TestEntityData>
{
    public const int ScriptTrigger = -99;

    public static async Task<SchemaDto> CreateSchemaAsync(ISchemasClient schemas, string appName, string name, SchemaScriptsDto scripts = null)
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
                    Name = TestEntityData.JsonField,
                    Properties = new JsonFieldPropertiesDto
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
                },
                new UpsertSchemaFieldDto
                {
                    Name = TestEntityData.IdField,
                    Properties = new StringFieldPropertiesDto
                    {
                        IsRequired = false
                    }
                }
            },
            Scripts = scripts,
            IsPublished = true
        });

        return schema;
    }

    public static TestEntityData CreateTestEntry(int index)
    {
        var data = new TestEntityData
        {
            Number = index,
            Json = JObject.FromObject(new
            {
                nested0 = index,
                nested1 = new
                {
                    nested2 = index
                }
            }),
            String = index.ToString(CultureInfo.InvariantCulture)
        };

        if (index % 2 == 0)
        {
            data.Geo = new
            {
                type = "Point",
                coordinates = new[]
                {
                    index,
                    index
                }
            };
        }
        else
        {
            data.Geo = new { longitude = index, latitude = index };
        }

        return data;
    }
}

public sealed class TestEntityData
{
    public static readonly string LocalizedField = nameof(Localized).ToLowerInvariant();

    public static readonly string StringField = nameof(String).ToLowerInvariant();

    public static readonly string NumberField = nameof(Number).ToLowerInvariant();

    public static readonly string JsonField = nameof(Json).ToLowerInvariant();

    public static readonly string GeoField = nameof(Geo).ToLowerInvariant();

    public static readonly string IdField = nameof(Id).ToLowerInvariant();

    public Dictionary<string, string> Localized { get; set; }

    [JsonConverter(typeof(InvariantConverter))]
    public int Number { get; set; }

    [JsonConverter(typeof(InvariantConverter))]
    public string Id { get; set; }

    [JsonConverter(typeof(InvariantConverter))]
    public string String { get; set; }

    [JsonConverter(typeof(InvariantConverter))]
    public JToken Json { get; set; }

    [JsonConverter(typeof(InvariantConverter))]
    public object Geo { get; set; }
}
