// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime.Extensions;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Json.Objects;
using System;

namespace Squidex.ICIS.Kafka.Services
{
    public static class MappingHelper
    {
        public static string GetString(this NamedContentData data, string field, string language = "iv")
        {
            var value = GetValue(data, field, language);

            return value.ToString();
        }

        public static long GetTimestamp(this NamedContentData data, string field, string language = "iv")
        {
            var value = GetValue(data, field, language).ToString();

            return DateTime.Parse(value).ToUniversalTime().ToInstant().ToUnixTimeSeconds();
        }

        public static Guid GetFirstReference(this NamedContentData data, string field, string language = "iv")
        {
            var value = GetValue(data, field, language) as JsonArray;

            return Guid.Parse(value[0].ToString());
        }

        private static IJsonValue GetValue(NamedContentData data, string field, string language)
        {
            if (!data.TryGetValue(field, out var fieldValue))
            {
                throw new ArgumentException($"Cannot find field '{field}' in data.", nameof(data));
            }

            if (!fieldValue.TryGetValue(language, out var value))
            {
                throw new ArgumentException($"Cannot find '{language}' value in field '{field}'.", nameof(data));
            }

            return value;
        }
    }
}
