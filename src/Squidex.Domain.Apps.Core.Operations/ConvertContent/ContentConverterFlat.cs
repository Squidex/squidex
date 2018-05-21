// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.ConvertContent
{
    public static class ContentConverterFlat
    {
        public static object ToFlatLanguageModel(this NamedContentData content, LanguagesConfig languagesConfig, IReadOnlyCollection<Language> languagePreferences = null)
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
                var fieldData = fieldValue.Value;

                foreach (var language in languagePreferences)
                {
                    if (fieldData.TryGetValue(language, out var value) && value != null)
                    {
                        result[fieldValue.Key] = value;

                        break;
                    }
                }
            }

            return result;
        }

        public static Dictionary<string, object> ToFlatten(this NamedContentData content)
        {
            var result = new Dictionary<string, object>();

            foreach (var fieldValue in content)
            {
                var fieldData = fieldValue.Value;

                if (fieldData.Count == 1)
                {
                    result[fieldValue.Key] = fieldData.Values.First();
                }
                else
                {
                    result[fieldValue.Key] = fieldData;
                }
            }

            return result;
        }
    }
}
