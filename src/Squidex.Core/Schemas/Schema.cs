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
using Newtonsoft.Json.Linq;
using NJsonSchema;
using Squidex.Core.Contents;
using Squidex.Infrastructure;
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression

// ReSharper disable InvertIf

namespace Squidex.Core.Schemas
{
    public sealed class Schema : Cloneable
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

            Field field;

            if (!fieldsById.TryGetValue(fieldId, out field))
            {
                throw new DomainObjectNotFoundException(fieldId.ToString(), "Fields", typeof(Field));
            }

            var newField = updater(field);

            return AddOrUpdateField(newField);
        }

        public JsonSchema4 BuildSchema(HashSet<Language> languages, Func<string, JsonSchema4, JsonSchema4> schemaResolver)
        {
            Guard.NotEmpty(languages, nameof(languages));
            Guard.NotNull(schemaResolver, nameof(schemaResolver));

            var schema = new JsonSchema4 { Id = Name, Type = JsonObjectType.Object };

            foreach (var field in fieldsByName.Values.Where(x => !x.IsHidden))
            {
                field.AddToSchema(schema, languages, Name, schemaResolver);
            }

            return schema;
        }

        public async Task ValidateAsync(ContentData data, IList<ValidationError> errors, HashSet<Language> languages)
        {
            Guard.NotNull(data, nameof(data));
            Guard.NotNull(errors, nameof(errors));
            Guard.NotEmpty(languages, nameof(languages));

            foreach (var fieldValue in data.Fields)
            {
                if (!fieldsByName.ContainsKey(fieldValue.Key))
                {
                    errors.Add(new ValidationError($"{fieldValue.Key} is not a known field", fieldValue.Key));
                }
            }

            foreach (var field in fieldsByName.Values)
            {
                var fieldErrors = new List<string>();

                var fieldData = data.Fields.GetOrDefault(field.Name) ?? ContentFieldData.Empty;
                
                if (field.RawProperties.IsLocalizable)
                {
                    foreach (var valueLanguage in fieldData.ValueByLanguage.Keys)
                    {
                        Language language;

                        if (!Language.TryGetLanguage(valueLanguage, out language))
                        {
                            fieldErrors.Add($"{field.Name} has an invalid language '{valueLanguage}'");
                        }
                        else if (!languages.Contains(language))
                        {
                            fieldErrors.Add($"{field.Name} has an unsupported language '{valueLanguage}'");
                        }
                    }

                    foreach (var language in languages)
                    {
                        var value = fieldData.ValueByLanguage.GetValueOrDefault(language.Iso2Code, JValue.CreateNull());

                        await field.ValidateAsync(value, fieldErrors, language);
                    }
                }
                else
                {
                    if (fieldData.ValueByLanguage.Keys.Any(x => x != "iv"))
                    {
                        fieldErrors.Add($"{field.Name} can only contain a single entry for invariant language (iv)");
                    }

                    var value = fieldData.ValueByLanguage.GetValueOrDefault("iv", JValue.CreateNull());

                    await field.ValidateAsync(value, fieldErrors);
                }

                foreach (var error in fieldErrors)
                {
                    errors.Add(new ValidationError(error, field.Name));
                }
            }
        }
    }
}