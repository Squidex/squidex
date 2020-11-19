﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// =========================================-=================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NodaTime.Text;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.Templates;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Text;
using ValueTaskSupplement;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public class RuleEventFormatter
    {
        private const string GlobalFallback = "null";
        private static readonly Regex RegexPatternOld = new Regex(@"^(?<FullPath>(?<Type>[^_]*)_(?<Path>[^\s]*))", RegexOptions.Compiled);
        private static readonly Regex RegexPatternNew = new Regex(@"^\{(?<FullPath>(?<Type>[\w]+)_(?<Path>[\w\.\-]+))[\s]*(\|[\s]*(?<Transform>[^\?}]+))?(\?[\s]*(?<Fallback>[^\}\s]+))?[\s]*\}", RegexOptions.Compiled);
        private readonly IJsonSerializer jsonSerializer;
        private readonly IEnumerable<IRuleEventFormatter> formatters;
        private readonly ITemplateEngine templateEngine;
        private readonly IScriptEngine scriptEngine;

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

        public RuleEventFormatter(IJsonSerializer jsonSerializer, IEnumerable<IRuleEventFormatter> formatters, ITemplateEngine templateEngine, IScriptEngine scriptEngine)
        {
            Guard.NotNull(jsonSerializer, nameof(jsonSerializer));
            Guard.NotNull(scriptEngine, nameof(scriptEngine));
            Guard.NotNull(templateEngine, nameof(templateEngine));
            Guard.NotNull(formatters, nameof(formatters));

            this.jsonSerializer = jsonSerializer;
            this.formatters = formatters;
            this.templateEngine = templateEngine;
            this.scriptEngine = scriptEngine;
        }

        public virtual string ToPayload<T>(T @event)
        {
            return jsonSerializer.Serialize(@event);
        }

        public virtual string ToEnvelope(EnrichedEvent @event)
        {
            return jsonSerializer.Serialize(new { type = @event.Name, payload = @event, timestamp = @event.Timestamp });
        }

        public async ValueTask<string?> FormatAsync(string text, EnrichedEvent @event)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            if (TryGetTemplate(text.Trim(), out var template))
            {
                var vars = new TemplateVars
                {
                    ["event"] = @event
                };

                return await templateEngine.RenderAsync(template, vars);
            }

            if (TryGetScript(text.Trim(), out var script))
            {
                var vars = new ScriptVars
                {
                    ["event"] = @event
                };

                var result = scriptEngine.Execute(vars, script).ToString();

                if (result == "undefined")
                {
                    return GlobalFallback;
                }

                return result;
            }

            var parts = BuildParts(text, @event);

            if (parts.Any(x => !x.Var.IsCompleted))
            {
                await ValueTaskEx.WhenAll(parts.Select(x => x.Var));
            }

            return CombineParts(text, parts);
        }

        private static string CombineParts(string text, List<TextPart> parts)
        {
            var span = text.AsSpan();

            var sb = new StringBuilder();

            foreach (var part in parts)
            {
                if (!part.IsText)
                {
                    var result = part.Var.Result;

                    result = TransformText(result, part.VarTransform);

                    if (result == null)
                    {
                        result = part.VarFallback;
                    }

                    if (string.IsNullOrEmpty(result))
                    {
                        result = GlobalFallback;
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
                            text = JsonConvert.ToString(text);
                            text = text[1..^1];
                            break;
                        case "slugify":
                            text = text.Slugify();
                            break;
                        case "trim":
                            text = text.Trim();
                            break;
                        case "timestamp":
                            {
                                var instant = InstantPattern.General.Parse(text);

                                if (instant.Success)
                                {
                                    text = instant.Value.ToUnixTimeMilliseconds().ToString();
                                }

                                break;
                            }

                        case "timestamp_sec":
                            {
                                var instant = InstantPattern.General.Parse(text);

                                if (instant.Success)
                                {
                                    text = instant.Value.ToUnixTimeSeconds().ToString();
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

        private static bool TryGetScript(string text, out string script)
        {
            const string ScriptSuffix = ")";
            const string ScriptPrefix = "Script(";

            script = null!;

            var comparer = StringComparison.OrdinalIgnoreCase;

            if (text.StartsWith(ScriptPrefix, comparer) && text.EndsWith(ScriptSuffix, comparer))
            {
                script = text.Substring(ScriptPrefix.Length, text.Length - ScriptPrefix.Length - ScriptSuffix.Length);
                return true;
            }

            return false;
        }

        private static bool TryGetTemplate(string text, out string script)
        {
            const string TemplateSuffix = ")";
            const string TemplatePrefix = "Liquid(";

            script = null!;

            var comparer = StringComparison.OrdinalIgnoreCase;

            if (text.StartsWith(TemplatePrefix, comparer) && text.EndsWith(TemplateSuffix, comparer))
            {
                script = text.Substring(TemplatePrefix.Length, text.Length - TemplatePrefix.Length - TemplateSuffix.Length);
                return true;
            }

            return false;
        }
    }
}
