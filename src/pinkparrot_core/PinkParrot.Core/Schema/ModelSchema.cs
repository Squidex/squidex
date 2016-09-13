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
// ReSharper disable InvertIf

namespace PinkParrot.Core.Schema
{
    public sealed class ModelSchema
    {
        private readonly string name;
        private readonly ModelSchemaProperties properties;
        private readonly ImmutableDictionary<long, ModelField> fieldsById;
        private readonly Dictionary<string, ModelField> fieldsByName;

        public string Name
        {
            get { return name; }
        }

        public ImmutableDictionary<long, ModelField> Fields
        {
            get { return fieldsById; }
        }

        public ModelSchemaProperties Properties
        {
            get { return properties; }
        }

        public ModelSchema(string name, ModelSchemaProperties properties, ImmutableDictionary<long, ModelField> fields)
        {
            Guard.NotNull(fields, nameof(fields));
            Guard.NotNull(properties, nameof(properties));
            Guard.ValidSlug(name, nameof(name));

            this.name = name;

            this.properties = properties;

            fieldsById = fields;
            fieldsByName = fields.Values.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        }

        public static ModelSchema Create(string name, ModelSchemaProperties newProperties)
        {
            newProperties = newProperties ?? new ModelSchemaProperties(null, null);

            if (!name.IsSlug())
            {
                var error = new ValidationError("Name must be a valid slug", "Name");

                throw new ValidationException($"Cannot create a new schema", error);
            }

            return new ModelSchema(name, newProperties, ImmutableDictionary<long, ModelField>.Empty);
        }

        public ModelSchema Update(ModelSchemaProperties newProperties)
        {
            Guard.NotNull(newProperties, nameof(newProperties));

            return new ModelSchema(name, newProperties, fieldsById);
        }

        public ModelSchema AddOrUpdateField(ModelField field)
        {
            Guard.NotNull(field, nameof(field));

            if (fieldsById.Values.Any(f => f.Name == field.Name && f.Id != field.Id))
            {
                throw new ValidationException($"A field with name '{field.Name}' already exists.");
            }

            return new ModelSchema(name, properties, fieldsById.SetItem(field.Id, field));
        }

        public ModelSchema UpdateField(long fieldId, IModelFieldProperties newProperties)
        {
            return UpdateField(fieldId, field => field.Update(newProperties));
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

        public ModelSchema RenameField(long fieldId, string newName)
        {
            return UpdateField(fieldId, field => field.Rename(newName));
        }

        public ModelSchema DeleteField(long fieldId)
        {
            return new ModelSchema(name, properties, fieldsById.Remove(fieldId));
        }

        public ModelSchema UpdateField(long fieldId, Func<ModelField, ModelField> updater)
        {
            Guard.NotNull(updater, nameof(updater));

            ModelField field;

            if (!fieldsById.TryGetValue(fieldId, out field))
            {
                throw new DomainObjectNotFoundException(fieldId.ToString(), typeof(ModelField));
            }

            var newField = updater(field);

            return AddOrUpdateField(newField);
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