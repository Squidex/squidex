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

namespace Squidex.Core.Contents
{
    public sealed class ContentFieldData : Dictionary<string, JToken>, IEquatable<ContentFieldData>
    {
        public ContentFieldData()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public ContentFieldData SetValue(JToken value)
        {
            this[Language.Invariant.Iso2Code] = value;

            return this;
        }

        public ContentFieldData AddValue(string language, JToken value)
        {
            Guard.NotNullOrEmpty(language, nameof(language));

            this[language] = value;

            return this;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ContentFieldData);
        }

        public bool Equals(ContentFieldData other)
        {
            return other != null && (ReferenceEquals(this, other) || this.EqualsDictionary(other));
        }

        public override int GetHashCode()
        {
            return this.DictionaryHashCode();
        }
    }
}