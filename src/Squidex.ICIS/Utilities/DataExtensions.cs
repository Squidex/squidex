using NodaTime;
using NodaTime.Text;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Json.Objects;
using System;

namespace Squidex.ICIS.Utilities
{
    public static class DataExtensions
    {
        public static bool TryGetGuid(this NamedContentData data, string field, out Guid id, string language = "iv")
        {
            id = default;

            return data.TryGetValue(field, out var values) &&
                values.TryGetValue(language, out var value) &&
                value != null &&
                value is JsonArray array &&
                array.Count == 1 &&
                array[0] != null &&
                Guid.TryParse(array[0].ToString(), out id);
        }

        public static bool TryGetDateTime(this NamedContentData data, string field, out Instant dateTime, string language = "iv")
        {
            dateTime = default;

            return data != null && data.TryGetValue(field, out var value) && value.TryGetDateTime(out dateTime, language);
        }

        public static bool TryGetDateTime(this ContentFieldData data, out Instant dateTime, string language = "iv")
        {
            dateTime = default;

            if (data != null &&
                data.TryGetValue(language, out var value) &&
                value != null &&
                value is JsonString s)
            {
                var parsed = InstantPattern.General.Parse(s.ToString());

                if (parsed.Success)
                {
                    dateTime = parsed.Value;

                    return true;
                }
            }

            return false;
        }

        public static bool TryGetNumber(this NamedContentData data, string field, out double number, string language = "iv")
        {
            number = default;

            return data != null && data.TryGetValue(field, out var value) && value.TryGetNumber(out number, language);
        }

        public static bool TryGetNumber(this ContentFieldData data, out double number, string language = "iv")
        {
            number = default;

            if (data != null &&
                data.TryGetValue(language, out var value) &&
                value != null &&
                value is JsonNumber n)
            {
                number = n.Value;
                return true;
            }

            return false;
        }

        public static IJsonValue GetValue(this NamedContentData data, string field, string language)
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