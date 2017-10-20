// ==========================================================================
//  JsonSchemaModel.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;

namespace Squidex.Domain.Apps.Core.Schemas.Json
{
    public sealed class JsonSchemaModel
    {
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public bool IsPublished { get; set; }

        [JsonProperty]
        public SchemaProperties Properties { get; set; }

        [JsonProperty]
        public List<JsonFieldModel> Fields { get; set; }

        public JsonSchemaModel()
        {
        }

        public JsonSchemaModel(Schema schema)
        {
            Name = schema.Name;

            Properties = schema.Properties;

            Fields =
                schema.Fields?.Select(x =>
                    new JsonFieldModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        IsHidden = x.IsHidden,
                        IsLocked = x.IsLocked,
                        IsDisabled = x.IsDisabled,
                        Partitioning = x.Partitioning.Key,
                        Properties = x.RawProperties
                    }).ToList();

            IsPublished = schema.IsPublished;
        }

        public Schema ToSchema(FieldRegistry fieldRegistry)
        {
            var fields = Fields?.Select(fieldModel =>
            {
                var parititonKey = new Partitioning(fieldModel.Partitioning);

                var field = fieldRegistry.CreateField(fieldModel.Id, fieldModel.Name, parititonKey, fieldModel.Properties);

                if (fieldModel.IsDisabled)
                {
                    field = field.Disable();
                }

                if (fieldModel.IsLocked)
                {
                    field = field.Lock();
                }

                if (fieldModel.IsHidden)
                {
                    field = field.Hide();
                }

                return field;
            }).ToImmutableList() ?? ImmutableList<Field>.Empty;

            var schema = new Schema(Name, IsPublished, Properties, fields);

            return schema;
        }
    }
}