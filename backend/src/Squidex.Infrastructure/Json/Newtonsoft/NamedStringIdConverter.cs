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
    public sealed class NamedStringIdConverter : JsonClassConverter<NamedId<string>>
    {
        private static readonly Parser<string> Parser = ParseString;

        protected override void WriteValue(JsonWriter writer, NamedId<string> value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        protected override NamedId<string> ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            var value = serializer.Deserialize<string>(reader)!;

            if (!NamedId<string>.TryParse(value, Parser, out var result))
            {
                throw new JsonException("Named id must have at least 2 parts divided by commata.");
            }

            return result;
        }

        private static bool ParseString(ReadOnlySpan<char> value, out string result)
        {
            result = new string(value);

            return true;
        }
    }
}
