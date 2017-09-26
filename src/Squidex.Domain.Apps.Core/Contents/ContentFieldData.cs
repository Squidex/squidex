// ==========================================================================
//  ContentFieldData.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Contents
{
    public sealed class ContentFieldData : Dictionary<string, JToken>, IEquatable<ContentFieldData>
    {
        private static readonly JTokenEqualityComparer JTokenEqualityComparer = new JTokenEqualityComparer();

        public ContentFieldData()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public ContentFieldData SetValue(JToken value)
        {
            this[InvariantPartitioning.Instance.Master.Key] = value;

            return this;
        }

        public ContentFieldData AddValue(string key, JToken value)
        {
            Guard.NotNullOrEmpty(key, nameof(key));

            this[key] = value;

            return this;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ContentFieldData);
        }

        public bool Equals(ContentFieldData other)
        {
            return other != null && (ReferenceEquals(this, other) || this.EqualsDictionary(other, EqualityComparer<string>.Default, JTokenEqualityComparer));
        }

        public override int GetHashCode()
        {
            return this.DictionaryHashCode(EqualityComparer<string>.Default, JTokenEqualityComparer);
        }
    }
}