// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Newtonsoft.Json;
using Squidex.Infrastructure.Json.Newtonsoft;

namespace Squidex.Infrastructure.Queries.Json
{
    public sealed class PropertyPathConverter : JsonClassConverter<PropertyPath>
    {
        protected override PropertyPath ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            var value = serializer.Deserialize<string>(reader)!;

            return value;
        }

        protected override void WriteValue(JsonWriter writer, PropertyPath value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.ToList());
        }
    }
}
