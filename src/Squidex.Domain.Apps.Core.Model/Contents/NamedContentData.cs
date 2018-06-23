// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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

        public NamedContentData(int capacity)
            : base(capacity, StringComparer.OrdinalIgnoreCase)
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
