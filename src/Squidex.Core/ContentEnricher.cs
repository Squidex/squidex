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
using System.Collections.Generic;

namespace Squidex.Core
{
    public sealed class ContentEnricher
    {
        private readonly Schema schema;
        private readonly HashSet<Language> languages;

        public ContentEnricher(HashSet<Language> languages, Schema schema)
        {
            Guard.NotNull(schema, nameof(schema));
            Guard.NotNull(languages, nameof(languages));

            this.schema = schema;

            this.languages = languages;
        }

        public void Enrich(ContentData data)
        {
            Guard.NotNull(data, nameof(data));
            Guard.NotEmpty(languages, nameof(languages));

            foreach (var field in schema.FieldsByName.Values)
            {
                var fieldData = data.GetOrCreate(field.Name, k => new ContentFieldData());

                if (field.RawProperties.IsLocalizable)
                {
                    foreach (var language in languages)
                    {
                        Enrich(field, fieldData, language);
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
            Guard.NotNull(language, nameof(language));

            var defaultValue = field.RawProperties.GetDefaultValue();

            if (field.RawProperties.IsRequired || defaultValue.IsNull())
            {
                return;
            }

            if (!fieldData.TryGetValue(language.Iso2Code, out JToken value) || value == null || value.Type == JTokenType.Null)
            {
                fieldData.AddValue(language.Iso2Code, defaultValue);
            }
        }
    }
}
