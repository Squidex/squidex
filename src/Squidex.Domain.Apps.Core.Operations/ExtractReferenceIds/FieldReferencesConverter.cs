// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds
{
    public static class FieldReferencesConverter
    {
        public static FieldConverter CleanReferences(IEnumerable<Guid> deletedReferencedIds)
        {
            var ids = new HashSet<Guid>(deletedReferencedIds);

            return (data, field) =>
            {
                foreach (var partitionValue in data.Where(x => !x.Value.IsNull()).ToList())
                {
                    var newValue = field.CleanReferences(partitionValue.Value, ids);

                    data[partitionValue.Key] = newValue;
                }

                return data;
            };
        }
    }
}
