// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Squidex.Infrastructure.Json.Newtonsoft;

namespace Squidex.Domain.Apps.Core.Apps.Json
{
    public sealed class AppContributorsConverter : JsonClassConverter<AppContributors>
    {
        protected override void WriteValue(JsonWriter writer, AppContributors value, JsonSerializer serializer)
        {
            var json = new Dictionary<string, string>(value.Count);

            foreach (var (userId, role) in value)
            {
                json.Add(userId, role);
            }

            serializer.Serialize(writer, json);
        }

        protected override AppContributors ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            var json = serializer.Deserialize<Dictionary<string, string>>(reader)!;

            return new AppContributors(json!);
        }
    }
}
