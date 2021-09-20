// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject.Guards
{
    public sealed class ScriptMetadataWrapper : IDictionary<string, object?>
    {
        private readonly AssetMetadata metadata;

        public int Count
        {
            get => metadata.Count;
        }

        public ICollection<string> Keys
        {
            get => metadata.Keys;
        }

        public ICollection<object?> Values
        {
            get => metadata.Values.Cast<object?>().ToList();
        }

        public object? this[string key]
        {
            get => metadata[key];
            set => metadata[key] = JsonValue.Create(value);
        }

        public bool IsReadOnly
        {
            get => false;
        }

        public ScriptMetadataWrapper(AssetMetadata metadata)
        {
            this.metadata = metadata;
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object? value)
        {
            if (metadata.TryGetValue(key, out var temp))
            {
                value = temp;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        public void Add(string key, object? value)
        {
            metadata.Add(key, JsonValue.Create(value));
        }

        public void Add(KeyValuePair<string, object?> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Remove(string key)
        {
            return metadata.Remove(key);
        }

        public bool Remove(KeyValuePair<string, object?> item)
        {
            return false;
        }

        public void Clear()
        {
            metadata.Clear();
        }

        public bool Contains(KeyValuePair<string, object?> item)
        {
            return false;
        }

        public bool ContainsKey(string key)
        {
            return metadata.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
        {
            var i = arrayIndex;

            foreach (var item in metadata)
            {
                if (i >= array.Length)
                {
                    break;
                }

                array[i] = new KeyValuePair<string, object?>(item.Key, item.Value);
                i++;
            }
        }

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            return metadata.Select(x => new KeyValuePair<string, object?>(x.Key, x.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)metadata).GetEnumerator();
        }
    }
}
