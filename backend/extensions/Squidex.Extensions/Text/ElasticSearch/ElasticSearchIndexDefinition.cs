// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Extensions.Text.ElasticSearch;

public static class ElasticSearchIndexDefinition
{
    private static readonly Dictionary<string, string> FieldPaths;
    private static readonly Dictionary<string, string> FieldAnalyzers = new Dictionary<string, string>
    {
        ["ar"] = "arabic",
        ["hy"] = "armenian",
        ["eu"] = "basque",
        ["bn"] = "bengali",
        ["br"] = "brazilian",
        ["bg"] = "bulgarian",
        ["ca"] = "catalan",
        ["zh"] = "cjk",
        ["ja"] = "cjk",
        ["ko"] = "cjk",
        ["cs"] = "czech",
        ["da"] = "danish",
        ["nl"] = "dutch",
        ["en"] = "english",
        ["fi"] = "finnish",
        ["fr"] = "french",
        ["gl"] = "galician",
        ["de"] = "german",
        ["el"] = "greek",
        ["hi"] = "hindi",
        ["hu"] = "hungarian",
        ["id"] = "indonesian",
        ["ga"] = "irish",
        ["it"] = "italian",
        ["lv"] = "latvian",
        ["lt"] = "lithuanian",
        ["no"] = "norwegian",
        ["pt"] = "portuguese",
        ["ro"] = "romanian",
        ["ru"] = "russian",
        ["ku"] = "sorani",
        ["es"] = "spanish",
        ["sv"] = "swedish",
        ["tr"] = "turkish",
        ["th"] = "thai"
    };

    static ElasticSearchIndexDefinition()
    {
        FieldPaths = FieldAnalyzers.ToDictionary(x => x.Key, x => $"texts.{x.Key}");
    }

    public static string GetFieldName(string key)
    {
        if (FieldAnalyzers.ContainsKey(key))
        {
            return key;
        }

        if (key.Length > 0)
        {
            var language = key[2..];

            if (FieldAnalyzers.ContainsKey(language))
            {
                return language;
            }
        }

        return "iv";
    }

    public static string GetFieldPath(string key)
    {
        if (FieldPaths.TryGetValue(key, out var path))
        {
            return path;
        }

        if (key.Length > 0)
        {
            var language = key[2..];

            if (FieldPaths.TryGetValue(language, out path))
            {
                return path;
            }
        }

        return "texts.iv";
    }

    public static Task ApplyAsync(IElasticSearchClient client, string indexName,
        CancellationToken ct = default)
    {
        var query = new
        {
            properties = new Dictionary<string, object>
            {
                ["geoObject"] = new
                {
                    type = "geo_point"
                }
            }
        };

        foreach (var (key, analyzer) in FieldAnalyzers)
        {
            query.properties[GetFieldPath(key)] = new
            {
                type = "text",
                analyzer
            };
        }

        return client.CreateIndexAsync(indexName, query, ct);
    }
}
