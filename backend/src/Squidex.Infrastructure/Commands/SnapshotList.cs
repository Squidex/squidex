// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace Squidex.Infrastructure.Commands
{
    public sealed class SnapshotList<T> where T : class, IDomainState<T>, new()
    {
        private readonly List<T?> items = new List<T?>(2);
        private int capacity = 2;

        public int Capacity
        {
            get => capacity;
            set
            {
                value = Math.Max(1, value);

                if (capacity != value)
                {
                    capacity = value;

                    Clean();
                }
            }
        }

        public long Version
        {
            get => items.Count - 2;
        }

        public T Current
        {
            get => items[^1]!;
        }

        public SnapshotList()
        {
            Clear();
        }

        public void Clear()
        {
            items.Clear();
            items.Add(new T
            {
                Version = EtagVersion.Empty
            });
        }

        public (T?, bool Valid) Get(long version)
        {
            if (version == EtagVersion.Any || version == EtagVersion.Auto)
            {
                return (Current, true);
            }

            var index = GetIndex(version);

            if (index >= 0 && index < items.Count)
            {
                return (items[index], true);
            }

            return (null, false);
        }

        public bool Contains(long version)
        {
            var index = GetIndex(version);

            return items.ElementAtOrDefault(index) != null;
        }

        public void Add(T snapshot, long version, bool clean = false)
        {
            var index = GetIndex(version);

            while (items.Count <= index)
            {
                items.Add(null);
            }

            items[index] = snapshot;

            if (clean)
            {
                Clean();
            }
        }

        public void ResetTo(T snapshot, long version)
        {
            var index = GetIndex(version);

            while (items.Count > index + 1)
            {
                items.RemoveAt(items.Count - 1);
            }

            items[index] = snapshot;
        }

        private void Clean()
        {
            var lastIndex = items.Count - 1;

            for (var i = lastIndex - capacity; i > 0; i--)
            {
                items[i] = null;
            }
        }

        private static int GetIndex(long version)
        {
            return (int)(version + 1);
        }
    }
}
