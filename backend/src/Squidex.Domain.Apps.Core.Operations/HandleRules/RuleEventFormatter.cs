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
        private const string Fallback = "null";
        private static readonly Regex RegexPatternOld = new Regex(@"^(?<FullPath>(?<Type>[^_]*)_(?<Path>[^\s]*))", RegexOptions.Compiled);
        private static readonly Regex RegexPatternNew = new Regex(@"^\{(?<FullPath>(?<Type>[\w]+)_(?<Path>[\w\.\-]+))[\s]*(\|[\s]*(?<Transform>[^\?}]+))?(\?[\s]*(?<Fallback>[^\}\s]+))?[\s]*\}", RegexOptions.Compiled);
        private readonly List<(string Pattern, Func<EnrichedEvent, string?> Replacer)> patterns = new List<(string Pattern, Func<EnrichedEvent, string?> Replacer)>();
        private readonly IJsonSerializer jsonSerializer;
        private readonly IUrlGenerator urlGenerator;
        private readonly IScriptEngine scriptEngine;

        public RuleEventFormatter(IJsonSerializer jsonSerializer, IUrlGenerator urlGenerator, IScriptEngine scriptEngine)
        {
            Guard.NotNull(jsonSerializer, nameof(jsonSerializer));
            Guard.NotNull(scriptEngine, nameof(scriptEngine));
            Guard.NotNull(urlGenerator, nameof(urlGenerator));

            this.jsonSerializer = jsonSerializer;
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

        public string? Format(string text, EnrichedEvent @event)
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

            var span = text.AsSpan();

            var currentOffset = 0;

            var parts = new List<(int Offset, int Length, ValueTask<string?> Task)>();

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];

                if (c == '$')
                {
                    parts.Add((currentOffset, i - currentOffset, default));

                    var (replacement, length) = GetReplacement(span.Slice(i + 1).ToString(), @event);

                    if (length > 0)
                    {
                        parts.Add((0, 0, new ValueTask<string?>(replacement)));

                        i += length + 1;
                    }

                    currentOffset = i;
                }
            }

            parts.Add((currentOffset, text.Length - currentOffset, default));

            var sb = new StringBuilder();

            foreach (var (offset, length, task) in parts)
            {
                if (task.Result != null)
                {
                    sb.Append(task.Result);
                }
                else
                {
                    sb.Append(span.Slice(offset, length));
                }
            }

            return sb.ToString();
        }

        private (string Result, int Length) GetReplacement(string test, EnrichedEvent @event)
        {
            var (isNewRegex, match) = Match(test);

            if (match.Success)
            {
                var (length, text) = ResolveOldPatterns(match, isNewRegex, @event);

                if (length == 0)
                {
                    (length, text) = ResolveFromPath(match, @event);
                }

                var result = TransformText(text, match.Groups["Transform"]?.Value);

                if (result == null)
                {
                    result = match.Groups["Fallback"]?.Value;
                }

                if (string.IsNullOrEmpty(result))
                {
                    result = Fallback;
                }

                return (result, length);
            }

            return (Fallback, 0);
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

        private (int Length, string? Result) ResolveOldPatterns(Match match, bool isNewRegex, EnrichedEvent @event)
        {
            var fullPath = match.Groups["FullPath"].Value;

            for (var j = 0; j < patterns.Count; j++)
            {
                var (pattern, replacer) = patterns[j];

                if (fullPath.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    var result = replacer(@event);

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

        private (int Length, string? Result) ResolveFromPath(Match match, EnrichedEvent @event)
        {
            var path = match.Groups["Path"].Value.Split('.', StringSplitOptions.RemoveEmptyEntries);

            var result = CalculateData(@event, path);

            return (match.Length, result);
        }

        private static string? CalculateData(object @event, string[] path)
        {
            object? current = @event;

            foreach (var segment in path)
            {
                if (current is NamedContentData data)
                {
                    if (!data.TryGetValue(segment, out var temp) || temp == null)
                    {
                        return null;
                    }

                    current = temp;
                }
                else if (current is ContentFieldData field)
                {
                    if (!field.TryGetValue(segment, out var temp) || temp == null)
                    {
                        return null;
                    }

                    current = temp;
                }
                else if (current is IJsonValue json)
                {
                    if (!json.TryGet(segment, out var temp) || temp == null || temp.Type == JsonValueType.Null)
                    {
                        return null;
                    }

                    current = temp;
                }
                else if (current != null)
                {
                    if (current is IUser user)
                    {
                        var type = segment;

                        if (string.Equals(type, "Name", StringComparison.OrdinalIgnoreCase))
                        {
                            type = SquidexClaimTypes.DisplayName;
                        }

                        var claim = user.Claims.FirstOrDefault(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase));

                        if (claim != null)
                        {
                            current = claim.Value;
                            continue;
                        }
                    }

                    const BindingFlags bindingFlags =
                        BindingFlags.FlattenHierarchy |
                        BindingFlags.Public |
                        BindingFlags.Instance;

                    var properties = current.GetType().GetProperties(bindingFlags);
                    var property = properties.FirstOrDefault(x => x.CanRead && string.Equals(x.Name, segment, StringComparison.OrdinalIgnoreCase));

                    if (property == null)
                    {
                        return null;
                    }

                    current = property.GetValue(current);
                }
                else
                {
                    return null;
                }
            }

            return current?.ToString();
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
