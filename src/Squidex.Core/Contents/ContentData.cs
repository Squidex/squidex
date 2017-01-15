// ==========================================================================
//  ContentData.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;

namespace Squidex.Core.Contents
{
    public sealed class ContentData
    {
        private readonly ImmutableDictionary<string, ContentFieldData> fields;

        public static readonly ContentData Empty = new ContentData(ImmutableDictionary<string, ContentFieldData>.Empty.WithComparers (StringComparer.OrdinalIgnoreCase));

        public ImmutableDictionary<string, ContentFieldData> Fields
        {
            get { return fields; }
        }

        public ContentData(ImmutableDictionary<string, ContentFieldData> fields)
        {
            Guard.NotNull(fields, nameof(fields));

            this.fields = fields;
        }

        public ContentData AddField(string fieldName, ContentFieldData data)
        {
            Guard.ValidPropertyName(fieldName, nameof(fieldName));

            return new ContentData(Fields.Add(fieldName, data));
        }

        public static ContentData Create(Dictionary<string, Dictionary<string, JToken>> raw)
        {
            return new ContentData(raw.ToImmutableDictionary(x => x.Key, x => new ContentFieldData(x.Value.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase)), StringComparer.OrdinalIgnoreCase));
        }

        public Dictionary<string, Dictionary<string, JToken>> ToRaw()
        {
            return fields.ToDictionary(x => x.Key, x => x.Value.ValueByLanguage.ToDictionary(y => y.Key, y => y.Value));
        }
    }
}
