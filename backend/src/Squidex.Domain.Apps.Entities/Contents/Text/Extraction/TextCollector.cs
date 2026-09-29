// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.ObjectPool;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public sealed class TextCollector(
    IReadOnlyList<IFieldTextStrategy>? strategies = null,
    IReadOnlyList<ITextNormalizer>? normalizers = null,
    TextExtractionContext? context = null)
    : IDisposable
{
    // Not all text indexes support field weights, therefore we boost titles by repeating them.
    private const int TitleWeight = 3;
    private readonly Dictionary<string, StringBuilder> bodies = [];
    private readonly Dictionary<string, StringBuilder> titles = [];

    public TextExtractionContext? Context => context;

    public ResolvedComponents Components => context?.Components ?? ResolvedComponents.Empty;

    // The language and the title flag of the field that is currently collected.
    public string Language { get; set; } = InvariantPartitioning.Key;

    public bool IsTitle { get; set; }

    public void Dispose()
    {
        foreach (var sb in bodies.Values.Concat(titles.Values))
        {
            DefaultPools.StringBuilder.Return(sb);
        }
    }

    public void AppendField(IField? field, JsonValue value)
    {
        var searchMode = field?.RawProperties.SearchMode ?? FieldSearchMode.Default;
        if (searchMode == FieldSearchMode.Exclude)
        {
            return;
        }

        var wasTitle = IsTitle;
        try
        {
            IsTitle |= searchMode == FieldSearchMode.Title;

            foreach (var strategy in strategies ?? [])
            {
                if (strategy.TryExtract(field, value, this))
                {
                    return;
                }
            }
        }
        finally
        {
            // The title mode only applies to the field and its nested values.
            IsTitle = wasTitle;
        }
    }

    public void AppendValue(JsonValue value)
    {
        AppendField(null, value);
    }

    public void AppendObject(JsonObject obj, Func<string, IField?> fields)
    {
        foreach (var (key, value) in obj)
        {
            if (key is Component.Discriminator or Component.Descriptor)
            {
                continue;
            }

            AppendField(fields(key), value);
        }
    }

    public void AppendText(string text, IField? field = null)
    {
        foreach (var normalizer in normalizers ?? [])
        {
            var normalized = normalizer.Normalize(text, field);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return;
            }

            text = normalized;
        }

        Append(text, Language, IsTitle);
    }

    public void Append(string text, string language, bool isTitle)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var target = isTitle ? titles : bodies;
        if (!target.TryGetValue(language, out var sb))
        {
            sb = DefaultPools.StringBuilder.Get();

            target[language] = sb;
        }

        sb.AppendIfNotEmpty(' ');
        sb.Append(text.Trim());
    }

    public Dictionary<string, string>? Build()
    {
        Dictionary<string, string>? result = null;

        foreach (var language in titles.Keys.Union(bodies.Keys))
        {
            var sb = new StringBuilder();

            if (titles.TryGetValue(language, out var title))
            {
                for (var i = 0; i < TitleWeight; i++)
                {
                    sb.AppendIfNotEmpty(' ');
                    sb.Append(title);
                }
            }

            if (bodies.TryGetValue(language, out var body))
            {
                sb.AppendIfNotEmpty(' ');
                sb.Append(body);
            }

            result ??= [];
            result[language] = sb.ToString();
        }

        return result;
    }
}
