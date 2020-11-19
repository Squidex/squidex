// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Contents
{
    public abstract class ContentData<T> : Dictionary<T, ContentFieldData?>, IEquatable<ContentData<T>> where T : notnull
    {
        public IEnumerable<KeyValuePair<T, ContentFieldData?>> ValidValues
        {
            get { return this.Where(x => x.Value != null); }
        }

        protected ContentData(IEqualityComparer<T> comparer)
            : base(comparer)
        {
        }

        protected ContentData(ContentData<T> source, IEqualityComparer<T> comparer)
            : base(source, comparer)
        {
        }

        protected ContentData(int capacity, IEqualityComparer<T> comparer)
            : base(capacity, comparer)
        {
        }

        protected static TResult MergeTo<TResult>(TResult target, params TResult[] sources) where TResult : ContentData<T>
        {
            Guard.NotEmpty(sources, nameof(sources));

            if (sources.Length == 1 || sources.Skip(1).All(x => ReferenceEquals(x, sources[0])))
            {
                return sources[0];
            }

            foreach (var source in sources)
            {
                foreach (var (key, contentFieldData) in source)
                {
                    if (contentFieldData != null)
                    {
                        var fieldValue = target.GetOrAdd(key, x => new ContentFieldData());

                        if (fieldValue != null)
                        {
                            foreach (var (fieldName, value) in contentFieldData)
                            {
                                fieldValue[fieldName] = value;
                            }
                        }
                    }
                }
            }

            return target;
        }

        protected static TResult Clean<TResult>(TResult source, TResult target) where TResult : ContentData<T>
        {
            foreach (var fieldValue in source.ValidValues)
            {
                if (fieldValue.Value != null)
                {
                    var resultValue = new ContentFieldData();

                    foreach (var (key, value) in fieldValue.Value.Where(x => x.Value.Type != JsonValueType.Null))
                    {
                        resultValue[key] = value;
                    }

                    if (resultValue.Count > 0)
                    {
                        target[fieldValue.Key] = resultValue;
                    }
                }
            }

            return target;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ContentData<T>);
        }

        public bool Equals(ContentData<T>? other)
        {
            return other != null && (ReferenceEquals(this, other) || this.EqualsDictionary(other));
        }

        public override int GetHashCode()
        {
            return this.DictionaryHashCode();
        }
    }
}
