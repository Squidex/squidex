// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Caching.Memory;

namespace Squidex.Infrastructure.Caching
{
    public class InvalidatingMemoryCache : DisposableObjectBase, IMemoryCache, IInvalidatingCache
    {
        private readonly IMemoryCache inner;
        private readonly IDisposable subscription;
        private readonly IPubSub invalidator;

        public InvalidatingMemoryCache(IMemoryCache inner, IPubSub invalidator)
        {
            Guard.NotNull(inner, nameof(inner));
            Guard.NotNull(invalidator, nameof(invalidator));

            this.inner = inner;
            this.invalidator = invalidator;

            subscription = invalidator.Subscribe<InvalidateMessage>(m =>
            {
                inner.Remove(m.CacheKey);
            });
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                subscription.Dispose();

                inner.Dispose();
            }
        }

        public ICacheEntry CreateEntry(object key)
        {
            return inner.CreateEntry(key);
        }

        public bool TryGetValue(object key, out object value)
        {
            return inner.TryGetValue(key, out value);
        }

        public void Remove(object key)
        {
            inner.Remove(key);
        }

        public void Invalidate(object key)
        {
            if (key is string stringKey)
            {
                invalidator.Publish(new InvalidateMessage { CacheKey = stringKey }, true);
            }
        }
    }
}
