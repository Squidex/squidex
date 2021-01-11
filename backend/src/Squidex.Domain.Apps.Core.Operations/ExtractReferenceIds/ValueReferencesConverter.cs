// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds
{
    public static class ValueReferencesConverter
    {
        public static ValueConverter CleanReferences(HashSet<DomainId>? validIds = null)
        {
            if (validIds == null)
            {
                return ValueConverters.Noop;
            }

            return (value, field, parent) =>
            {
                if (value.Type == JsonValueType.Null)
                {
                    return value;
                }

                return ReferencesCleaner.Cleanup(field, value, validIds);
            };
        }
    }
}
