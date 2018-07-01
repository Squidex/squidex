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
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
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
        private static readonly Regex ContentDataPlaceholderV2 = new Regex(@"\$\{CONTENT_DATA(\.([0-9A-Za-z\-_]*)){2,}\}", RegexOptions.Compiled);
        private static readonly TimeSpan UserCacheDuration = TimeSpan.FromMinutes(10);
        private readonly JsonSerializer serializer;
        private readonly IRuleUrlGenerator urlGenerator;
        private readonly IMemoryCache memoryCache;
        private readonly IUserResolver userResolver;

        public RuleEventFormatter(
            JsonSerializer serializer,
            IRuleUrlGenerator urlGenerator,
            IMemoryCache memoryCache,
            IUserResolver userResolver)
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

        public virtual JObject ToPayload(object @event)
        {
            return JObject.FromObject(@event, serializer);
        }

        public virtual JObject ToEnvelope(EnrichedEvent @event)
        {
            return new JObject(
                new JProperty("type", @event),
                new JProperty("payload", ToPayload(@event)),
                new JProperty("timestamp", @event.Timestamp.ToString()));
        }

        public async virtual Task<string> FormatStringAsync(string text, EnrichedEvent @event)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            var sb = new StringBuilder(text);

            sb.Replace(TimestampDateTimePlaceholder, @event.Timestamp.ToString("yyy-MM-dd-hh-mm-ss", CultureInfo.InvariantCulture));
            sb.Replace(TimestampDatePlaceholder, @event.Timestamp.ToString("yyy-MM-dd", CultureInfo.InvariantCulture));

            if (@event.AppId != null)
            {
                sb.Replace(AppIdPlaceholder, @event.AppId.Id.ToString());
                sb.Replace(AppNamePlaceholder, @event.AppId.Name);
            }

            if (@event is EnrichedSchemaEvent schemaEvent && schemaEvent.SchemaId != null)
            {
                sb.Replace(SchemaIdPlaceholder, schemaEvent.SchemaId.Id.ToString());
                sb.Replace(SchemaNamePlaceholder, schemaEvent.SchemaId.Name);
            }

            if (@event is EnrichedContentEvent contentEvent)
            {
                sb.Replace(ContentUrlPlaceholder, urlGenerator.GenerateContentUIUrl(@event.AppId, contentEvent.SchemaId, contentEvent.Id));
                sb.Replace(ContentActionPlaceholder, contentEvent.Action.ToString());
            }

            await FormatUserInfoAsync(@event, sb);

            var result = sb.ToString();

            if (@event is EnrichedContentEvent contentEvent2)
            {
                result = ReplaceData(contentEvent2.Data, result);
            }

            return result;
        }

        private async Task FormatUserInfoAsync(EnrichedEvent @event, StringBuilder sb)
        {
            var text = sb.ToString();

            if (text.Contains(UserEmailPlaceholder) || text.Contains(UserNamePlaceholder))
            {
                var actor = @event.Actor;

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

        private static string ReplaceData(NamedContentData data, string text)
        {
            text = ContentDataPlaceholder.Replace(text, match =>
            {
                return Replace(data, match);
            });

            text = ContentDataPlaceholderV2.Replace(text, match =>
            {
                return Replace(data, match);
            });

            return text;
        }

        private static string Replace(NamedContentData data, Match match)
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
