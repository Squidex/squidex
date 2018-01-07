// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;

namespace Squidex.Infrastructure.Json
{
    public sealed class LanguageConverter : JsonClassConverter<Language>
    {
        protected override void WriteValue(JsonWriter writer, Language value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Iso2Code);
        }

        protected override Language ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
            {
                throw new JsonException($"Expected String, but got {reader.TokenType}.");
            }

            return Language.GetLanguage(reader.Value.ToString());
        }
    }
}
