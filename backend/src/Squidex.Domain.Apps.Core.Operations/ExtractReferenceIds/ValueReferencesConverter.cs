// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Infrastructure;

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
                if (value == default)
                {
                    return value;
                }

                return ReferencesCleaner.Cleanup(field, value, validIds);
            };
        }
    }
}
