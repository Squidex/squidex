// ==========================================================================
//  ReferenceExtractor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
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
            var oldIds = field.ExtractReferences(value).ToList();
            var newIds = oldIds.Except(oldReferences).ToList();

            return oldIds.Count != newIds.Count ? JToken.FromObject(newIds) : value;
        }

        private static JToken Visit(ReferencesField field, JToken value, ICollection<Guid> oldReferences)
        {
            if (oldReferences.Contains(field.Properties.SchemaId))
            {
                return new JArray();
            }

            var oldIds = field.ExtractReferences(value).ToList();
            var newIds = oldIds.Except(oldReferences).ToList();

            return oldIds.Count != newIds.Count ? JToken.FromObject(newIds) : value;
        }
    }
}
