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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Infrastructure;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public class RuleEventFormatter
    {
        private const string Undefined = "UNDEFINED";
        private static readonly char[] ContentPlaceholderStartOld = "CONTENT_DATA".ToCharArray();
        private static readonly char[] ContentPlaceholderStartNew = "{CONTENT_DATA".ToCharArray();
        private static readonly Regex ContentDataPlaceholderOld = new Regex(@"^CONTENT_DATA(\.([0-9A-Za-z\-_]*)){2,}", RegexOptions.Compiled);
        private static readonly Regex ContentDataPlaceholderNew = new Regex(@"^\{CONTENT_DATA(\.([0-9A-Za-z\-_]*)){2,}\}", RegexOptions.Compiled);
        private readonly List<(char[] Pattern, Func<EnrichedEvent, string> Replacer)> patterns = new List<(char[] Pattern, Func<EnrichedEvent, string> Replacer)>();
        private readonly JsonSerializer serializer;
        private readonly IRuleUrlGenerator urlGenerator;

        public RuleEventFormatter(JsonSerializer serializer, IRuleUrlGenerator urlGenerator)
        {
            Guard.NotNull(serializer, nameof(serializer));
            Guard.NotNull(urlGenerator, nameof(urlGenerator));

            this.serializer = serializer;
            this.urlGenerator = urlGenerator;

            AddPattern("APP_ID", AppId);
            AddPattern("APP_NAME", AppName);
            AddPattern("CONTENT_ACTION", ContentAction);
            AddPattern("CONTENT_URL", ContentUrl);
            AddPattern("SCHEMA_ID", SchemaId);
            AddPattern("SCHEMA_NAME", SchemaName);
            AddPattern("TIMESTAMP_DATETIME", TimestampTime);
            AddPattern("TIMESTAMP_DATE", TimestampDate);
            AddPattern("USER_NAME", UserName);
            AddPattern("USER_EMAIL", UserEmail);
        }

        private void AddPattern(string placeholder, Func<EnrichedEvent, string> generator)
        {
            patterns.Add((placeholder.ToCharArray(), generator));
        }

        public virtual JObject ToPayload<T>(T @event)
        {
            return JObject.FromObject(@event, serializer);
        }

        public virtual JObject ToEnvelope(EnrichedEvent @event)
        {
            return new JObject(
                new JProperty("type", @event.Name),
                new JProperty("payload", ToPayload(@event)),
                new JProperty("timestamp", @event.Timestamp.ToString()));
        }

        public string Format(string text, EnrichedEvent @event)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
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
                                sb.Append(Undefined);
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
            if (@event is EnrichedSchemaEvent schemaEvent)
            {
                return schemaEvent.SchemaId.Id.ToString();
            }

            return Undefined;
        }

        private static string SchemaName(EnrichedEvent @event)
        {
            if (@event is EnrichedSchemaEvent schemaEvent)
            {
                return schemaEvent.SchemaId.Name;
            }

            return Undefined;
        }

        private static string ContentAction(EnrichedEvent @event)
        {
            if (@event is EnrichedContentEvent contentEvent)
            {
                return contentEvent.Type.ToString().ToLowerInvariant();
            }

            return Undefined;
        }

        private string ContentUrl(EnrichedEvent @event)
        {
            if (@event is EnrichedContentEvent contentEvent)
            {
                return urlGenerator.GenerateContentUIUrl(contentEvent.AppId, contentEvent.SchemaId, contentEvent.Id);
            }

            return Undefined;
        }

        private static string UserName(EnrichedEvent @event)
        {
            if (@event.Actor != null)
            {
                if (@event.Actor.Type.Equals(RefTokenType.Client, StringComparison.OrdinalIgnoreCase))
                {
                    return @event.Actor.ToString();
                }

                if (@event.User != null)
                {
                    return @event.User.DisplayName();
                }
            }

            return Undefined;
        }

        private static string UserEmail(EnrichedEvent @event)
        {
            if (@event.Actor != null)
            {
                if (@event.Actor.Type.Equals(RefTokenType.Client, StringComparison.OrdinalIgnoreCase))
                {
                    return @event.Actor.ToString();
                }

                if (@event.User != null)
                {
                    return @event.User.Email;
                }
            }

            return Undefined;
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
                return Undefined;
            }

            if (!field.TryGetValue(path[1], out var value))
            {
                return Undefined;
            }

            for (var j = 2; j < path.Length; j++)
            {
                if (value is JObject obj && obj.TryGetValue(path[j], out value))
                {
                    continue;
                }

                if (value is JArray arr && int.TryParse(path[j], out var idx) && idx >= 0 && idx < arr.Count)
                {
                    value = arr[idx];
                }
                else
                {
                    return Undefined;
                }
            }

            if (value == null || value.Type == JTokenType.Null || value.Type == JTokenType.Undefined)
            {
                return Undefined;
            }

            if (value is JValue jValue)
            {
                return jValue.Value.ToString();
            }

            return value.ToString(Formatting.Indented) ?? Undefined;
        }
    }
}
