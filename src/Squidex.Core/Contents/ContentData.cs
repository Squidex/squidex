// ==========================================================================
//  ContentData.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Core.Contents
{
    public sealed class ContentData : Dictionary<string, ContentFieldData>
    {
        public ContentData()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public ContentData AddField(string fieldName, ContentFieldData data)
        {
            Guard.ValidPropertyName(fieldName, nameof(fieldName));

            this[fieldName] = data;

            return this;
        }

        public ContentData ToNameModel(Schema schema)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new ContentData();

            foreach (var fieldValue in this)
            {
                long fieldId;

                Field field;

                if (!long.TryParse(fieldValue.Key, out fieldId) || !schema.Fields.TryGetValue(fieldId, out field))
                {
                    continue;
                }

                result[field.Name] = fieldValue.Value;
            }

            return result;
        }

        public ContentData ToIdModel(Schema schema)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new ContentData();

            foreach (var fieldValue in this)
            {
                Field field;

                if (!schema.FieldsByName.TryGetValue(fieldValue.Key, out field))
                {
                    continue;
                }

                result[field.Id.ToString()] = fieldValue.Value;
            }

            return result;
        }

        public ContentData ToApiModel(Schema schema, IReadOnlyCollection<Language> languages, Language masterLanguage, bool excludeHidden = true)
        {
            Guard.NotNull(schema, nameof(schema));
            Guard.NotNull(languages, nameof(languages));
            Guard.NotNull(masterLanguage, nameof(masterLanguage));

            var invariantCode = Language.Invariant.Iso2Code;

            var result = new ContentData();

            foreach (var fieldValue in this)
            {
                Field field;
                
                if (!schema.FieldsByName.TryGetValue(fieldValue.Key, out field) || (excludeHidden && field.IsHidden))
                {
                    continue;
                }

                var fieldResult = new ContentFieldData();
                var fieldValues = fieldValue.Value;

                if (field.RawProperties.IsLocalizable)
                {
                    foreach (var language in languages)
                    {
                        var languageCode = language.Iso2Code;

                        JToken value;

                        if (fieldValues.TryGetValue(languageCode, out value))
                        {
                            fieldResult.Add(languageCode, value);
                        }
                    }
                }
                else
                {
                    JToken value;

                    if (fieldValues.TryGetValue(invariantCode, out value))
                    {
                        fieldResult.Add(invariantCode, value);
                    }
                    else if (fieldValues.TryGetValue(masterLanguage.Iso2Code, out value))
                    {
                        fieldResult.Add(invariantCode, value);
                    }
                    else if (fieldValues.Count > 0)
                    {
                        fieldResult.Add(invariantCode, fieldValues.Values.First());
                    }
                }

                result.Add(field.Name, fieldResult);
            }

            return result;
        }
    }
}
