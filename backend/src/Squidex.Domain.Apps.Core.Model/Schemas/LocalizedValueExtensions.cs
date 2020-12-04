// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public static class LocalizedValueExtensions
    {
        public static TValue? GetValueOrDefault<TValue>(this LocalizedValue<TValue> dictionary, string key, TValue defaultValue = default(TValue))
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }
    }
}
