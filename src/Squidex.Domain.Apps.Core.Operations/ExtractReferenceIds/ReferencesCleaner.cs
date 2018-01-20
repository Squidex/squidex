// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds
{
    public static class ReferencesCleaner
    {
        private static readonly List<Guid> EmptyIds = new List<Guid>();
        public static JToken CleanReferences(this Field field, JToken value, ISet<Guid> oldReferences)
        {
            if ((field is AssetsField || field is ReferencesField) && !value.IsNull())
            {
                switch (field)
                {
                    case AssetsField assetsField:
                        return Visit(assetsField, value, oldReferences);

                    case ReferencesField referencesField:
                        return Visit(referencesField, value, oldReferences);
                }
            }

            return value;
        }

        private static JToken Visit(AssetsField field, JToken value, IEnumerable<Guid> oldReferences)
        {
            var oldIds = GetIds(value);
            var newIds = oldIds.Except(oldReferences).ToList();

            return oldIds.Count != newIds.Count ? JToken.FromObject(newIds) : value;
        }

        private static JToken Visit(ReferencesField field, JToken value, ICollection<Guid> oldReferences)
        {
            if (oldReferences.Contains(field.Properties.SchemaId))
            {
                return new JArray();
            }

            var oldIds = GetIds(value);
            var newIds = oldIds.Except(oldReferences).ToList();

            return oldIds.Count != newIds.Count ? JToken.FromObject(newIds) : value;
        }

        private static List<Guid> GetIds(JToken value)
        {
            try
            {
                return value?.ToObject<List<Guid>>() ?? EmptyIds;
            }
            catch
            {
                return EmptyIds;
            }
        }
    }
}
