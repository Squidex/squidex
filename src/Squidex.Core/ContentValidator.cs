// ==========================================================================
//  ContentValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
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
        private readonly LanguagesConfig languagesConfig;
        private readonly List<ValidationError> errors = new List<ValidationError>();

        public IReadOnlyList<ValidationError> Errors
        {
            get { return errors; }
        }

        public ContentValidator(Schema schema, LanguagesConfig languagesConfig)
        {
            Guard.NotNull(schema, nameof(schema));
            Guard.NotNull(languagesConfig, nameof(languagesConfig));

            this.schema = schema;

            this.languagesConfig = languagesConfig;
        }

        public async Task ValidatePartialAsync(ContentData data)
        {
            Guard.NotNull(data, nameof(data));

            foreach (var fieldData in data)
            {
                var fieldName = fieldData.Key;

                if (!schema.FieldsByName.TryGetValue(fieldData.Key, out Field field))
                {
                    errors.AddError("<FIELD> is not a known field", fieldName);
                }
                else
                {
                    if (field.RawProperties.IsLocalizable)
                    {
                        await ValidateFieldPartialAsync(field, fieldData.Value, languagesConfig);
                    }
                    else
                    {
                        await ValidateFieldPartialAsync(field, fieldData.Value, LanguagesConfig.Invariant);
                    }
                }
            }
        }

        private async Task ValidateFieldPartialAsync(Field field, ContentFieldData fieldData, LanguagesConfig languages)
        {
            foreach (var languageValue in fieldData)
            {
                if (!Language.TryGetLanguage(languageValue.Key, out var language))
                {
                    errors.AddError($"<FIELD> has an invalid language '{languageValue.Key}'", field);
                }
                else if (!languages.TryGetConfig(language, out var languageConfig))
                {
                    errors.AddError($"<FIELD> has an unsupported language '{languageValue.Key}'", field);
                }
                else
                {
                    var config = languageConfig;

                    await field.ValidateAsync(languageValue.Value, config.IsOptional, m => errors.AddError(m, field, config.Language));
                }
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
                    await ValidateFieldAsync(field, fieldData, languagesConfig);
                }
                else
                {
                    await ValidateFieldAsync(field, fieldData, LanguagesConfig.Invariant);
                }
            }
        }

        private void ValidateUnknownFields(ContentData data)
        {
            foreach (var fieldData in data)
            {
                if (!schema.FieldsByName.ContainsKey(fieldData.Key))
                {
                    errors.AddError("<FIELD> is not a known field", fieldData.Key);
                }
            }
        }

        private async Task ValidateFieldAsync(Field field, ContentFieldData fieldData, LanguagesConfig languages)
        {
            foreach (var valueLanguage in fieldData.Keys)
            {
                if (!Language.TryGetLanguage(valueLanguage, out Language language))
                {
                    errors.AddError($"<FIELD> has an invalid language '{valueLanguage}'", field);
                }
                else if (!languages.Contains(language))
                {
                    errors.AddError($"<FIELD> has an unsupported language '{valueLanguage}'", field);
                }
            }

            foreach (var languageConfig in languages)
            {
                var config = languageConfig;
                var value = fieldData.GetOrCreate(config.Language, k => JValue.CreateNull());

                await field.ValidateAsync(value, config.IsOptional, m => errors.AddError(m, field, config.Language));
            }
        }
    }
}
