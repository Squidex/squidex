// ==========================================================================
//  LRUCacheItem.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

#pragma warning disable SA1401 // Fields must be private

namespace Squidex.Infrastructure.Caching
{
    internal class LRUCacheItem
    {
        public object Key;
        public object Value;
    }
}