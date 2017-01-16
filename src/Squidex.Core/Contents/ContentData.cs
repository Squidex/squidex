// ==========================================================================
//  ContentData.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json.Linq;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Core.Contents
{
    public sealed class ContentData
    {
        private readonly ImmutableDictionary<string, ContentFieldData> fields;

        public static readonly ContentData Empty = new ContentData(ImmutableDictionary<string, ContentFieldData>.Empty.WithComparers (StringComparer.OrdinalIgnoreCase));

        public ImmutableDictionary<string, ContentFieldData> Fields
        {
            get { return fields; }
        }

        public ContentData(ImmutableDictionary<string, ContentFieldData> fields)
        {
            Guard.NotNull(fields, nameof(fields));

            this.fields = fields;
        }

        public ContentData AddField(string fieldName, ContentFieldData data)
        {
            Guard.ValidPropertyName(fieldName, nameof(fieldName));

            return new ContentData(Fields.Add(fieldName, data));
        }

        public static ContentData FromApiRequest(Dictionary<string, Dictionary<string, JToken>> request)
        {
            Guard.NotNull(request, nameof(request));

            return new ContentData(request.ToImmutableDictionary(x => x.Key, x => new ContentFieldData(x.Value.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase)), StringComparer.OrdinalIgnoreCase));
        }

        public Dictionary<string, Dictionary<string, JToken>> ToApiResponse(Schema schema, IReadOnlyCollection<Language> languages, Language masterLanguage)
        {
            Guard.NotNull(schema, nameof(schema));
            Guard.NotNull(languages, nameof(languages));
            Guard.NotNull(masterLanguage, nameof(masterLanguage));

            var invariantCode = Language.Invariant.Iso2Code;

            var result = new Dictionary<string, Dictionary<string, JToken>>();

            foreach (var fieldValue in fields)
            {
                Field field;
                
                if (!schema.FieldsByName.TryGetValue(fieldValue.Key, out field))
                {
                    continue;
                }

                var fieldResult = new Dictionary<string, JToken>();
                var fieldValues = fieldValue.Value.ValueByLanguage;

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
