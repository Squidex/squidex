// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Caching
{
    public interface IRequestCache
    {
        void Add(object key, object value);

        void Remove(object key);

        bool TryGetValue(object key, out object value);
    }
}
