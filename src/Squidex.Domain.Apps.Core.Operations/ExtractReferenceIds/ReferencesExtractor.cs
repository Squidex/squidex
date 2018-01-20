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

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds
{
    public static class ReferencesExtractor
    {
        public static IEnumerable<Guid> ExtractReferences(this Field field, JToken value)
        {
            switch (field)
            {
                case AssetsField assetsField:
                    return Visit(assetsField, value);

                case ReferencesField referencesField:
                    return Visit(referencesField, value);
            }

            return Enumerable.Empty<Guid>();
        }

        public static IEnumerable<Guid> Visit(AssetsField field, JToken value)
        {
            IEnumerable<Guid> result;
            try
            {
                result = value?.ToObject<List<Guid>>();
            }
            catch
            {
                result = null;
            }

            return result ?? Enumerable.Empty<Guid>();
        }

        private static IEnumerable<Guid> Visit(ReferencesField field, JToken value)
        {
            IEnumerable<Guid> result;
            try
            {
                result = value?.ToObject<List<Guid>>() ?? Enumerable.Empty<Guid>();
            }
            catch
            {
                result = Enumerable.Empty<Guid>();
            }

            return result.Union(new[] { field.Properties.SchemaId });
        }
    }
}
