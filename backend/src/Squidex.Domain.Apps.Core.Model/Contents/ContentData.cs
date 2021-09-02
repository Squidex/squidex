// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Contents
{
    public sealed class ContentData : Dictionary<string, ContentFieldData?>, IEquatable<ContentData>
    {
        public ContentData()
            : base(StringComparer.Ordinal)
        {
        }

        public ContentData(ContentData source)
            : base(source, StringComparer.Ordinal)
        {
        }

        public ContentData(int capacity)
            : base(capacity, StringComparer.Ordinal)
        {
        }

        public ContentData AddField(string name, ContentFieldData? data)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            this[name] = data;

            return this;
        }

        private static ContentData MergeTo(ContentData target, params ContentData[] sources)
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
                        var fieldValue = target.GetOrAdd(key, _ => new ContentFieldData());

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

        public static ContentData Merge(params ContentData[] contents)
        {
            return MergeTo(new ContentData(), contents);
        }

        public ContentData MergeInto(ContentData target)
        {
            return Merge(target, this);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ContentData);
        }

        public bool Equals(ContentData? other)
        {
            return other != null && (ReferenceEquals(this, other) || this.EqualsDictionary(other));
        }

        public override int GetHashCode()
        {
            return this.DictionaryHashCode();
        }

        public override string ToString()
        {
            return $"{{{string.Join(", ", this.Select(x => $"\"{x.Key}\":{x.Value}"))}}}";
        }

        public ContentData Clone()
        {
            var clone = new ContentData(Count);

            foreach (var (key, value) in this)
            {
                clone[key] = value?.Clone()!;
            }

            return clone;
        }
    }
}
