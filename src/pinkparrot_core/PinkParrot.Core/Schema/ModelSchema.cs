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
        private readonly ModelSchemaMetadata metadata;
        private readonly ImmutableDictionary<Guid, ModelField> fields;
        private readonly Dictionary<string, ModelField> fieldsByName;

        public ModelSchema(ModelSchemaMetadata metadata, ImmutableDictionary<Guid, ModelField> fields)
        {
            Guard.NotNull(fields, nameof(fields));
            Guard.NotNull(metadata, nameof(metadata));

            this.fields = fields;

            this.metadata = metadata;

            fieldsByName = fields.Values.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        }

        public static ModelSchema Create(string name)
        {
            if (!name.IsSlug())
            {
                throw new DomainValidationException("Cannot create the schema.", $"'{name}' is not a valid slug.");
            }

            return new ModelSchema(new ModelSchemaMetadata(name), ImmutableDictionary<Guid, ModelField>.Empty);
        }

        public IReadOnlyDictionary<Guid, ModelField> Fields
        {
            get { return fields; }
        }

        public ModelSchemaMetadata Metadata
        {
            get { return metadata; }
        }

        public ModelSchema Update(ModelSchemaMetadata newMetadata)
        {
            Guard.NotNull(newMetadata, nameof(newMetadata));

            return new ModelSchema(newMetadata, fields);
        }

        public ModelSchema AddField(Guid id, string type, string fieldName, ModelFieldFactory factory)
        {
            var field = factory.CreateField(id, type, fieldName);

            return SetField(field);
        }

        public ModelSchema SetField(Guid fieldId, PropertiesBag settings)
        {
            Guard.NotNull(settings, nameof(settings));

            return UpdateField(fieldId, field =>
            {
                var errors = new List<string>();

                var newField = field.Configure(settings, errors);

                if (errors.Any())
                {
                    throw new DomainValidationException($"Cannot update field with id '{fieldId}', becase the settings are invalid.", errors);
                }

                return newField;
            });
        }

        public ModelSchema DisableField(Guid fieldId)
        {
            return UpdateField(fieldId, field => field.Disable());
        }

        public ModelSchema EnableField(Guid fieldId)
        {
            return UpdateField(fieldId, field => field.Enable());
        }

        public ModelSchema HideField(Guid fieldId)
        {
            return UpdateField(fieldId, field => field.Show());
        }

        public ModelSchema ShowField(Guid fieldId)
        {
            return UpdateField(fieldId, field => field.Show());
        }

        public ModelSchema SetField(ModelField field)
        {
            Guard.NotNull(field, nameof(field));

            if (fields.Values.Any(f => f.Name == field.Name && f.Id != field.Id))
            {
                throw new DomainValidationException($"A field with name '{field.Name}' already exists.");
            }

            return new ModelSchema(metadata, fields.SetItem(field.Id, field));
        }

        public ModelSchema DeleteField(Guid fieldId)
        {
            Guard.NotEmpty(fieldId, nameof(fieldId));

            if (!fields.ContainsKey(fieldId))
            {
                throw new DomainValidationException($"A field with id {fieldId} does not exist.");
            }

            return new ModelSchema(metadata, fields.Remove(fieldId));
        }

        private ModelSchema UpdateField(Guid fieldId, Func<ModelField, ModelField> updater)
        {
            ModelField field;

            if (!fields.TryGetValue(fieldId, out field))
            {
                throw new DomainValidationException($"Cannot update field with id '{fieldId}'.", "Field does not exist.");
            }

            var newField = updater(field);

            return SetField(newField);
        }

        public async Task ValidateAsync(PropertiesBag data)
        {
            Guard.NotNull(data, nameof(data));

            var errors = new List<string>();

            foreach (var kvp in data.Properties)
            {
                ModelField field;

                if (fieldsByName.TryGetValue(kvp.Key, out field))
                {
                    var newErrors = new List<string>();

                    await field.ValidateAsync(kvp.Value, newErrors);

                    errors.AddRange(newErrors.Select(e => e.Replace("<Field>", "'" + field.Name + "'")));
                }
                else
                {
                    errors.Add($"'{kvp.Key}' is not a known field");
                }
            }

            if (errors.Any())
            {
                throw new DomainValidationException("The data is not valid.", errors);
            }
        }
    }
}