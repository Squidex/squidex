// ==========================================================================
//  ContentData.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Immutable;
using Squidex.Infrastructure;

namespace Squidex.Core.Contents
{
    public sealed class ContentData
    {
        private readonly ImmutableDictionary<string, ContentFieldData> fields;

        public ImmutableDictionary<string, ContentFieldData> Fields
        {
            get { return fields; }
        }

        public ContentData(ImmutableDictionary<string, ContentFieldData> fields)
        {
            Guard.NotNull(fields, nameof(fields));

            this.fields = fields;
        }

        public static ContentData Empty()
        {
            return new ContentData(ImmutableDictionary<string, ContentFieldData>.Empty.WithComparers(StringComparer.OrdinalIgnoreCase));
        }

        public ContentData AddField(string fieldName, ContentFieldData data)
        {
            Guard.ValidPropertyName(fieldName, nameof(fieldName));

            return new ContentData(Fields.Add(fieldName, data));
        }
    }
}
