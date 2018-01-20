// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.ConvertContent
{
    public static class ContentConverter
    {
        public static NamedContentData ToNameModel(this IdContentData source, Schema schema, bool decodeJsonField)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new NamedContentData();

            foreach (var fieldValue in source)
            {
                if (!schema.FieldsById.TryGetValue(fieldValue.Key, out var field))
                {
                    continue;
                }

                if (decodeJsonField && field is JsonField)
                {
                    var encodedValue = new ContentFieldData();

                    foreach (var partitionValue in fieldValue.Value)
                    {
                        if (partitionValue.Value.IsNull())
                        {
                            encodedValue[partitionValue.Key] = null;
                        }
                        else
                        {
                            var value = Encoding.UTF8.GetString(Convert.FromBase64String(partitionValue.Value.ToString()));

                            encodedValue[partitionValue.Key] = JToken.Parse(value);
                        }
                    }

                    result[field.Name] = encodedValue;
                }
                else
                {
                    result[field.Name] = fieldValue.Value;
                }
            }

            return result;
        }

        public static IdContentData ToIdModel(this NamedContentData content, Schema schema, bool encodeJsonField)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new IdContentData();

            foreach (var fieldValue in content)
            {
                if (!schema.FieldsByName.TryGetValue(fieldValue.Key, out var field))
                {
                    continue;
                }

                var fieldId = field.Id;

                if (encodeJsonField && field is JsonField)
                {
                    var encodedValue = new ContentFieldData();

                    foreach (var partitionValue in fieldValue.Value)
                    {
                        if (partitionValue.Value.IsNull())
                        {
                            encodedValue[partitionValue.Key] = null;
                        }
                        else
                        {
                            var value = Convert.ToBase64String(Encoding.UTF8.GetBytes(partitionValue.Value.ToString()));

                            encodedValue[partitionValue.Key] = value;
                        }
                    }

                    result[fieldId] = encodedValue;
                }
                else
                {
                    result[fieldId] = fieldValue.Value;
                }
            }

            return result;
        }

        public static NamedContentData ToApiModel(this NamedContentData content, Schema schema, LanguagesConfig languagesConfig, bool excludeHidden = true)
        {
            Guard.NotNull(schema, nameof(schema));
            Guard.NotNull(languagesConfig, nameof(languagesConfig));

            var codeForInvariant = InvariantPartitioning.Instance.Master.Key;
            var codeForMasterLanguage = languagesConfig.Master.Language.Iso2Code;

            var result = new NamedContentData();

            foreach (var fieldValue in content)
            {
                if (!schema.FieldsByName.TryGetValue(fieldValue.Key, out var field) || (excludeHidden && field.IsHidden))
                {
                    continue;
                }

                var fieldResult = new ContentFieldData();
                var fieldValues = fieldValue.Value;

                if (field.Partitioning.Equals(Partitioning.Language))
                {
                    foreach (var languageConfig in languagesConfig)
                    {
                        var languageCode = languageConfig.Key;

                        if (fieldValues.TryGetValue(languageCode, out var value))
                        {
                            fieldResult.Add(languageCode, value);
                        }
                        else if (languageConfig == languagesConfig.Master && fieldValues.TryGetValue(codeForInvariant, out value))
                        {
                            fieldResult.Add(languageCode, value);
                        }
                    }
                }
                else
                {
                    if (fieldValues.TryGetValue(codeForInvariant, out var value))
                    {
                        fieldResult.Add(codeForInvariant, value);
                    }
                    else if (fieldValues.TryGetValue(codeForMasterLanguage, out value))
                    {
                        fieldResult.Add(codeForInvariant, value);
                    }
                    else if (fieldValues.Count > 0)
                    {
                        fieldResult.Add(codeForInvariant, fieldValues.Values.First());
                    }
                }

                result.Add(field.Name, fieldResult);
            }

            return result;
        }

        public static object ToLanguageModel(this NamedContentData content, LanguagesConfig languagesConfig, IReadOnlyCollection<Language> languagePreferences = null)
        {
            Guard.NotNull(languagesConfig, nameof(languagesConfig));

            if (languagePreferences == null || languagePreferences.Count == 0)
            {
                return content;
            }

            if (languagePreferences.Count == 1 && languagesConfig.TryGetConfig(languagePreferences.First(), out var languageConfig))
            {
                languagePreferences = languagePreferences.Union(languageConfig.LanguageFallbacks).ToList();
            }

            var result = new Dictionary<string, JToken>();

            foreach (var fieldValue in content)
            {
                var fieldValues = fieldValue.Value;

                foreach (var language in languagePreferences)
                {
                    if (fieldValues.TryGetValue(language, out var value) && value != null)
                    {
                        result[fieldValue.Key] = value;

                        break;
                    }
                }
            }

            return result;
        }
    }
}
