// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds
{
    public static class ValueReferencesConverter
    {
        public static ValueConverter CleanReferences(IEnumerable<Guid> deletedReferencedIds)
        {
            var ids = new HashSet<Guid>(deletedReferencedIds);

            return (value, field) =>
            {
                if (value.IsNull())
                {
                    return value;
                }

                return field.CleanReferences(value, ids);
            };
        }
    }
}
