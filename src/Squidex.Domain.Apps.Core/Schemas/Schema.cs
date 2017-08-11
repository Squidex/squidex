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
using Microsoft.OData.Edm;
using NJsonSchema;
using Squidex.Infrastructure;

// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable InvertIf

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class Schema : CloneableBase
    {
        private readonly string name;
        private readonly SchemaProperties properties;
        private readonly ImmutableList<Field> fields;
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

        public ImmutableList<Field> Fields
        {
            get { return fields; }
        }

        public ImmutableDictionary<long, Field> FieldsById
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

        public Schema(string name, bool isPublished, SchemaProperties properties, ImmutableList<Field> fields)
        {
            Guard.NotNull(fields, nameof(fields));
            Guard.NotNull(properties, nameof(properties));
            Guard.ValidSlug(name, nameof(name));

            fieldsById = fields.ToImmutableDictionary(x => x.Id);
            fieldsByName = fields.ToImmutableDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

            this.name = name;

            this.fields = fields;

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

            return new Schema(name, false, newProperties, ImmutableList<Field>.Empty);
        }

        public Schema Update(SchemaProperties newProperties)
        {
            Guard.NotNull(newProperties, nameof(newProperties));

            return new Schema(name, isPublished, newProperties, fields);
        }

        public Schema UpdateField(long fieldId, FieldProperties newProperties)
        {
            return UpdateField(fieldId, field => field.Update(newProperties));
        }

        public Schema LockField(long fieldId)
        {
            return UpdateField(fieldId, field => field.Lock());
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
            if (fieldsById.TryGetValue(fieldId, out var field) && field.IsLocked)
            {
                throw new DomainException($"Field {fieldId} is locked.");
            }

            return new Schema(name, isPublished, properties, fields.Where(x => x.Id != fieldId).ToImmutableList());
        }

        public Schema Publish()
        {
            if (isPublished)
            {
                throw new DomainException("Schema is already published");
            }

            return new Schema(name, true, properties, fields);
        }

        public Schema Unpublish()
        {
            if (!isPublished)
            {
                throw new DomainException("Schema is not published");
            }

            return new Schema(name, false, properties, fields);
        }

        public Schema ReorderFields(List<long> ids)
        {
            Guard.NotNull(ids, nameof(ids));

            if (ids.Count != fields.Count || ids.Any(x => !fieldsById.ContainsKey(x)))
            {
                throw new ArgumentException("Ids must cover all fields.", nameof(ids));
            }

            var newFields = fields.OrderBy(f => ids.IndexOf(f.Id)).ToImmutableList();

            return new Schema(name, isPublished, properties, newFields);
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

        public Schema AddOrUpdateField(Field field)
        {
            Guard.NotNull(field, nameof(field));

            if (fieldsById.Values.Any(f => f.Name == field.Name && f.Id != field.Id))
            {
                throw new ValidationException($"A field with name '{field.Name}' already exists.");
            }

            ImmutableList<Field> newFields;

            if (fieldsById.ContainsKey(field.Id))
            {
                newFields = fields.Select(f => f.Id == field.Id ? field : f).ToImmutableList();
            }
            else
            {
                newFields = fields.Add(field);
            }

            return new Schema(name, isPublished, properties, newFields);
        }

        public EdmComplexType BuildEdmType(PartitionResolver partitionResolver, Func<EdmComplexType, EdmComplexType> typeResolver)
        {
            Guard.NotNull(typeResolver, nameof(typeResolver));
            Guard.NotNull(partitionResolver, nameof(partitionResolver));

            var schemaName = Name.ToPascalCase();

            var edmType = new EdmComplexType("Squidex", schemaName);

            foreach (var field in fieldsByName.Values.Where(x => !x.IsHidden))
            {
                field.AddToEdmType(edmType, partitionResolver, schemaName, typeResolver);
            }

            return edmType;
        }

        public JsonSchema4 BuildJsonSchema(PartitionResolver partitionResolver, Func<string, JsonSchema4, JsonSchema4> schemaResolver)
        {
            Guard.NotNull(schemaResolver, nameof(schemaResolver));
            Guard.NotNull(partitionResolver, nameof(partitionResolver));

            var schemaName = Name.ToPascalCase();

            var schema = new JsonSchema4 { Type = JsonObjectType.Object };

            foreach (var field in fieldsByName.Values.Where(x => !x.IsHidden))
            {
                field.AddToJsonSchema(schema, partitionResolver, schemaName, schemaResolver);
            }

            return schema;
        }
    }
}