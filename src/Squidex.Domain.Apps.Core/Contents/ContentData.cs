// ==========================================================================
//  ContentData.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.Contents
{
    public abstract class ContentData<T> : Dictionary<T, ContentFieldData>, IEquatable<ContentData<T>>
    {
        public IEnumerable<KeyValuePair<T, ContentFieldData>> ValidValues
        {
            get { return this.Where(x => x.Value != null); }
        }

        protected ContentData(IEqualityComparer<T> comparer)
            : base(comparer)
        {
        }

        protected ContentData(IDictionary<T, ContentFieldData> copy, IEqualityComparer<T> comparer)
            : base(copy, comparer)
        {
        }

        protected static TResult Merge<TResult>(TResult source, TResult target) where TResult : ContentData<T>
        {
            if (ReferenceEquals(target, source))
            {
                return source;
            }

            foreach (var otherValue in source)
            {
                var fieldValue = target.GetOrAdd(otherValue.Key, x => new ContentFieldData());

                foreach (var value in otherValue.Value)
                {
                    fieldValue[value.Key] = value.Value;
                }
            }

            return target;
        }

        protected static TResult Clean<TResult>(TResult source, TResult target) where TResult : ContentData<T>
        {
            foreach (var fieldValue in source.ValidValues)
            {
                var resultValue = new ContentFieldData();

                foreach (var partitionValue in fieldValue.Value.Where(x => !x.Value.IsNull()))
                {
                    resultValue[partitionValue.Key] = partitionValue.Value;
                }

                if (resultValue.Count > 0)
                {
                    target[fieldValue.Key] = resultValue;
                }
            }

            return target;
        }

        public IEnumerable<Guid> GetReferencedIds(Schema schema)
        {
            Guard.NotNull(schema, nameof(schema));

            var foundReferences = new HashSet<Guid>();

            foreach (var field in schema.Fields)
            {
                if (field is IReferenceField referenceField)
                {
                    var fieldKey = GetKey(field);
                    var fieldData = this.GetOrDefault(fieldKey);

                    if (fieldData == null)
                    {
                        continue;
                    }

                    foreach (var partitionValue in fieldData.Where(x => x.Value != null))
                    {
                        var ids = referenceField.GetReferencedIds(partitionValue.Value);

                        foreach (var id in ids.Where(x => foundReferences.Add(x)))
                        {
                            yield return id;
                        }
                    }
                }
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ContentData<T>);
        }

        public bool Equals(ContentData<T> other)
        {
            return other != null && (ReferenceEquals(this, other) || this.EqualsDictionary(other));
        }

        public override int GetHashCode()
        {
            return this.DictionaryHashCode();
        }

        public abstract T GetKey(Field field);
    }
}
