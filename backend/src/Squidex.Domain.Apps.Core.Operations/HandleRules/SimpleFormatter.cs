// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using NodaTime.Text;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Infrastructure;
using Squidex.Text;
using ValueTaskSupplement;

namespace Squidex.Domain.Apps.Core.HandleRules;

public sealed partial class SimpleFormatter(IEnumerable<IRuleEventFormatter> formatters)
{
    private static readonly Regex RegexPatternOld = RegexPatternOldFactory();
    private static readonly Regex RegexPatternNew = RegexPatternNewFactory();

    private struct TextPart
    {
        public bool IsText;

        public int TextLength;

        public int TextOffset;

        public string VarFallback;

        public string VarTransform;

        public ValueTask<string?> Var;

        public static TextPart Text(int offset, int length)
        {
            var result = default(TextPart);

            result.TextOffset = offset;
            result.TextLength = length;
            result.IsText = true;

            return result;
        }

        public static TextPart Variable(ValueTask<string?> replacement, string fallback, string transform)
        {
            var result = default(TextPart);

            result.Var = replacement;
            result.VarFallback = fallback;
            result.VarTransform = transform;

            return result;
        }
    }

    public async ValueTask<string> Format(string text, EnrichedEvent @event, string fallback)
    {
        var parts = BuildParts(text, @event);

        if (parts.Exists(x => !x.Var.IsCompleted))
        {
            await ValueTaskEx.WhenAll(parts.Select(x => x.Var));
        }

        return CombineParts(text, parts, fallback);
    }

    private static string CombineParts(string text, List<TextPart> parts, string fallback)
    {
        var span = text.AsSpan();

        var sb = new StringBuilder();

        foreach (var part in parts)
        {
            if (!part.IsText)
            {
                var result = TransformText(part.Var.Result, part.VarTransform) ?? part.VarFallback;
                if (string.IsNullOrEmpty(result))
                {
                    result = fallback;
                }

                sb.Append(result);
            }
            else
            {
                sb.Append(span.Slice(part.TextOffset, part.TextLength));
            }
        }

        return sb.ToString();
    }

    private List<TextPart> BuildParts(string text, EnrichedEvent @event)
    {
        var parts = new List<TextPart>();

        var span = text.AsSpan();

        var currentOffset = 0;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (c == '$')
            {
                parts.Add(TextPart.Text(currentOffset, i - currentOffset));

                var (length, part) = GetReplacement(span[(i + 1)..].ToString(), @event);
                if (length > 0)
                {
                    parts.Add(part);
                    i += length + 1;
                }

                currentOffset = i;
            }
        }

        parts.Add(TextPart.Text(currentOffset, text.Length - currentOffset));

        return parts;
    }

    private (int Length, TextPart Part) GetReplacement(string test, EnrichedEvent @event)
    {
        var (isNewRegex, match) = Match(test);

        if (match.Success)
        {
            var (length, replacement) = ResolveOldPatterns(match, isNewRegex, @event);
            if (length == 0)
            {
                (length, replacement) = ResolveFromPath(match, @event);
            }

            return (length, TextPart.Variable(replacement, match.Groups["Fallback"].Value, match.Groups["Transform"].Value));
        }

        return default;
    }

    private static (bool IsNew, Match) Match(string test)
    {
        var match = RegexPatternNew.Match(test);

        if (match.Success)
        {
            return (true, match);
        }

        return (false, RegexPatternOld.Match(test));
    }

    private (int Length, ValueTask<string?> Result) ResolveOldPatterns(Match match, bool isNewRegex, EnrichedEvent @event)
    {
        var fullPath = match.Groups["FullPath"].Value;

        foreach (var formatter in formatters)
        {
            var (replaced, result, replacedLength) = formatter.Format(@event, fullPath);

            if (replaced)
            {
                if (isNewRegex)
                {
                    replacedLength = match.Length;
                }

                return (replacedLength, new ValueTask<string?>(result));
            }
        }

        return default;
    }

    private static string? TransformText(string? text, string? transform)
    {
        if (text != null && !string.IsNullOrWhiteSpace(transform))
        {
            var transformations = transform.Split("|", StringSplitOptions.RemoveEmptyEntries);

            foreach (var transformation in transformations)
            {
                switch (transformation.Trim().ToLowerInvariant())
                {
                    case "lower":
                        text = text.ToLowerInvariant();
                        break;
                    case "upper":
                        text = text.ToUpperInvariant();
                        break;
                    case "escape":
                        text = text.JsonEscape();
                        break;
                    case "slugify":
                        text = text.Slugify();
                        break;
                    case "trim":
                        text = text.Trim();
                        break;
                    case "timestamp":
                        {
                            var instant = InstantPattern.ExtendedIso.Parse(text);

                            if (instant.Success)
                            {
                                text = instant.Value.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
                            }

                            break;
                        }

                    case "timestamp_sec":
                        {
                            var instant = InstantPattern.ExtendedIso.Parse(text);

                            if (instant.Success)
                            {
                                text = instant.Value.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
                            }

                            break;
                        }
                }
            }
        }

        return text;
    }

    private (int Length, ValueTask<string?> Result) ResolveFromPath(Match match, EnrichedEvent @event)
    {
        var path = match.Groups["Path"].Value.Split('.', StringSplitOptions.RemoveEmptyEntries);

        var (result, remaining) = RuleVariable.GetValue(@event, path);

        if (remaining.Length > 0 && result != null)
        {
            foreach (var formatter in formatters)
            {
                var (replaced, result2) = formatter.Format(@event, result, remaining);

                if (replaced)
                {
                    return (match.Length, result2);
                }
            }
        }
        else if (remaining.Length == 0)
        {
            return (match.Length, new ValueTask<string?>(result?.ToString()));
        }

        return (match.Length, default);
    }

    [GeneratedRegex(@"^(?<FullPath>(?<Type>[^_]*)_(?<Path>[^\s]*))", RegexOptions.Compiled | RegexOptions.ExplicitCapture)]
    private static partial Regex RegexPatternOldFactory();

    [GeneratedRegex(@"^\{(?<FullPath>(?<Type>[\w]+)_(?<Path>[\w\.\-]+))[\s]*(\|[\s]*(?<Transform>[^\?}]+))?(\?[\s]*(?<Fallback>[^\}\s]+))?[\s]*\}", RegexOptions.Compiled | RegexOptions.ExplicitCapture)]
    private static partial Regex RegexPatternNewFactory();
}
