// ==========================================================================
//  ContentEnricher.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json.Linq;
using Squidex.Core.Contents;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Core
{
    public sealed class ContentEnricher
    {
        private readonly Schema schema;
        private readonly LanguagesConfig languagesConfig;

        public ContentEnricher(LanguagesConfig languagesConfig, Schema schema)
        {
            Guard.NotNull(schema, nameof(schema));
            Guard.NotNull(languagesConfig, nameof(languagesConfig));

            this.schema = schema;

            this.languagesConfig = languagesConfig;
        }

        public void Enrich(ContentData data)
        {
            Guard.NotNull(data, nameof(data));

            foreach (var field in schema.FieldsByName.Values)
            {
                var fieldData = data.GetOrCreate(field.Name, k => new ContentFieldData());

                if (field.RawProperties.IsLocalizable)
                {
                    foreach (var languageConfig in languagesConfig)
                    {
                        Enrich(field, fieldData, languageConfig.Language);
                    }
                }
                else
                {
                    Enrich(field, fieldData, Language.Invariant);
                }

                if (fieldData.Count > 0)
                {
                    data.AddField(field.Name, fieldData);
                }
            }
        }

        private static void Enrich(Field field, ContentFieldData fieldData, Language language)
        {
            Guard.NotNull(fieldData, nameof(fieldData));

            var defaultValue = field.RawProperties.GetDefaultValue();

            if (field.RawProperties.IsRequired || defaultValue.IsNull())
            {
                return;
            }

            if (!fieldData.TryGetValue(language, out JToken value) || value == null || value.Type == JTokenType.Null)
            {
                fieldData.AddValue(language, defaultValue);
            }
        }
    }
}
