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

    public ExtractedTexts? Build()
    {
        var extractedTexts = Build(bodies);
        var extractedTitles = Build(titles);

        if (extractedTexts == null && extractedTitles == null)
        {
            return null;
        }

        return new ExtractedTexts(extractedTexts, extractedTitles);
    }

    private static Dictionary<string, string>? Build(Dictionary<string, StringBuilder> source)
    {
        if (source.Count == 0)
        {
            return null;
        }

        return source.ToDictionary(x => x.Key, x => x.Value.ToString());
    }
}
