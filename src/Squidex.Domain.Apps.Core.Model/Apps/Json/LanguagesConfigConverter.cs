// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.Apps.Json
{
    public sealed class LanguagesConfigConverter : JsonClassConverter<LanguagesConfig>
    {
        protected override void WriteValue(JsonWriter writer, LanguagesConfig value, JsonSerializer serializer)
        {
            var json = new JsonLanguagesConfig(value);

            serializer.Serialize(writer, json);
        }

        protected override LanguagesConfig ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            var json = serializer.Deserialize<JsonLanguagesConfig>(reader);

            return json.ToConfig();
        }
    }
}
