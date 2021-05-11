// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public abstract class FieldBase
    {
        private Dictionary<string, object> metadata;

        public long Id { get; }

        public string Name { get; }

        public IDictionary<string, object> Metadata
        {
            get => metadata ??= new Dictionary<string, object>();
        }

        protected FieldBase(long id, string name)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.GreaterThan(id, 0, nameof(id));

            Id = id;

            Name = name;
        }

        public T? GetMetadata<T>(string key, T? defaultValue = default)
        {
            var local = metadata;

            return local != null && local.TryGetValue(key, out var item) ? (T)item : defaultValue;
        }

        public T GetMetadata<T>(string key, Func<T> defaultValueFactory)
        {
            var local = metadata;

            return local != null && local.TryGetValue(key, out var item) ? (T)item : defaultValueFactory();
        }

        public bool HasMetadata(string key)
        {
            var local = metadata;

            return local?.ContainsKey(key) == true;
        }
    }
}
