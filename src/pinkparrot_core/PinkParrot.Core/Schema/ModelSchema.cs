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
        private readonly ImmutableDictionary<long, ModelField> fieldsById;
        private readonly Dictionary<string, ModelField> fieldsByName;

        public ModelSchema(ModelSchemaProperties properties, ImmutableDictionary<long, ModelField> fields)
        {
            Guard.NotNull(fields, nameof(fields));
            Guard.NotNull(properties, nameof(properties));
            
            this.properties = properties;

            fieldsById = fields;
            fieldsByName = fields.Values.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        }

        public ImmutableDictionary<long, ModelField> Fields
        {
            get { return fieldsById; }
        }

        public ModelSchemaProperties Properties
        {
            get { return properties; }
        }

        public static ModelSchema Create(ModelSchemaProperties newProperties)
        {
            Guard.NotNull(newProperties, nameof(newProperties));

            newProperties.Validate(() => "Failed to create a new model schema.");

            return new ModelSchema(newProperties, ImmutableDictionary<long, ModelField>.Empty);
        }

        public ModelSchema Update(ModelSchemaProperties newProperties)
        {
            Guard.NotNull(newProperties, nameof(newProperties));

            newProperties.Validate(() => "Failed to update the model schema.");

            return new ModelSchema(newProperties, fieldsById);
        }

        public ModelSchema AddField(long id, ModelFieldProperties fieldProperties, ModelFieldFactory factory)
        {
            var field = factory.CreateField(id, fieldProperties);

            return ReplaceOrAddField(field);
        }

        public ModelSchema SetField(long fieldId, ModelFieldProperties fieldProperties)
        {
            Guard.NotNull(fieldProperties, nameof(fieldProperties));

            return UpdateField(fieldId, field =>
            {
                fieldProperties.Validate(() => $"Cannot update field with id '{fieldId}', becase the settings are invalid.");

                var newField = field.Configure(fieldProperties);

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

        public ModelSchema DeleteField(long fieldId)
        {
            return new ModelSchema(properties, fieldsById.Remove(fieldId));
        }

        private ModelSchema UpdateField(long fieldId, Func<ModelField, ModelField> updater)
        {
            ModelField field;

            if (!fieldsById.TryGetValue(fieldId, out field))
            {
                throw new DomainObjectNotFoundException(fieldId.ToString(), typeof(ModelField));
            }

            var newField = updater(field);

            return ReplaceOrAddField(newField);
        }

        private ModelSchema ReplaceOrAddField(ModelField field)
        {
            Guard.NotNull(field, nameof(field));

            if (fieldsById.Values.Any(f => f.Name == field.Name && f.Id != field.Id))
            {
                throw new ValidationException($"A field with name '{field.Name}' already exists.");
            }

            return new ModelSchema(properties, fieldsById.SetItem(field.Id, field));
        }

        public async Task ValidateAsync(PropertiesBag data, IList<ValidationError> errors)
        {
            Guard.NotNull(data, nameof(data));
            Guard.NotNull(errors, nameof(errors));

            foreach (var kvp in data.Properties)
            {
                var fieldErrors = new List<string>();

                ModelField field;

                if (fieldsByName.TryGetValue(kvp.Key, out field))
                {
                    await field.ValidateAsync(kvp.Value, fieldErrors);
                }
                else
                {
                    fieldErrors.Add($"'{kvp.Key}' is not a known field");
                }

                fieldErrors.ForEach(error => errors.Add(new ValidationError(error, kvp.Key)));
            }
        }
    }
}