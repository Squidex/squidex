﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;

namespace Squidex.Infrastructure.Json.Newtonsoft
{
    public sealed class NamedLongIdConverter : JsonClassConverter<NamedId<long>>
    {
        protected override void WriteValue(JsonWriter writer, NamedId<long> value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        protected override NamedId<long> ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            var value = serializer.Deserialize<string>(reader);

            if (!NamedId<long>.TryParse(value, long.TryParse, out var result))
            {
                throw new JsonException("Named id must have at least 2 parts divided by commata.");
            }

            return result;
        }
    }
}
