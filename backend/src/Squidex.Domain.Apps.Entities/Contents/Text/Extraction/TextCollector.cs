// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Squidex.Infrastructure;
using Squidex.Infrastructure.ObjectPool;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public sealed class TextCollector : IDisposable
{
    // Not all text indexes support field weights, therefore we boost titles by repeating them.
    private const int TitleWeight = 3;
    private readonly Dictionary<string, StringBuilder> bodies = [];
    private readonly Dictionary<string, StringBuilder> titles = [];

    public void Dispose()
    {
        foreach (var sb in bodies.Values.Concat(titles.Values))
        {
            DefaultPools.StringBuilder.Return(sb);
        }
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
