// ==========================================================================
//  RuleConverter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.Rules.Json
{
    public sealed class RuleConverter : JsonClassConverter<Rule>
    {
        protected override Rule ReadValue(JsonReader reader, JsonSerializer serializer)
        {
            return serializer.Deserialize<JsonRule>(reader).ToRule();
        }

        protected override void WriteValue(JsonWriter writer, Rule value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, new JsonRule(value));
        }
    }
}
