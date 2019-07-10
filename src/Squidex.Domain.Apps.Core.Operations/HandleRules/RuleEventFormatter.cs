// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// =========================================-=================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public class RuleEventFormatter
    {
        private const string Fallback = "null";
        private const string ScriptSuffix = ")";
        private const string ScriptPrefix = "Script(";
        private static readonly char[] ContentPlaceholderStartOld = "CONTENT_DATA".ToCharArray();
        private static readonly char[] ContentPlaceholderStartNew = "{CONTENT_DATA".ToCharArray();
        private static readonly Regex ContentDataPlaceholderOld = new Regex(@"^CONTENT_DATA(\.([0-9A-Za-z\-_]*)){2,}", RegexOptions.Compiled);
        private static readonly Regex ContentDataPlaceholderNew = new Regex(@"^\{CONTENT_DATA(\.([0-9A-Za-z\-_]*)){2,}\}", RegexOptions.Compiled);
        private readonly List<(char[] Pattern, Func<EnrichedEvent, string> Replacer)> patterns = new List<(char[] Pattern, Func<EnrichedEvent, string> Replacer)>();
        private readonly IJsonSerializer jsonSerializer;
        private readonly IRuleUrlGenerator urlGenerator;
        private readonly IScriptEngine scriptEngine;

        public RuleEventFormatter(IJsonSerializer jsonSerializer, IRuleUrlGenerator urlGenerator, IScriptEngine scriptEngine)
        {
            Guard.NotNull(jsonSerializer, nameof(jsonSerializer));
            Guard.NotNull(scriptEngine, nameof(scriptEngine));
            Guard.NotNull(urlGenerator, nameof(urlGenerator));

            this.jsonSerializer = jsonSerializer;
            this.scriptEngine = scriptEngine;
            this.urlGenerator = urlGenerator;

            AddPattern("APP_ID", AppId);
            AddPattern("APP_NAME", AppName);
            AddPattern("CONTENT_ACTION", ContentAction);
            AddPattern("CONTENT_STATUS", ContentStatus);
            AddPattern("CONTENT_URL", ContentUrl);
            AddPattern("SCHEMA_ID", SchemaId);
            AddPattern("SCHEMA_NAME", SchemaName);
            AddPattern("TIMESTAMP_DATETIME", TimestampTime);
            AddPattern("TIMESTAMP_DATE", TimestampDate);
            AddPattern("USER_ID", UserId);
            AddPattern("USER_NAME", UserName);
            AddPattern("USER_EMAIL", UserEmail);
        }

        private void AddPattern(string placeholder, Func<EnrichedEvent, string> generator)
        {
            patterns.Add((placeholder.ToCharArray(), generator));
        }

        public virtual string ToPayload<T>(T @event)
        {
            return jsonSerializer.Serialize(@event);
        }

        public virtual string ToEnvelope(EnrichedEvent @event)
        {
            return jsonSerializer.Serialize(new { type = @event.Name, payload = @event, timestamp = @event.Timestamp });
        }

        public string Format(string text, EnrichedEvent @event)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            if (text.StartsWith(ScriptPrefix, StringComparison.OrdinalIgnoreCase) && text.EndsWith(ScriptSuffix, StringComparison.OrdinalIgnoreCase))
            {
                var script = text.Substring(ScriptPrefix.Length, text.Length - ScriptPrefix.Length - ScriptSuffix.Length);

                var customFunctions = new Dictionary<string, Func<string>>
                {
                    ["contentUrl"] = () => ContentUrl(@event),
                    ["contentAction"] = () => ContentAction(@event)
                };

                return scriptEngine.Interpolate("event", @event, script, customFunctions);
            }

            var current = text.AsSpan();

            var sb = new StringBuilder();

            var cp2 = new ReadOnlySpan<char>(ContentPlaceholderStartNew);
            var cp1 = new ReadOnlySpan<char>(ContentPlaceholderStartOld);

            for (var i = 0; i < current.Length; i++)
            {
                var c = current[i];

                if (c == '$')
                {
                    sb.Append(current.Slice(0, i).ToString());

                    current = current.Slice(i);

                    var test = current.Slice(1);
                    var tested = false;

                    for (var j = 0; j < patterns.Count; j++)
                    {
                        var (pattern, replacer) = patterns[j];

                        if (test.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
                        {
                            sb.Append(replacer(@event));

                            current = current.Slice(pattern.Length + 1);
                            i = 0;

                            tested = true;
                            break;
                        }
                    }

                    if (!tested && (test.StartsWith(cp1, StringComparison.OrdinalIgnoreCase) || test.StartsWith(cp2, StringComparison.OrdinalIgnoreCase)))
                    {
                        var currentString = test.ToString();

                        var match = ContentDataPlaceholderOld.Match(currentString);

                        if (!match.Success)
                        {
                            match = ContentDataPlaceholderNew.Match(currentString);
                        }

                        if (match.Success)
                        {
                            if (@event is EnrichedContentEvent contentEvent)
                            {
                                sb.Append(CalculateData(contentEvent.Data, match));
                            }
                            else
                            {
                                sb.Append(Fallback);
                            }

                            current = current.Slice(match.Length + 1);
                            i = 0;
                        }
                    }
                }
            }

            sb.Append(current.ToString());

            return sb.ToString();
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

        private static string SchemaId(EnrichedEvent @event)
        {
            if (@event is EnrichedSchemaEventBase schemaEvent)
            {
                return schemaEvent.SchemaId.Id.ToString();
            }

            return Fallback;
        }

        private static string SchemaName(EnrichedEvent @event)
        {
            if (@event is EnrichedSchemaEventBase schemaEvent)
            {
                return schemaEvent.SchemaId.Name;
            }

            return Fallback;
        }

        private static string ContentAction(EnrichedEvent @event)
        {
            if (@event is EnrichedContentEvent contentEvent)
            {
                return contentEvent.Type.ToString();
            }

            return Fallback;
        }

        private static string ContentStatus(EnrichedEvent @event)
        {
            if (@event is EnrichedContentEvent contentEvent)
            {
                return contentEvent.Status.ToString();
            }

            return Fallback;
        }

        private string ContentUrl(EnrichedEvent @event)
        {
            if (@event is EnrichedContentEvent contentEvent)
            {
                return urlGenerator.GenerateContentUIUrl(contentEvent.AppId, contentEvent.SchemaId, contentEvent.Id);
            }

            return Fallback;
        }

        private static string UserName(EnrichedEvent @event)
        {
            if (@event is EnrichedUserEventBase userEvent)
            {
                return userEvent.User?.DisplayName() ?? Fallback;
            }

            return Fallback;
        }

        private static string UserId(EnrichedEvent @event)
        {
            if (@event is EnrichedUserEventBase userEvent)
            {
                return userEvent.User?.Id ?? Fallback;
            }

            return Fallback;
        }

        private static string UserEmail(EnrichedEvent @event)
        {
            if (@event is EnrichedUserEventBase userEvent)
            {
                return userEvent.User?.Email ?? Fallback;
            }

            return Fallback;
        }

        private static string CalculateData(NamedContentData data, Match match)
        {
            var captures = match.Groups[2].Captures;

            var path = new string[captures.Count];

            for (var i = 0; i < path.Length; i++)
            {
                path[i] = captures[i].Value;
            }

            if (!data.TryGetValue(path[0], out var field))
            {
                return Fallback;
            }

            if (!field.TryGetValue(path[1], out var value))
            {
                return Fallback;
            }

            for (var j = 2; j < path.Length; j++)
            {
                if (value is JsonObject obj && obj.TryGetValue(path[j], out value))
                {
                    continue;
                }

                if (value is JsonArray array && int.TryParse(path[j], out var idx) && idx >= 0 && idx < array.Count)
                {
                    value = array[idx];
                }
                else
                {
                    return Fallback;
                }
            }

            if (value == null || value.Type == JsonValueType.Null)
            {
                return Fallback;
            }

            return value.ToString() ?? Fallback;
        }
    }
}
