// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds
{
    public static class ValueReferencesConverter
    {
        public static ValueConverter CleanReferences(HashSet<Guid>? validIds = null)
        {
            if (validIds == null || validIds.Count == 0)
            {
                return (value, field) => value;
            }

            var cleaner = new ReferencesCleaner(validIds);

            return (value, field) =>
            {
                if (value.Type == JsonValueType.Null)
                {
                    return value!;
                }

                cleaner.SetValue(value);

                return field.Accept(cleaner);
            };
        }
    }
}
