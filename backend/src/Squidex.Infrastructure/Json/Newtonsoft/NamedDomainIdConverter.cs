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
    public sealed class NamedDomainIdConverter : JsonClassConverter<NamedId<DomainId>>
    {
        private static readonly Parser<DomainId> Parser = ParseString;

        protected override void WriteValue(JsonWriter writer, NamedId<DomainId> value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        protected override NamedId<DomainId> ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            var value = serializer.Deserialize<string>(reader)!;

            if (!NamedId<DomainId>.TryParse(value, Parser, out var result))
            {
                throw new JsonException("Named id must have at least 2 parts divided by comma.");
            }

            return result;
        }

        private static bool ParseString(ReadOnlySpan<char> value, out DomainId result)
        {
            result = DomainId.Create(new string(value));

            return true;
        }
    }
}
