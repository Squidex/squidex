﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;

namespace Squidex.Infrastructure.Caching
{
    public sealed class LRUCache<TKey, TValue> where TKey : notnull
    {
        private readonly Dictionary<TKey, LinkedListNode<LRUCacheItem<TKey, TValue>>> cacheMap = new Dictionary<TKey, LinkedListNode<LRUCacheItem<TKey, TValue>>>();
        private readonly LinkedList<LRUCacheItem<TKey, TValue>> cacheHistory = new LinkedList<LRUCacheItem<TKey, TValue>>();
        private readonly int capacity;
        private readonly Action<TKey, TValue> itemEvicted;

        public LRUCache(int capacity, Action<TKey, TValue>? itemEvicted = null)
        {
            Guard.GreaterThan(capacity, 0);

            this.capacity = capacity;

            this.itemEvicted = itemEvicted ?? ((key, value) => { });
        }

        public bool Set(TKey key, TValue value)
        {
            if (cacheMap.TryGetValue(key, out var node))
            {
                node.Value.Value = value;

                cacheHistory.Remove(node);
                cacheHistory.AddLast(node);

                cacheMap[key] = node;

                return true;
            }

            if (cacheMap.Count >= capacity)
            {
                RemoveFirst();
            }

            var cacheItem = new LRUCacheItem<TKey, TValue> { Key = key, Value = value };

            node = new LinkedListNode<LRUCacheItem<TKey, TValue>>(cacheItem);

            cacheMap.Add(key, node);
            cacheHistory.AddLast(node);

            return false;
        }

        public bool Remove(TKey key)
        {
            if (cacheMap.TryGetValue(key, out var node))
            {
                cacheMap.Remove(key);
                cacheHistory.Remove(node);

                return true;
            }

            return false;
        }

        public bool TryGetValue(TKey key, out object? value)
        {
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

        public bool Contains(TKey key)
        {
            return cacheMap.ContainsKey(key);
        }

        private void RemoveFirst()
        {
            var node = cacheHistory.First;

            if (node != null)
            {
                itemEvicted(node.Value.Key, node.Value.Value);

                cacheMap.Remove(node.Value.Key);
                cacheHistory.RemoveFirst();
            }
        }
    }
}