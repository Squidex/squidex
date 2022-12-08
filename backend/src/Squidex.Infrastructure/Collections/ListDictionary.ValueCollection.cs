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
    private sealed class ValueCollection : ICollection<TValue>
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

        public ValueCollection(ListDictionary<TKey, TValue> dictionary)
        {
            this.dictionary = dictionary;
        }

        public void Add(TValue item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            var i = 0;
            foreach (var (_, value) in dictionary.entries)
            {
                array[arrayIndex + i] = value;
                i++;
            }
        }

        public bool Remove(TValue item)
        {
            throw new NotSupportedException();
        }

        public bool Contains(TValue item)
        {
            foreach (var entry in dictionary.entries)
            {
                if (Equals(entry.Value, item))
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return new Enumerator(dictionary);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(dictionary);
        }

        private struct Enumerator : IEnumerator<TValue>, IEnumerator
        {
            private readonly ListDictionary<TKey, TValue> dictionary;
            private int index = -1;
            private TValue value = default!;

            readonly TValue IEnumerator<TValue>.Current
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

                value = dictionary.entries[index].Value;
                return true;
            }

            public void Reset()
            {
                index = -1;
            }
        }
    }
}
