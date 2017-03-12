// ==========================================================================
//  ContentValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.Core.Contents;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Core
{
    public sealed class ContentValidator
    {
        private readonly Schema schema;
        private readonly HashSet<Language> languages;
        private readonly List<ValidationError> errors = new List<ValidationError>();

        public ContentValidator(Schema schema, HashSet<Language> languages)
        {
            Guard.NotNull(schema, nameof(schema));
            Guard.NotNull(languages, nameof(languages));

            this.schema = schema;

            this.languages = languages;
        }

        public IReadOnlyList<ValidationError> Errors
        {
            get { return errors; }
        }

        public async Task ValidatePartialAsync(ContentData data)
        {
            Guard.NotNull(data, nameof(data));

            foreach (var fieldData in data)
            {
                var fieldName = fieldData.Key;

                if (!schema.FieldsByName.TryGetValue(fieldData.Key, out Field field))
                {
                    AddError("<FIELD> is not a known field", fieldName);
                }
                else
                {
                    if (field.RawProperties.IsLocalizable)
                    {
                        await ValidateLocalizableFieldPartialAsync(field, fieldData.Value);
                    }
                    else
                    {
                        await ValidateNonLocalizableFieldPartialAsync(field, fieldData.Value);
                    }
                }
            }
        }

        private async Task ValidateLocalizableFieldPartialAsync(Field field, ContentFieldData fieldData)
        {
            foreach (var languageValue in fieldData)
            {
                if (!Language.TryGetLanguage(languageValue.Key, out Language language))
                {
                    AddError($"<FIELD> has an invalid language '{languageValue.Key}'", field);
                }
                else if (!languages.Contains(language))
                {
                    AddError($"<FIELD> has an unsupported language '{languageValue.Key}'", field);
                }
                else
                {
                    await ValidateAsync(field, languageValue.Value, language);
                }
            }
        }

        private async Task ValidateNonLocalizableFieldPartialAsync(Field field, ContentFieldData fieldData)
        {
            if (fieldData.Keys.Any(x => x != Language.Invariant.Iso2Code))
            {
                AddError($"<FIELD> can only contain a single entry for invariant language ({Language.Invariant.Iso2Code})", field);
            }

            if (fieldData.TryGetValue(Language.Invariant.Iso2Code, out JToken value))
            {
                await ValidateAsync(field, value);
            }
        }

        public async Task ValidateAsync(ContentData data)
        {
            Guard.NotNull(data, nameof(data));

            ValidateUnknownFields(data);

            foreach (var field in schema.FieldsByName.Values)
            {
                var fieldData = data.GetOrCreate(field.Name, k => new ContentFieldData());

                if (field.RawProperties.IsLocalizable)
                {
                    await ValidateLocalizableFieldAsync(field, fieldData);
                }
                else
                {
                    await ValidateNonLocalizableField(field, fieldData);
                }
            }
        }

        private void ValidateUnknownFields(ContentData data)
        {
            foreach (var fieldData in data)
            {
                if (!schema.FieldsByName.ContainsKey(fieldData.Key))
                {
                    AddError("<FIELD> is not a known field", fieldData.Key);
                }
            }
        }

        private async Task ValidateLocalizableFieldAsync(Field field, ContentFieldData fieldData)
        {
            foreach (var valueLanguage in fieldData.Keys)
            {
                if (!Language.TryGetLanguage(valueLanguage, out Language language))
                {
                    AddError($"<FIELD> has an invalid language '{valueLanguage}'", field);
                }
                else if (!languages.Contains(language))
                {
                    AddError($"<FIELD> has an unsupported language '{valueLanguage}'", field);
                }
            }

            foreach (var language in languages)
            {
                var value = fieldData.GetOrCreate(language.Iso2Code, k => JValue.CreateNull());

                await ValidateAsync(field, value, language);
            }
        }

        private async Task ValidateNonLocalizableField(Field field, ContentFieldData fieldData)
        {
            if (fieldData.Keys.Any(x => x != Language.Invariant.Iso2Code))
            {
                AddError($"<FIELD> can only contain a single entry for invariant language ({Language.Invariant.Iso2Code})", field);
            }

            var value = fieldData.GetOrCreate(Language.Invariant.Iso2Code, k => JValue.CreateNull());

            await ValidateAsync(field, value);
        }

        private Task ValidateAsync(Field field, JToken value, Language language = null)
        {
            return field.ValidateAsync(value, m => AddError(m, field, language));
        }

        private void AddError(string message, Field field, Language language = null)
        {
            var displayName = !string.IsNullOrWhiteSpace(field.RawProperties.Label) ? field.RawProperties.Label : field.Name;

            if (language != null)
            {
                displayName += $" ({language.Iso2Code})";
            }

            message = message.Replace("<FIELD>", displayName);

            errors.Add(new ValidationError(message, field.Name));
        }

        private void AddError(string message, string fieldName)
        {
            message = message.Replace("<FIELD>", fieldName);

            errors.Add(new ValidationError(message, fieldName));
        }
    }
}
