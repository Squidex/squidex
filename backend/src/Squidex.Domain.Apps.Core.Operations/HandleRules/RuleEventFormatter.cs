// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// =========================================-=================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public class RuleEventFormatter
    {
        private const string GlobalFallback = "null";
        private static readonly Regex RegexPatternOld = new Regex(@"^(?<FullPath>(?<Type>[^_]*)_(?<Path>[^\s]*))", RegexOptions.Compiled);
        private static readonly Regex RegexPatternNew = new Regex(@"^\{(?<FullPath>(?<Type>[\w]+)_(?<Path>[\w\.\-]+))[\s]*(\|[\s]*(?<Transform>[^\?}]+))?(\?[\s]*(?<Fallback>[^\}\s]+))?[\s]*\}", RegexOptions.Compiled);
        private readonly List<(string Pattern, Func<EnrichedEvent, string?> Replacer)> patterns = new List<(string Pattern, Func<EnrichedEvent, string?> Replacer)>();
        private readonly IJsonSerializer jsonSerializer;
        private readonly IEnumerable<IRuleEventFormatter> formatters;
        private readonly IUrlGenerator urlGenerator;
        private readonly IScriptEngine scriptEngine;

        private struct TextPart
        {
            public bool IsText;

            public int Length;

            public int Offset;

            public string Fallback;

            public string Transform;

            public ValueTask<string?> Replacement;

            public static TextPart Text(int offset, int length)
            {
                var result = default(TextPart);
                result.Offset = offset;
                result.Length = length;
                result.IsText = true;

                return result;
            }

            public static TextPart Variable(ValueTask<string?> replacement, string fallback, string transform)
            {
                var result = default(TextPart);
                result.Replacement = replacement;
                result.Fallback = fallback;
                result.Transform = transform;

                return result;
            }
        }

        public RuleEventFormatter(IJsonSerializer jsonSerializer, IEnumerable<IRuleEventFormatter> formatters, IUrlGenerator urlGenerator, IScriptEngine scriptEngine)
        {
            Guard.NotNull(jsonSerializer, nameof(jsonSerializer));
            Guard.NotNull(scriptEngine, nameof(scriptEngine));
            Guard.NotNull(urlGenerator, nameof(urlGenerator));
            Guard.NotNull(formatters, nameof(formatters));

            this.jsonSerializer = jsonSerializer;
            this.formatters = formatters;
            this.scriptEngine = scriptEngine;
            this.urlGenerator = urlGenerator;

            AddPattern("APP_ID", AppId);
            AddPattern("APP_NAME", AppName);
            AddPattern("ASSET_CONTENT_URL", AssetContentUrl);
            AddPattern("CONTENT_ACTION", ContentAction);
            AddPattern("CONTENT_URL", ContentUrl);
            AddPattern("MENTIONED_ID", MentionedId);
            AddPattern("MENTIONED_NAME", MentionedName);
            AddPattern("MENTIONED_EMAIL", MentionedEmail);
            AddPattern("SCHEMA_ID", SchemaId);
            AddPattern("SCHEMA_NAME", SchemaName);
            AddPattern("TIMESTAMP_DATETIME", TimestampTime);
            AddPattern("TIMESTAMP_DATE", TimestampDate);
            AddPattern("USER_ID", UserId);
            AddPattern("USER_NAME", UserName);
            AddPattern("USER_EMAIL", UserEmail);
        }

        private void AddPattern(string placeholder, Func<EnrichedEvent, string?> generator)
        {
            patterns.Add((placeholder, generator));
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

            if (TryGetScript(text.Trim(), out var script))
            {
                var context = new ScriptContext
                {
                    ["event"] = @event
                };

                return scriptEngine.Interpolate(context, script);
            }

            var parts = BuildParts(text, @event);

            await Task.WhenAll(parts.Select(x => x.Replacement.AsTask()));

            return CombineParts(text, parts);
        }

        private string CombineParts(string text, List<TextPart> parts)
        {
            var span = text.AsSpan();

            var sb = new StringBuilder();

            foreach (var part in parts)
            {
                if (!part.IsText)
                {
                    var result = part.Replacement.Result;

                    result = TransformText(result, part.Transform);

                    if (result == null)
                    {
                        result = part.Fallback;
                    }

                    if (string.IsNullOrEmpty(result))
                    {
                        result = GlobalFallback;
                    }

                    sb.Append(result);
                }
                else
                {
                    sb.Append(span.Slice(part.Offset, part.Length));
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

                    var (length, part) = GetReplacement(span.Slice(i + 1).ToString(), @event);

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

        private (bool IsNew, Match) Match(string test)
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

            for (var j = 0; j < patterns.Count; j++)
            {
                var (pattern, replacer) = patterns[j];

                if (fullPath.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    var result = new ValueTask<string?>(replacer(@event));

                    if (isNewRegex)
                    {
                        return (match.Length, result);
                    }
                    else
                    {
                        return (pattern.Length, result);
                    }
                }
            }

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

        private static string TimestampDate(EnrichedEvent @event)
        {
            return @event.Timestamp.ToDateTimeUtc().ToString("yyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private static string TimestampTime(EnrichedEvent @event)
        {
            return @event.Timestamp.ToDateTimeUtc().ToString("yyy-MM-dd-hh-mm-ss", CultureInfo.InvariantCulture);
        }

        private static string AppId(EnrichedEvent @event)
        {
            return @event.AppId.Id.ToString();
        }

        private static string AppName(EnrichedEvent @event)
        {
            return @event.AppId.Name;
        }

        private static string? SchemaId(EnrichedEvent @event)
        {
            if (@event is EnrichedSchemaEventBase schemaEvent)
            {
                return schemaEvent.SchemaId.Id.ToString();
            }

            return null;
        }

        private static string? SchemaName(EnrichedEvent @event)
        {
            if (@event is EnrichedSchemaEventBase schemaEvent)
            {
                return schemaEvent.SchemaId.Name;
            }

            return null;
        }

        private static string? ContentAction(EnrichedEvent @event)
        {
            if (@event is EnrichedContentEvent contentEvent)
            {
                return contentEvent.Type.ToString();
            }

            return null;
        }

        private string? AssetContentUrl(EnrichedEvent @event)
        {
            if (@event is EnrichedAssetEvent assetEvent)
            {
                return urlGenerator.AssetContent(assetEvent.Id);
            }

            return null;
        }

        private string? ContentUrl(EnrichedEvent @event)
        {
            if (@event is EnrichedContentEvent contentEvent)
            {
                return urlGenerator.ContentUI(contentEvent.AppId, contentEvent.SchemaId, contentEvent.Id);
            }

            return null;
        }

        private static string? UserName(EnrichedEvent @event)
        {
            if (@event is EnrichedUserEventBase userEvent)
            {
                return userEvent.User?.DisplayName();
            }

            return null;
        }

        private static string? UserId(EnrichedEvent @event)
        {
            if (@event is EnrichedUserEventBase userEvent)
            {
                return userEvent.User?.Id;
            }

            return null;
        }

        private static string? UserEmail(EnrichedEvent @event)
        {
            if (@event is EnrichedUserEventBase userEvent)
            {
                return userEvent.User?.Email;
            }

            return null;
        }

        private static string? MentionedName(EnrichedEvent @event)
        {
            if (@event is EnrichedCommentEvent commentEvent)
            {
                return commentEvent.MentionedUser.DisplayName();
            }

            return null;
        }

        private static string? MentionedId(EnrichedEvent @event)
        {
            if (@event is EnrichedCommentEvent commentEvent)
            {
                return commentEvent.MentionedUser.Id;
            }

            return null;
        }

        private static string? MentionedEmail(EnrichedEvent @event)
        {
            if (@event is EnrichedCommentEvent commentEvent)
            {
                return commentEvent.MentionedUser.Email;
            }

            return null;
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
                    }
                }
            }

            return text;
        }

        private (int Length, ValueTask<string?> Result) ResolveFromPath(Match match, EnrichedEvent @event)
        {
            var path = match.Groups["Path"].Value.Split('.', StringSplitOptions.RemoveEmptyEntries);

            foreach (var formatter in formatters)
            {
                var (replaced, result) = formatter.Format(@event, path);

                if (replaced)
                {
                    return (match.Length, result);
                }
            }

            var (result2, _) = RuleVariable.GetValue(@event, path);

            return (match.Length, new ValueTask<string?>(result2?.ToString()));
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
    }
}
