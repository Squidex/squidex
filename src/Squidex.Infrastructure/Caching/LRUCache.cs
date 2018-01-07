// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Infrastructure.Caching
{
    public sealed class LRUCache
    {
        private readonly Dictionary<object, LinkedListNode<LRUCacheItem>> cacheMap = new Dictionary<object, LinkedListNode<LRUCacheItem>>();
        private readonly LinkedList<LRUCacheItem> cacheHistory = new LinkedList<LRUCacheItem>();
        private readonly int capacity;

        public LRUCache(int capacity)
        {
            Guard.GreaterThan(capacity, 0, nameof(capacity));

            this.capacity = capacity;
        }

        public bool Set(object key, object value)
        {
            Guard.NotNull(key, nameof(key));

            if (cacheMap.TryGetValue(key, out var node))
            {
                node.Value.Value = value;

                cacheHistory.Remove(node);
                cacheHistory.AddLast(node);

                cacheMap[key] = node;

                return true;
            }
            else
            {
                if (cacheMap.Count >= capacity)
                {
                    RemoveFirst();
                }

                var cacheItem = new LRUCacheItem { Key = key, Value = value };

                node = new LinkedListNode<LRUCacheItem>(cacheItem);

                cacheMap.Add(key, node);
                cacheHistory.AddLast(node);

                return false;
            }
        }

        public bool Remove(object key)
        {
            Guard.NotNull(key, nameof(key));

            if (cacheMap.TryGetValue(key, out var node))
            {
                cacheMap.Remove(key);
                cacheHistory.Remove(node);

                return true;
            }

            return false;
        }

        public bool TryGetValue(object key, out object value)
        {
            Guard.NotNull(key, nameof(key));

            value = null;

            if (cacheMap.TryGetValue(key, out var node))
            {
                value = node.Value.Value;

                cacheHistory.Remove(node);
                cacheHistory.AddLast(node);

                return true;
            }

            return false;
        }

        public bool Contains(object key)
        {
            Guard.NotNull(key, nameof(key));

            return cacheMap.ContainsKey(key);
        }

        private void RemoveFirst()
        {
            var node = cacheHistory.First;

            cacheMap.Remove(node.Value.Key);
            cacheHistory.RemoveFirst();
        }
    }
}