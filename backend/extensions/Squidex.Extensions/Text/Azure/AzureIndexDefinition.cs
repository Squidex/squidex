// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Azure.Search.Documents.Indexes.Models;

namespace Squidex.Extensions.Text.Azure
{
    public static class AzureIndexDefinition
    {
        private static readonly Dictionary<string, (string Field, string Analyzer)> AllowedLanguages = new Dictionary<string, (string Field, string Analyzer)>(StringComparer.OrdinalIgnoreCase)
        {
            ["iv"] = ("text_iv", LexicalAnalyzerName.StandardLucene.ToString()),
            ["zh"] = ("text_zh", LexicalAnalyzerName.ZhHansLucene.ToString())
        };

        static AzureIndexDefinition()
        {
            var analyzers =
                typeof(LexicalAnalyzerName)
                    .GetProperties(BindingFlags.Public | BindingFlags.Static)
                    .Select(x => x.GetValue(null))
                    .Select(x => x.ToString())
                    .OrderBy(x => x)
                    .ToList();

            var addedLanguage = new HashSet<string>();

            foreach (var analyzer in analyzers)
            {
                var indexOfDot = analyzer.IndexOf('.', StringComparison.Ordinal);

                if (indexOfDot > 0)
                {
                    var language = analyzer[0..indexOfDot];

                    var isValidLanguage =
                        language.Length == 2 ||
                        language.StartsWith("zh-", StringComparison.Ordinal);

                    if (isValidLanguage && addedLanguage.Add(language))
                    {
                        var fieldName = $"text_{language.Replace('-', '_')}";

                        AllowedLanguages[language] = (fieldName, analyzer);
                    }
                }
            }
        }

        public static string GetTextField(string key)
        {
            if (AllowedLanguages.TryGetValue(key, out var field))
            {
                return field.Field;
            }

            if (key.Length > 2 && AllowedLanguages.TryGetValue(key[2..], out field))
            {
                return field.Field;
            }

            return AllowedLanguages["iv"].Field;
        }

        public static SearchIndex Create(string indexName)
        {
            var fields = new List<SearchField>
            {
                new SimpleField("docId", SearchFieldDataType.String)
                {
                    IsKey = true,
                },
                new SimpleField("appId", SearchFieldDataType.String)
                {
                    IsFilterable = true,
                },
                new SimpleField("appName", SearchFieldDataType.String)
                {
                    IsFilterable = false
                },
                new SimpleField("contentId", SearchFieldDataType.String)
                {
                    IsFilterable = false
                },
                new SimpleField("schemaId", SearchFieldDataType.String)
                {
                    IsFilterable = true
                },
                new SimpleField("schemaName", SearchFieldDataType.String)
                {
                    IsFilterable = false
                },
                new SimpleField("serveAll", SearchFieldDataType.Boolean)
                {
                    IsFilterable = true
                },
                new SimpleField("servePublished", SearchFieldDataType.Boolean)
                {
                    IsFilterable = true
                }
            };

            foreach (var (field, analyzer) in AllowedLanguages.Values)
            {
                fields.Add(
                    new SearchableField(field)
                    {
                        IsFilterable = false,
                        AnalyzerName = analyzer
                    });
            }

            var index = new SearchIndex(indexName)
            {
                Fields = fields
            };

            return index;
        }
    }
}
