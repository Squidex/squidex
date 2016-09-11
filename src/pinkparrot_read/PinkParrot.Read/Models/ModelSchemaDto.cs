// ==========================================================================
//  ModelSchemaDto.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using PinkParrot.Core.Schema;
using PinkParrot.Infrastructure;
using Newtonsoft.Json;
// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InvertIf

namespace PinkParrot.Read.Models
{
    public sealed class ModelSchemaDto
    {
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public Dictionary<long, ModelFieldDto> Fields { get; set; }

        [JsonProperty]
        public ModelSchemaProperties Properties { get; set; }

        public static ModelSchemaDto Create(ModelSchema schema)
        {
            Guard.NotNull(schema, nameof(schema));

            var dto = new ModelSchemaDto { Properties = schema.Properties, Name = schema.Name };

            dto.Fields = 
                schema.Fields.ToDictionary(
                    kvp => kvp.Key,
                    kvp => ModelFieldDto.Create(kvp.Value));

            return dto;
        }

        public ModelSchema ToSchema(ModelFieldRegistry registry)
        {
            Guard.NotNull(registry, nameof(registry));

            var schema = ModelSchema.Create(Name, Properties);

            if (Fields != null)
            {
                foreach (var kvp in Fields)
                {
                    var field = kvp.Value;

                    schema = schema.AddOrUpdateField(registry.CreateField(kvp.Key, field.Name, field.Properties));
                }
            }

            return schema;
        }
    }
}
