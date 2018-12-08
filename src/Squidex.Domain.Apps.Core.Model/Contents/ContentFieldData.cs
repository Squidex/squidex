// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Contents
{
    public sealed class ContentFieldData : Dictionary<string, IJsonValue>, IEquatable<ContentFieldData>
    {
        public ContentFieldData()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public ContentFieldData AddValue(string key, object value)
        {
            return AddJsonValue(key, JsonValue.Create(value));
        }

        public ContentFieldData AddJsonValue(string key, IJsonValue value)
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
            return other != null && (ReferenceEquals(this, other) || this.EqualsDictionary(other));
        }

        public override int GetHashCode()
        {
            return this.DictionaryHashCode();
        }
    }
}