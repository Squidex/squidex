// ==========================================================================
//  Schema.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure;

// ReSharper disable InvertIf

namespace Squidex.Core.Schemas
{
    public sealed class Schema
    {
        private readonly string name;
        private readonly bool isPublished;
        private readonly SchemaProperties properties;
        private readonly ImmutableDictionary<long, Field> fieldsById;
        private readonly Dictionary<string, Field> fieldsByName;

        public string Name
        {
            get { return name; }
        }

        public bool IsPublished
        {
            get { return isPublished; }
        }

        public ImmutableDictionary<long, Field> Fields
        {
            get { return fieldsById; }
        }

        public SchemaProperties Properties
        {
            get { return properties; }
        }

        public Schema(string name, SchemaProperties properties, bool isPublished, ImmutableDictionary<long, Field> fields)
        {
            Guard.NotNull(fields, nameof(fields));
            Guard.NotNull(properties, nameof(properties));
            Guard.ValidSlug(name, nameof(name));

            this.name = name;

            this.properties = properties;
            this.isPublished = isPublished;

            fieldsById = fields;
            fieldsByName = fields.Values.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

            properties.Freeze();
        }

        public static Schema Create(string name, SchemaProperties newProperties)
        {
            if (!name.IsSlug())
            {
                var error = new ValidationError("Name must be a valid slug", "Name");

                throw new ValidationException("Cannot create a new schema", error);
            }

            return new Schema(name, newProperties, false, ImmutableDictionary<long, Field>.Empty);
        }

        public Schema Update(SchemaProperties newProperties)
        {
            Guard.NotNull(newProperties, nameof(newProperties));

            return new Schema(name, newProperties, isPublished, fieldsById);
        }

        public Schema AddOrUpdateField(Field field)
        {
            Guard.NotNull(field, nameof(field));

            if (fieldsById.Values.Any(f => f.Name == field.Name && f.Id != field.Id))
            {
                throw new ValidationException($"A field with name '{field.Name}' already exists.");
            }

            return new Schema(name, properties, isPublished, fieldsById.SetItem(field.Id, field));
        }

        public Schema UpdateField(long fieldId, FieldProperties newProperties)
        {
            return UpdateField(fieldId, field => field.Update(newProperties));
        }

        public Schema DisableField(long fieldId)
        {
            return UpdateField(fieldId, field => field.Disable());
        }

        public Schema EnableField(long fieldId)
        {
            return UpdateField(fieldId, field => field.Enable());
        }

        public Schema HideField(long fieldId)
        {
            return UpdateField(fieldId, field => field.Hide());
        }

        public Schema ShowField(long fieldId)
        {
            return UpdateField(fieldId, field => field.Show());
        }

        public Schema RenameField(long fieldId, string newName)
        {
            return UpdateField(fieldId, field => field.Rename(newName));
        }

        public Schema DeleteField(long fieldId)
        {
            return new Schema(name, properties, isPublished, fieldsById.Remove(fieldId));
        }

        public Schema Publish()
        {
            if (isPublished)
            {
                throw new DomainException("Schema is already published");
            }

            return new Schema(name, properties, true, fieldsById);
        }

        public Schema Unpublish()
        {
            if (!isPublished)
            {
                throw new DomainException("Schema is not published");
            }

            return new Schema(name, properties, false, fieldsById);
        }

        public Schema UpdateField(long fieldId, Func<Field, Field> updater)
        {
            Guard.NotNull(updater, nameof(updater));

            Field field;

            if (!fieldsById.TryGetValue(fieldId, out field))
            {
                throw new DomainObjectNotFoundException(fieldId.ToString(), "Fields", typeof(Field));
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

                Field field;

                if (fieldsByName.TryGetValue(kvp.Key, out field))
                {
                    await field.ValidateAsync(kvp.Value, fieldErrors);
                }
                else
                {
                    fieldErrors.Add($"{kvp.Key} is not a known field");
                }

                foreach (var error in fieldErrors)
                {
                    errors.Add(new ValidationError(error, kvp.Key));
                }
            }

            foreach (var field in fieldsByName.Values)
            {
                if (field.RawProperties.IsRequired && !data.Contains(field.Name))
                {
                    errors.Add(new ValidationError($"{field.Name} is required", field.Name));
                }
            }
        }
    }
}