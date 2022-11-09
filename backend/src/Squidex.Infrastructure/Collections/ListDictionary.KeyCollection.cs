// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections;

namespace Squidex.Infrastructure.Collections;

public partial class ListDictionary<TKey, TValue>
{
    private sealed class KeyCollection : ICollection<TKey>
    {
        private readonly ListDictionary<TKey, TValue> dictionary;

        public int Count
        {
            get => dictionary.Count;
        }

        public bool IsReadOnly
        {
            get => false;
        }

        public KeyCollection(ListDictionary<TKey, TValue> dictionary)
        {
            this.dictionary = dictionary;
        }

        public void Add(TKey item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public void CopyTo(TKey[] array, int arrayIndex)
        {
            var i = 0;
            foreach (var (key, _) in dictionary.entries)
            {
                array[arrayIndex + i] = key;
                i++;
            }
        }

        public bool Remove(TKey item)
        {
            throw new NotSupportedException();
        }

        public bool Contains(TKey item)
        {
            foreach (var entry in dictionary.entries)
            {
                if (dictionary.comparer.Equals(entry.Key, item))
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerator<TKey> GetEnumerator()
        {
            return new Enumerator(dictionary);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(dictionary);
        }

        private struct Enumerator : IEnumerator<TKey>, IEnumerator
        {
            private readonly ListDictionary<TKey, TValue> dictionary;
            private int index = -1;
            private TKey value = default!;

            readonly TKey IEnumerator<TKey>.Current
            {
                get => value!;
            }

            readonly object IEnumerator.Current
            {
                get => value!;
            }

            public Enumerator(ListDictionary<TKey, TValue> dictionary)
            {
                this.dictionary = dictionary;
            }

            public readonly void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (index >= dictionary.entries.Count - 1)
                {
                    return false;
                }

                index++;

                value = dictionary.entries[index].Key;
                return true;
            }

            public void Reset()
            {
                index = -1;
            }
        }
    }
}
