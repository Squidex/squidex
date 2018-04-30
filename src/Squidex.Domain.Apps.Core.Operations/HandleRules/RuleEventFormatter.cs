// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// =========================================-=================================

using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Shared.Users;

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
        private const string ContentUrlPlaceholder = "$CONTENT_URL";
        private const string UserNamePlaceholder = "$USER_NAME";
        private const string UserEmailPlaceholder = "$USER_EMAIL";
        private static readonly Regex ContentDataPlaceholder = new Regex(@"\$CONTENT_DATA(\.([0-9A-Za-z\-_]*)){2,}", RegexOptions.Compiled);
        private static readonly TimeSpan UserCacheDuration = TimeSpan.FromMinutes(10);
        private readonly JsonSerializer serializer;
        private readonly IRuleUrlGenerator urlGenerator;
        private readonly IMemoryCache memoryCache;
        private readonly IUserResolver userResolver;

        public RuleEventFormatter(JsonSerializer serializer, IRuleUrlGenerator urlGenerator, IMemoryCache memoryCache, IUserResolver userResolver)
        {
            Guard.NotNull(memoryCache, nameof(memoryCache));
            Guard.NotNull(serializer, nameof(serializer));
            Guard.NotNull(urlGenerator, nameof(urlGenerator));
            Guard.NotNull(userResolver, nameof(userResolver));

            this.memoryCache = memoryCache;
            this.serializer = serializer;
            this.userResolver = userResolver;
            this.urlGenerator = urlGenerator;
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

        public async virtual Task<string> FormatStringAsync(string text, Envelope<AppEvent> @event)
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

            if (@event.Payload is ContentEvent contentEvent)
            {
                sb.Replace(ContentUrlPlaceholder, urlGenerator.GenerateContentUIUrl(@event.Payload.AppId, contentEvent.SchemaId, contentEvent.ContentId));
            }

            await FormatUserInfoAsync(@event, sb);

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

        private async Task FormatUserInfoAsync(Envelope<AppEvent> @event, StringBuilder sb)
        {
            var text = sb.ToString();

            if (text.Contains(UserEmailPlaceholder) || text.Contains(UserNamePlaceholder))
            {
                var actor = @event.Payload.Actor;

                if (actor.Type.Equals("client", StringComparison.OrdinalIgnoreCase))
                {
                    var displayText = actor.ToString();

                    sb.Replace(UserEmailPlaceholder, displayText);
                    sb.Replace(UserNamePlaceholder, displayText);
                }
                else
                {
                    var user = await FindUserAsync(actor);

                    if (user != null)
                    {
                        sb.Replace(UserEmailPlaceholder, user.Email);
                        sb.Replace(UserNamePlaceholder, user.DisplayName());
                    }
                    else
                    {
                        sb.Replace(UserEmailPlaceholder, Undefined);
                        sb.Replace(UserNamePlaceholder, Undefined);
                    }
                }
            }
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

        private Task<IUser> FindUserAsync(RefToken actor)
        {
            var key = $"RuleEventFormatter_Users_${actor.Identifier}";

            return memoryCache.GetOrCreateAsync(key, async x =>
            {
                x.AbsoluteExpirationRelativeToNow = UserCacheDuration;

                try
                {
                    return await userResolver.FindByIdOrEmailAsync(actor.Identifier);
                }
                catch
                {
                    return null;
                }
            });
        }
    }
}
