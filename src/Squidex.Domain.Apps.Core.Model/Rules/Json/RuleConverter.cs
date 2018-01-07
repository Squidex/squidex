// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.Rules.Json
{
    public sealed class RuleConverter : JsonClassConverter<Rule>
    {
        protected override void WriteValue(JsonWriter writer, Rule value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, new JsonRule(value));
        }

        protected override Rule ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            return serializer.Deserialize<JsonRule>(reader).ToRule();
        }
    }
}
