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

// ReSharper disable InvertIf

namespace Squidex.Core.Contents
{
    public sealed class ContentData : Dictionary<string, ContentFieldData>, IEquatable<ContentData>
    {
        public ContentData()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public ContentData(IDictionary<string, ContentFieldData> copy)
            : base(copy, StringComparer.OrdinalIgnoreCase)
        {
        }

        public ContentData AddField(string fieldName, ContentFieldData data)
        {
            Guard.ValidPropertyName(fieldName, nameof(fieldName));

            this[fieldName] = data;

            return this;
        }

        public ContentData MergeInto(ContentData other)
        {
            Guard.NotNull(other, nameof(other));

            var result = new ContentData(this);

            if (ReferenceEquals(other, this))
            {
                return result;
            }

            foreach (var otherValue in other)
            {
                var fieldValue = result.GetOrAdd(otherValue.Key, x => new ContentFieldData());

                foreach (var value in otherValue.Value)
                {
                    fieldValue[value.Key] = value.Value;
                }
            }

            return result;
        }

        public ContentData ToCleaned()
        {
            var result = new ContentData();

            foreach (var fieldValue in this.Where(x => x.Value != null))
            {
                var resultValue = new ContentFieldData();

                foreach (var languageValue in fieldValue.Value.Where(x => x.Value != null && x.Value.Type != JTokenType.Null))
                {
                    resultValue[languageValue.Key] = languageValue.Value;
                }

                if (resultValue.Count > 0)
                {
                    result[fieldValue.Key] = resultValue;
                }
            }

            return result;
        }

        public ContentData ToIdModel(Schema schema)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new ContentData();

            foreach (var fieldValue in this)
            {
                if (!schema.FieldsByName.TryGetValue(fieldValue.Key, out Field field))
                {
                    continue;
                }

                result[field.Id.ToString()] = fieldValue.Value;
            }

            return result;
        }

        public ContentData ToNameModel(Schema schema)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new ContentData();

            foreach (var fieldValue in this)
            {
                if (!long.TryParse(fieldValue.Key, out long fieldId) || !schema.Fields.TryGetValue(fieldId, out Field field))
                {
                    continue;
                }

                result[field.Name] = fieldValue.Value;
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
                if (!schema.FieldsByName.TryGetValue(fieldValue.Key, out Field field) || (excludeHidden && field.IsHidden))
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

                        if (fieldValues.TryGetValue(languageCode, out JToken value))
                        {
                            fieldResult.Add(languageCode, value);
                        }
                    }
                }
                else
                {
                    if (fieldValues.TryGetValue(invariantCode, out JToken value))
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

        public object ToLanguageModel(IReadOnlyCollection<Language> languagePreferences = null)
        {
            if (languagePreferences == null)
            {
                return this;
            }

            var result = new Dictionary<string, JToken>();

            foreach (var fieldValue in this)
            {
                var fieldValues = fieldValue.Value;

                foreach (var language in languagePreferences)
                {
                    var languageCode = language.Iso2Code;

                    if (fieldValues.TryGetValue(languageCode, out JToken value) && value != null)
                    {
                        result[fieldValue.Key] = value;

                        break;
                    }
                }
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ContentData);
        }

        public bool Equals(ContentData other)
        {
            return other != null && (ReferenceEquals(this, other) || this.EqualsDictionary(other));
        }

        public override int GetHashCode()
        {
            return this.DictionaryHashCode();
        }
    }
}
