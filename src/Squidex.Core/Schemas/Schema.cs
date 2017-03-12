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
using Microsoft.OData.Edm.Library;
using NJsonSchema;
using Squidex.Infrastructure;

// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable InvertIf

namespace Squidex.Core.Schemas
{
    public sealed class Schema : CloneableBase
    {
        private readonly string name;
        private readonly SchemaProperties properties;
        private readonly ImmutableDictionary<long, Field> fieldsById;
        private readonly ImmutableDictionary<string, Field> fieldsByName;
        private readonly bool isPublished;

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

        public ImmutableDictionary<string, Field> FieldsByName
        {
            get { return fieldsByName; }
        }

        public SchemaProperties Properties
        {
            get { return properties; }
        }

        public Schema(string name, bool isPublished, SchemaProperties properties, ImmutableDictionary<long, Field> fields)
        {
            Guard.NotNull(fields, nameof(fields));
            Guard.NotNull(properties, nameof(properties));
            Guard.ValidSlug(name, nameof(name));

            fieldsById = fields;
            fieldsByName = fields.Values.ToImmutableDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

            this.name = name;

            this.properties = properties;
            this.properties.Freeze();

            this.isPublished = isPublished;
        }

        public static Schema Create(string name, SchemaProperties newProperties)
        {
            if (!name.IsSlug())
            {
                var error = new ValidationError("Name must be a valid slug", "Name");

                throw new ValidationException("Cannot create a new schema", error);
            }

            return new Schema(name, false, newProperties, ImmutableDictionary<long, Field>.Empty);
        }

        public Schema Update(SchemaProperties newProperties)
        {
            Guard.NotNull(newProperties, nameof(newProperties));

            return new Schema(name, isPublished, newProperties, fieldsById);
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
            return new Schema(name, isPublished, properties, fieldsById.Remove(fieldId));
        }

        public Schema Publish()
        {
            if (isPublished)
            {
                throw new DomainException("Schema is already published");
            }

            return new Schema(name, true, properties, fieldsById);
        }

        public Schema Unpublish()
        {
            if (!isPublished)
            {
                throw new DomainException("Schema is not published");
            }

            return new Schema(name, false, properties, fieldsById);
        }

        public Schema AddOrUpdateField(Field field)
        {
            Guard.NotNull(field, nameof(field));

            if (fieldsById.Values.Any(f => f.Name == field.Name && f.Id != field.Id))
            {
                throw new ValidationException($"A field with name '{field.Name}' already exists.");
            }

            return new Schema(name, isPublished, properties, fieldsById.SetItem(field.Id, field));
        }

        public Schema UpdateField(long fieldId, Func<Field, Field> updater)
        {
            Guard.NotNull(updater, nameof(updater));

            if (!fieldsById.TryGetValue(fieldId, out Field field))
            {
                throw new DomainObjectNotFoundException(fieldId.ToString(), "Fields", typeof(Field));
            }

            var newField = updater(field);

            return AddOrUpdateField(newField);
        }

        public EdmComplexType BuildEdmType(HashSet<Language> languages, Func<EdmComplexType, EdmComplexType> typeResolver)
        {
            Guard.NotEmpty(languages, nameof(languages));
            Guard.NotNull(typeResolver, nameof(typeResolver));

            var schemaName = Name.ToPascalCase();

            var edmType = new EdmComplexType("Squidex", schemaName);

            foreach (var field in fieldsByName.Values.Where(x => !x.IsHidden))
            {
                field.AddToEdmType(edmType, languages, schemaName, typeResolver);
            }

            return edmType;
        }

        public JsonSchema4 BuildJsonSchema(HashSet<Language> languages, Func<string, JsonSchema4, JsonSchema4> schemaResolver)
        {
            Guard.NotEmpty(languages, nameof(languages));
            Guard.NotNull(schemaResolver, nameof(schemaResolver));

            var schemaName = Name.ToPascalCase();

            var schema = new JsonSchema4 { Type = JsonObjectType.Object };

            foreach (var field in fieldsByName.Values.Where(x => !x.IsHidden))
            {
                field.AddToJsonSchema(schema, languages, schemaName, schemaResolver);
            }

            return schema;
        }
    }
}