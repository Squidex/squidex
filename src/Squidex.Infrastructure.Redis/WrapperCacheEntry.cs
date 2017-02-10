// ==========================================================================
//  WrapperCacheEntry.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Squidex.Infrastructure.Redis
{
    internal sealed class WrapperCacheEntry : ICacheEntry
    {
        private readonly ICacheEntry inner;
        private readonly RedisInvalidator invalidator;

        public object Key
        {
            get { return inner.Key; }
        }

        public IList<IChangeToken> ExpirationTokens
        {
            get { return inner.ExpirationTokens; }
        }

        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks
        {
            get { return inner.PostEvictionCallbacks; }
        }

        public DateTimeOffset? AbsoluteExpiration
        {
            get { return inner.AbsoluteExpiration; }
            set { inner.AbsoluteExpiration = value; }
        }

        public TimeSpan? AbsoluteExpirationRelativeToNow
        {
            get { return inner.AbsoluteExpirationRelativeToNow; }
            set { inner.AbsoluteExpirationRelativeToNow = value; }
        }

        public TimeSpan? SlidingExpiration
        {
            get { return inner.SlidingExpiration; }
            set { inner.SlidingExpiration = value; }
        }

        public CacheItemPriority Priority
        {
            get { return inner.Priority; }
            set { inner.Priority = value; }
        }

        public object Value
        {
            get { return inner.Value; }
            set { inner.Value = value; }
        }

        public WrapperCacheEntry(ICacheEntry inner, RedisInvalidator invalidator)
        {
            this.inner = inner;

            this.invalidator = invalidator;
        }

        public void Dispose()
        {
            if (Key is string)
            {
                invalidator.Invalidate(Key?.ToString());
            }

            inner.Dispose();
        }
    }
}
