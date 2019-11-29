// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;

namespace Squidex.Infrastructure.Json.Newtonsoft
{
    public sealed class LanguageConverter : JsonClassConverter<Language>
    {
        protected override void WriteValue(JsonWriter writer, Language value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Iso2Code);
        }

        protected override Language ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            var value = serializer.Deserialize<string>(reader)!;

            return Language.GetLanguage(value);
        }
    }
}
