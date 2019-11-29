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
    public sealed class RefTokenConverter : JsonClassConverter<RefToken>
    {
        protected override void WriteValue(JsonWriter writer, RefToken value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        protected override RefToken ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            var value = serializer.Deserialize<string>(reader)!;

            if (!RefToken.TryParse(value, out var result))
            {
                throw new JsonException("Named id must have at least 2 parts divided by colon.");
            }

            return result;
        }
    }
}