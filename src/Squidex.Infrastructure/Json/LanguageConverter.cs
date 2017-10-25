// ==========================================================================
//  LanguageConverter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json;

namespace Squidex.Infrastructure.Json
{
    public sealed class LanguageConverter : JsonClassConverter<Language>
    {
        protected override void WriteValue(JsonWriter writer, Language value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Iso2Code);
        }

        protected override Language ReadValue(JsonReader reader, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
            {
                throw new JsonException($"Expected String, but got {reader.TokenType}.");
            }

            return Language.GetLanguage(reader.Value.ToString());
        }
    }
}
