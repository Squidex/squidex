// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;
using Azure.Search.Documents.Indexes.Models;

namespace Squidex.Extensions.Text.Azure;

public static class AzureIndexDefinition
{
    private static readonly Dictionary<string, (string Field, string Analyzer)> FieldAnalyzers = new (StringComparer.OrdinalIgnoreCase)
    {
        ["iv"] = ("iv", LexicalAnalyzerName.StandardLucene.ToString()),
        ["zh"] = ("zh", LexicalAnalyzerName.ZhHansLucene.ToString())
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
                var language = analyzer[..indexOfDot];

                var isValidLanguage =
                    language.Length == 2 ||
                    language.StartsWith("zh-", StringComparison.Ordinal);

                if (isValidLanguage && addedLanguage.Add(language))
                {
                    var fieldName = language.Replace('-', '_');

                    FieldAnalyzers[language] = (fieldName, analyzer);
                }
            }
        }
    }

    public static string GetFieldName(string key)
    {
        if (FieldAnalyzers.TryGetValue(key, out var analyzer))
        {
            return analyzer.Field;
        }

        if (key.Length > 0)
        {
            var language = key[2..];

            if (FieldAnalyzers.TryGetValue(language, out analyzer))
            {
                return analyzer.Field;
            }
        }

        return "iv";
    }

    public static SearchIndex Create(string indexName)
    {
        var fields = new List<SearchField>
        {
            new SimpleField("docId", SearchFieldDataType.String)
            {
                IsKey = true
            },
            new SimpleField("appId", SearchFieldDataType.String)
            {
                IsFilterable = true
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
            },
            new SimpleField("geoObject", SearchFieldDataType.GeographyPoint)
            {
                IsFilterable = true
            },
            new SimpleField("geoField", SearchFieldDataType.String)
            {
                IsFilterable = true
            }
        };

        foreach (var (field, analyzer) in FieldAnalyzers.Values)
        {
            fields.Add(
                new SearchableField(field)
                {
                    IsFilterable = false,
                    IsFacetable = false,
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
