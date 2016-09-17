// ==========================================================================
//  SchemaDto.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using PinkParrot.Core.Schemas;
using PinkParrot.Infrastructure;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InvertIf

namespace PinkParrot.Read.Models
{
    public sealed class SchemaDto
    {
        public string Name { get; set; }
        
        public Dictionary<long, FieldDto> Fields { get; set; }
        
        public SchemaProperties Properties { get; set; }

        public static SchemaDto Create(Schema schema)
        {
            Guard.NotNull(schema, nameof(schema));

            var dto = new SchemaDto { Properties = schema.Properties, Name = schema.Name };

            dto.Fields = 
                schema.Fields.ToDictionary(
                    kvp => kvp.Key,
                    kvp => FieldDto.Create(kvp.Value));

            return dto;
        }

        public Schema ToSchema(FieldRegistry registry)
        {
            Guard.NotNull(registry, nameof(registry));

            var schema = Schema.Create(Name, Properties);

            if (Fields != null)
            {
                foreach (var kvp in Fields)
                {
                    var field = kvp.Value;

                    schema = schema.AddOrUpdateField(field.ToField(kvp.Key, registry));
                }
            }

            return schema;
        }
    }
}
