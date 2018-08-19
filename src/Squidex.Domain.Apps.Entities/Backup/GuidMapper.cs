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
    public sealed class GuidMapper
    {
        private static readonly int GuidLength = Guid.Empty.ToString().Length;
        private readonly List<(JObject Source, string NewKey, string OldKey)> mappings = new List<(JObject Source, string NewKey, string OldKey)>();
        private readonly Dictionary<Guid, Guid> oldToNewGuid = new Dictionary<Guid, Guid>();
        private readonly Dictionary<Guid, Guid> newToOldGuid = new Dictionary<Guid, Guid>();

        public Guid NewGuid(Guid oldGuid)
        {
            return oldToNewGuid.GetOrDefault(oldGuid);
        }

        public Guid OldGuid(Guid newGuid)
        {
            return newToOldGuid.GetOrDefault(newGuid);
        }

        public string NewGuidString(string key)
        {
            if (Guid.TryParse(key, out var guid))
            {
                return GenerateNewGuid(guid).ToString();
            }

            return null;
        }

        public JToken NewGuids(JToken jToken)
        {
            var result = NewGuidsCore(jToken);

            if (mappings.Count > 0)
            {
                foreach (var mapping in mappings)
                {
                    if (mapping.Source.TryGetValue(mapping.OldKey, out var value))
                    {
                        mapping.Source.Remove(mapping.OldKey);
                        mapping.Source[mapping.NewKey] = value;
                    }
                }

                mappings.Clear();
            }

            return result;
        }

        private JToken NewGuidsCore(JToken jToken)
        {
            switch (jToken.Type)
            {
                case JTokenType.String:
                    if (TryConvertString(jToken.ToString(), out var result))
                    {
                        return result;
                    }

                    break;
                case JTokenType.Guid:
                    return GenerateNewGuid((Guid)jToken);
                case JTokenType.Object:
                    NewGuidsCore((JObject)jToken);
                    break;
                case JTokenType.Array:
                    NewGuidsCore((JArray)jToken);
                    break;
            }

            return jToken;
        }

        private void NewGuidsCore(JArray jArray)
        {
            for (var i = 0; i < jArray.Count; i++)
            {
                jArray[i] = NewGuidsCore(jArray[i]);
            }
        }

        private void NewGuidsCore(JObject jObject)
        {
            foreach (var jProperty in jObject.Properties())
            {
                var newValue = NewGuidsCore(jProperty.Value);

                if (!ReferenceEquals(newValue, jProperty.Value))
                {
                    jProperty.Value = newValue;
                }

                if (TryConvertString(jProperty.Name, out var newKey))
                {
                    mappings.Add((jObject, newKey, jProperty.Name));
                }
            }
        }

        private bool TryConvertString(string value, out string result)
        {
            return TryGenerateNewGuidString(value, out result) || TryGenerateNewNamedId(value, out result);
        }

        private bool TryGenerateNewGuidString(string value, out string result)
        {
            result = null;

            if (value.Length == GuidLength)
            {
                if (Guid.TryParse(value, out var guid))
                {
                    var newGuid = GenerateNewGuid(guid);

                    result = newGuid.ToString();

                    return true;
                }
            }

            return false;
        }

        private bool TryGenerateNewNamedId(string value, out string result)
        {
            result = null;

            if (value.Length > GuidLength && value[GuidLength] == ',')
            {
                if (Guid.TryParse(value.Substring(0, GuidLength), out var guid))
                {
                    var newGuid = GenerateNewGuid(guid);

                    result = newGuid + value.Substring(GuidLength);

                    return true;
                }
            }

            return false;
        }

        private Guid GenerateNewGuid(Guid oldGuid)
        {
            return oldToNewGuid.GetOrAdd(oldGuid, GuidGenerator);
        }

        private Guid GuidGenerator(Guid oldGuid)
        {
            var newGuid = Guid.NewGuid();

            newToOldGuid[newGuid] = oldGuid;

            return newGuid;
        }
    }
}
