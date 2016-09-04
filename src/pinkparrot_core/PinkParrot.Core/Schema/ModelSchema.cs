// ==========================================================================
//  ModelSchema.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using PinkParrot.Infrastructure;

namespace PinkParrot.Core.Schema
{
    public sealed class ModelSchema
    {
        private readonly ModelSchemaProperties properties;
        private readonly ImmutableDictionary<long, ModelField> fields;
        private readonly Dictionary<string, ModelField> fieldsByName;

        public ModelSchema(ModelSchemaProperties properties, ImmutableDictionary<long, ModelField> fields)
        {
            Guard.NotNull(fields, nameof(fields));
            Guard.NotNull(properties, nameof(properties));

            this.fields = fields;

            this.properties = properties;

            fieldsByName = fields.Values.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        }

        public static ModelSchema Create(ModelSchemaProperties metadata)
        {
            var errors = new List<ValidationError>();

            metadata.Validate(errors);

            if (errors.Any())
            {
                throw new ValidationException("Failed to create a new model schema.", errors);
            }

            return new ModelSchema(metadata, ImmutableDictionary<long, ModelField>.Empty);
        }

        public IReadOnlyDictionary<long, ModelField> Fields
        {
            get { return fields; }
        }

        public ModelSchemaProperties Properties
        {
            get { return properties; }
        }

        public ModelSchema Update(ModelSchemaProperties newMetadata)
        {
            Guard.NotNull(newMetadata, nameof(newMetadata));

            return new ModelSchema(newMetadata, fields);
        }

        public ModelSchema AddField(long id, ModelFieldProperties fieldProperties, ModelFieldFactory factory)
        {
            var field = factory.CreateField(id, fieldProperties);

            return SetField(field);
        }

        public ModelSchema SetField(long fieldId, ModelFieldProperties fieldProperties)
        {
            Guard.NotNull(fieldProperties, nameof(fieldProperties));

            return UpdateField(fieldId, field =>
            {
                var errors = new List<ValidationError>();

                var newField = field.Configure(fieldProperties, errors);

                if (errors.Any())
                {
                    throw new ValidationException($"Cannot update field with id '{fieldId}', becase the settings are invalid.", errors);
                }

                return newField;
            });
        }

        public ModelSchema DisableField(long fieldId)
        {
            return UpdateField(fieldId, field => field.Disable());
        }

        public ModelSchema EnableField(long fieldId)
        {
            return UpdateField(fieldId, field => field.Enable());
        }

        public ModelSchema HideField(long fieldId)
        {
            return UpdateField(fieldId, field => field.Show());
        }

        public ModelSchema ShowField(long fieldId)
        {
            return UpdateField(fieldId, field => field.Show());
        }

        public ModelSchema SetField(ModelField field)
        {
            Guard.NotNull(field, nameof(field));

            if (fields.Values.Any(f => f.Name == field.Name && f.Id != field.Id))
            {
                throw new ValidationException($"A field with name '{field.Name}' already exists.");
            }

            return new ModelSchema(properties, fields.SetItem(field.Id, field));
        }

        public ModelSchema DeleteField(long fieldId)
        {
            if (!fields.ContainsKey(fieldId))
            {
                throw new ValidationException($"A field with id {fieldId} does not exist.");
            }

            return new ModelSchema(properties, fields.Remove(fieldId));
        }

        private ModelSchema UpdateField(long fieldId, Func<ModelField, ModelField> updater)
        {
            ModelField field;

            if (!fields.TryGetValue(fieldId, out field))
            {
                throw new ValidationException($"Cannot update field with id '{fieldId}'.", 
                    new ValidationError("Field does not exist.", "fieldId"));
            }

            var newField = updater(field);

            return SetField(newField);
        }

        public async Task ValidateAsync(PropertiesBag data)
        {
            Guard.NotNull(data, nameof(data));

            var errors = new List<ValidationError>();

            foreach (var kvp in data.Properties)
            {
                var fieldErrors = new List<string>();

                ModelField field;

                if (fieldsByName.TryGetValue(kvp.Key, out field))
                {
                    var newErrors = new List<string>();

                    await field.ValidateAsync(kvp.Value, newErrors);
                }
                else
                {
                    fieldErrors.Add($"'{kvp.Key}' is not a known field");
                }

                errors.AddRange(fieldErrors.Select(x => new ValidationError(x, kvp.Key)));
            }

            if (errors.Any())
            {
                throw new ValidationException("The data is not valid.", errors);
            }
        }
    }
}