// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// =========================================-=================================

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public class RuleEventFormatter
    {
        private const string Undefined = "UNDEFINED";
        private const string AppIdPlaceholder = "$APP_ID";
        private const string AppNamePlaceholder = "$APP_NAME";
        private const string SchemaIdPlaceholder = "$SCHEMA_ID";
        private const string SchemaNamePlaceholder = "$SCHEMA_NAME";
        private const string TimestampDatePlaceholder = "$TIMESTAMP_DATE";
        private const string TimestampDateTimePlaceholder = "$TIMESTAMP_DATETIME";
        private const string ContentActionPlaceholder = "$CONTENT_ACTION";
        private static readonly Regex ContentDataPlaceholder = new Regex(@"\$CONTENT_DATA(\.([0-9A-Za-z\-_]*)){2,}", RegexOptions.Compiled);
        private readonly JsonSerializer serializer;

        public RuleEventFormatter(JsonSerializer serializer)
        {
            Guard.NotNull(serializer, nameof(serializer));

            this.serializer = serializer;
        }

        public virtual JToken ToRouteData(object value)
        {
            return JToken.FromObject(value, serializer);
        }

        public virtual JToken ToRouteData(Envelope<AppEvent> @event, string eventName)
        {
            return new JObject(
                new JProperty("type", eventName),
                new JProperty("payload", JToken.FromObject(@event.Payload, serializer)),
                new JProperty("timestamp", @event.Headers.Timestamp().ToString()));
        }

        public virtual string FormatString(string text, Envelope<AppEvent> @event)
        {
            var sb = new StringBuilder(text);

            if (@event.Headers.Contains(CommonHeaders.Timestamp))
            {
                var timestamp = @event.Headers.Timestamp().ToDateTimeUtc();

                sb.Replace(TimestampDateTimePlaceholder, timestamp.ToString("yyy-MM-dd-hh-mm-ss", CultureInfo.InvariantCulture));
                sb.Replace(TimestampDatePlaceholder, timestamp.ToString("yyy-MM-dd", CultureInfo.InvariantCulture));
            }

            if (@event.Payload.AppId != null)
            {
                sb.Replace(AppIdPlaceholder, @event.Payload.AppId.Id.ToString());
                sb.Replace(AppNamePlaceholder, @event.Payload.AppId.Name);
            }

            if (@event.Payload is SchemaEvent schemaEvent && schemaEvent.SchemaId != null)
            {
                sb.Replace(SchemaIdPlaceholder, schemaEvent.SchemaId.Id.ToString());
                sb.Replace(SchemaNamePlaceholder, schemaEvent.SchemaId.Name);
            }

            FormatContentAction(@event, sb);

            var result = sb.ToString();

            if (@event.Payload is ContentCreated contentCreated && contentCreated.Data != null)
            {
                result = ReplaceData(contentCreated.Data, result);
            }

            if (@event.Payload is ContentUpdated contentUpdated && contentUpdated.Data != null)
            {
                result = ReplaceData(contentUpdated.Data, result);
            }

            return result;
        }

        private static void FormatContentAction(Envelope<AppEvent> @event, StringBuilder sb)
        {
            switch (@event.Payload)
            {
                case ContentCreated contentCreated:
                    sb.Replace(ContentActionPlaceholder, "created");
                    break;

                case ContentUpdated contentUpdated:
                    sb.Replace(ContentActionPlaceholder, "updated");
                    break;

                case ContentStatusChanged contentStatusChanged:
                    sb.Replace(ContentActionPlaceholder, $"set to {contentStatusChanged.Status.ToString().ToLowerInvariant()}");
                    break;

                case ContentDeleted contentDeleted:
                    sb.Replace(ContentActionPlaceholder, "deleted");
                    break;
            }
        }

        private static string ReplaceData(NamedContentData data, string text)
        {
            return ContentDataPlaceholder.Replace(text, match =>
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

                if (value is JValue jValue && jValue != null)
                {
                    return jValue.Value.ToString();
                }

                return value?.ToString(Formatting.Indented) ?? Undefined;
            });
        }
    }
}
