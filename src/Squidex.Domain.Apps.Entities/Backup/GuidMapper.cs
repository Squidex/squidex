// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public static class GuidMapper
    {
        private static readonly int GuidLength = Guid.Empty.ToString().Length;

        public static void GenerateNewGuid(JToken jToken, Dictionary<Guid, Guid> guids)
        {
            if (jToken.Type == JTokenType.Object)
            {
                GenerateNewGuid((JObject)jToken, guids);
            }
        }

        private static void GenerateNewGuid(JObject jObject, Dictionary<Guid, Guid> guids)
        {
            foreach (var kvp in jObject)
            {
                switch (kvp.Value.Type)
                {
                    case JTokenType.String:
                        ReplaceGuidString(jObject, guids, kvp);
                        break;
                    case JTokenType.Guid:
                        ReplaceGuid(jObject, guids, kvp);
                        break;
                    case JTokenType.Object:
                        GenerateNewGuid((JObject)kvp.Value, guids);
                        break;
                }
            }
        }

        private static void ReplaceGuidString(JObject jObject, Dictionary<Guid, Guid> guids, KeyValuePair<string, JToken> kvp)
        {
            var value = kvp.Value.ToString();

            if (value.Length == GuidLength)
            {
                if (Guid.TryParse(value, out var guid))
                {
                    var newGuid = guids.GetOrAdd(guid, GuidGenerator);

                    jObject.Property(kvp.Key).Value = newGuid.ToString();
                }
            }
            else if (value.Length > GuidLength && value[GuidLength] == ',')
            {
                if (Guid.TryParse(value.Substring(0, GuidLength), out var guid))
                {
                    var newGuid = guids.GetOrAdd(guid, GuidGenerator);

                    jObject.Property(kvp.Key).Value = newGuid + value.Substring(GuidLength);
                }
            }
        }

        private static void ReplaceGuid(JObject jObject, Dictionary<Guid, Guid> guids, KeyValuePair<string, JToken> kvp)
        {
            var newGuid = guids.GetOrAdd((Guid)kvp.Value, GuidGenerator);

            jObject.Property(kvp.Key).Value = newGuid;
        }

        private static Guid GuidGenerator(Guid key)
        {
            return Guid.NewGuid();
        }
    }
}
