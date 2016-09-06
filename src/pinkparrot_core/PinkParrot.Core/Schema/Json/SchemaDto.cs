// ==========================================================================
//  SerializationModel.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using PinkParrot.Infrastructure;

// ReSharper disable LoopCanBeConvertedToQuery

namespace PinkParrot.Core.Schema.Json
{
    public class SchemaDto
    {
        private readonly ModelSchemaProperties properties;
        private readonly ImmutableDictionary<long, ModelFieldProperties> fields;

        [Required]
        public ImmutableDictionary<long, ModelFieldProperties> Fields
        {
            get { return fields; }
        }

        [Required]
        public ModelSchemaProperties Properties
        {
            get { return properties; }
        }

        public SchemaDto(ModelSchemaProperties properties, ImmutableDictionary<long, ModelFieldProperties> fields)
        {
            Guard.NotNull(fields, nameof(fields));
            Guard.NotNull(properties, nameof(properties));

            this.properties = properties;

            this.fields = fields;
        }

        public static SchemaDto Create(ModelSchema schema)
        {
            Guard.NotNull(schema, nameof(schema));

            var fields = schema.Fields.ToDictionary(t => t.Key, t => t.Value.RawProperties).ToImmutableDictionary();

            return new SchemaDto(schema.Properties, fields);
        }

        public ModelSchema ToModelSchema(ModelFieldFactory factory)
        {
            Guard.NotNull(factory, nameof(factory));

            var schema = ModelSchema.Create(properties);

            foreach (var field in fields)
            {
                schema = schema.AddField(field.Key, field.Value, factory);
            }

            return schema;
        }
    }
}
