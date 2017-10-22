// ==========================================================================
//  AppContributorsConverter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Squidex.Domain.Apps.Core.Apps.Json
{
    public sealed class AppContributorsConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var contributors = (AppContributors)value;

            var json = new Dictionary<string, AppContributorPermission>(contributors.Count);

            foreach (var contributor in contributors)
            {
                json.Add(contributor.Key, contributor.Value);
            }

            serializer.Serialize(writer, json);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var json = serializer.Deserialize<Dictionary<string, AppContributorPermission>>(reader);

            var contributors = new AppContributors();

            foreach (var contributor in json)
            {
                contributors.Assign(contributor.Key, contributor.Value);
            }

            return contributors;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(AppContributors);
        }
    }
}
