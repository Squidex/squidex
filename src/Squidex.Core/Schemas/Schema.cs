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
using Microsoft.OData.Edm.Library;
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

        public JsonSchema4 BuildSchema(HashSet<Language> languages, Func<string, JsonSchema4, JsonSchema4> schemaResolver)
        {
            Guard.NotEmpty(languages, nameof(languages));
            Guard.NotNull(schemaResolver, nameof(schemaResolver));

            var schemaName = Name.ToPascalCase();

            var schema = new JsonSchema4 { Id = schemaName, Type = JsonObjectType.Object };

            foreach (var field in fieldsByName.Values.Where(x => !x.IsHidden))
            {
                field.AddToSchema(schema, languages, schemaName, schemaResolver);
            }

            return schema;
        }

        public async Task ValidatePartialAsync(ContentData data, IList<ValidationError> errors, HashSet<Language> languages)
        {
            Guard.NotNull(data, nameof(data));
            Guard.NotNull(errors, nameof(errors));

            foreach (var fieldData in data)
            {
                if (!fieldsByName.TryGetValue(fieldData.Key, out Field field))
                {
                    errors.Add(new ValidationError($"{fieldData.Key} is not a known field", fieldData.Key));
                }
                else
                {
                    var fieldErrors = new List<string>();

                    if (field.RawProperties.IsLocalizable)
                    {
                        foreach (var languageValue in fieldData.Value)
                        {
                            if (!Language.TryGetLanguage(languageValue.Key, out Language language))
                            {
                                fieldErrors.Add($"{field.Name} has an invalid language '{languageValue.Key}'");
                            }
                            else if (!languages.Contains(language))
                            {
                                fieldErrors.Add($"{field.Name} has an unsupported language '{languageValue.Key}'");
                            }
                            else
                            {
                                await field.ValidateAsync(languageValue.Value, fieldErrors, language);
                            }
                        }
                    }
                    else
                    {
                        if (fieldData.Value.Keys.Any(x => x != Language.Invariant.Iso2Code))
                        {
                            fieldErrors.Add($"{field.Name} can only contain a single entry for invariant language ({Language.Invariant.Iso2Code})");
                        }

                        if (fieldData.Value.TryGetValue(Language.Invariant.Iso2Code, out JToken value))
                        {
                            await field.ValidateAsync(value, fieldErrors);
                        }
                    }

                    foreach (var error in fieldErrors)
                    {
                        errors.Add(new ValidationError(error, field.Name));
                    }
                }
            }
        }

        public async Task ValidateAsync(ContentData data, IList<ValidationError> errors, HashSet<Language> languages)
        {
            Guard.NotNull(data, nameof(data));
            Guard.NotNull(errors, nameof(errors));
            Guard.NotEmpty(languages, nameof(languages));

            ValidateUnknownFields(data, errors);

            foreach (var field in fieldsByName.Values)
            {
                var fieldErrors = new List<string>();
                var fieldData = data.GetOrCreate(field.Name, k => new ContentFieldData());

                if (field.RawProperties.IsLocalizable)
                {
                    await ValidateLocalizableFieldAsync(languages, fieldData, fieldErrors, field);
                }
                else
                {
                    await ValidateNonLocalizableField(fieldData, fieldErrors, field);
                }

                foreach (var error in fieldErrors)
                {
                    errors.Add(new ValidationError(error, field.Name));
                }
            }
        }

        private void ValidateUnknownFields(ContentData data, IList<ValidationError> errors)
        {
            foreach (var fieldData in data)
            {
                if (!fieldsByName.ContainsKey(fieldData.Key))
                {
                    errors.Add(new ValidationError($"{fieldData.Key} is not a known field", fieldData.Key));
                }
            }
        }

        private static async Task ValidateLocalizableFieldAsync(HashSet<Language> languages, ContentFieldData fieldData, List<string> fieldErrors, Field field)
        {
            foreach (var valueLanguage in fieldData.Keys)
            {
                if (!Language.TryGetLanguage(valueLanguage, out Language language))
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
                var value = fieldData.GetOrCreate(language.Iso2Code, k => JValue.CreateNull());

                await field.ValidateAsync(value, fieldErrors, language);
            }
        }

        private static async Task ValidateNonLocalizableField(ContentFieldData fieldData, List<string> fieldErrors, Field field)
        {
            if (fieldData.Keys.Any(x => x != Language.Invariant.Iso2Code))
            {
                fieldErrors.Add($"{field.Name} can only contain a single entry for invariant language ({Language.Invariant.Iso2Code})");
            }

            var value = fieldData.GetOrCreate(Language.Invariant.Iso2Code, k => JValue.CreateNull());

            await field.ValidateAsync(value, fieldErrors);
        }

        public void Enrich(ContentData data, HashSet<Language> languages)
        {
            Guard.NotNull(data, nameof(data));
            Guard.NotEmpty(languages, nameof(languages));

            foreach (var field in fieldsByName.Values)
            {
                var fieldData = data.GetOrCreate(field.Name, k => new ContentFieldData());

                if (field.RawProperties.IsLocalizable)
                {
                    foreach (var language in languages)
                    {
                        field.Enrich(fieldData, language);
                    }
                }
                else
                {
                    field.Enrich(fieldData, Language.Invariant);
                }

                if (fieldData.Count > 0)
                {
                    data.AddField(field.Name, fieldData);
                }
            }
        }
    }
}