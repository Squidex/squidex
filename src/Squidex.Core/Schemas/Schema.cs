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
using Squidex.Infrastructure;

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

        public async Task ValidateAsync(JObject data, IList<ValidationError> errors, HashSet<Language> languages)
        {
            Guard.NotNull(data, nameof(data));
            Guard.NotNull(errors, nameof(errors));
            Guard.NotEmpty(languages, nameof(languages));

            AppendEmptyFields(data, languages);

            foreach (var property in data.Properties())
            {
                var fieldErrors = new List<string>();

                Field field;

                if (fieldsByName.TryGetValue(property.Name, out field))
                {
                    if (field.RawProperties.IsLocalizable)
                    {
                        var languageObject = property.Value as JObject;

                        if (languageObject == null)
                        {
                            fieldErrors.Add($"{property.Name} is localizable and must be an object");
                        }
                        else
                        {
                            AppendEmptyLanguages(languageObject, languages);

                            foreach (var languageProperty in languageObject.Properties())
                            {
                                Language language;

                                if (!Language.TryGetLanguage(languageProperty.Name, out language))
                                {
                                    fieldErrors.Add($"{property.Name} has an invalid language '{languageProperty.Name}'");
                                    continue;
                                }

                                if (!languages.Contains(language))
                                {
                                    fieldErrors.Add($"{property.Name} has an unsupported language '{languageProperty.Name}'");
                                    continue;
                                }

                                await field.ValidateAsync(languageProperty.Value, fieldErrors, language);
                            }
                        }
                    }
                    else
                    {
                        await field.ValidateAsync(property.Value, fieldErrors);
                    }
                }
                else
                {
                    fieldErrors.Add($"{property.Name} is not a known field");
                }

                foreach (var error in fieldErrors)
                {
                    errors.Add(new ValidationError(error, property.Name));
                }
            }
        }

        private void AppendEmptyLanguages(JObject data, IEnumerable<Language> languages)
        {
            var nullJson = JValue.CreateNull();

            foreach (var language in languages)
            {
                if (data.GetValue(language.Iso2Code, StringComparison.OrdinalIgnoreCase) == null)
                {
                    data.Add(new JProperty(language.Iso2Code, nullJson));
                }
            }
        }

        private void AppendEmptyFields(JObject data, HashSet<Language> languages)
        {
            var nullJson = JValue.CreateNull();

            foreach (var field in fieldsByName.Values)
            {
                if (data.GetValue(field.Name, StringComparison.OrdinalIgnoreCase) == null)
                {
                    JToken value = nullJson;

                    if (field.RawProperties.IsLocalizable)
                    {
                        value = new JObject(languages.Select(x => new JProperty(x.Iso2Code, nullJson)).OfType<object>().ToArray());
                    }

                    data.Add(new JProperty(field.Name, value));
                }
            }
        }
    }
}