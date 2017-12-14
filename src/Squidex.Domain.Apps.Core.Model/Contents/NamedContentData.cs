// ==========================================================================
//  NamedContentData.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Contents
{
    public sealed class NamedContentData : ContentData<string>, IEquatable<NamedContentData>
    {
        public NamedContentData()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public NamedContentData(NamedContentData copy)
            : base(copy, EqualityComparer<string>.Default)
        {
        }

        public static NamedContentData Merge(params NamedContentData[] contents)
        {
            return MergeTo(new NamedContentData(), contents);
        }

        public NamedContentData MergeInto(NamedContentData target)
        {
            return Merge(target, this);
        }

        public NamedContentData ToCleaned()
        {
            return Clean(this, new NamedContentData());
        }

        public NamedContentData AddField(string name, ContentFieldData data)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            this[name] = data;

            return this;
        }

        public bool Equals(NamedContentData other)
        {
            return base.Equals(other);
        }
    }
}
