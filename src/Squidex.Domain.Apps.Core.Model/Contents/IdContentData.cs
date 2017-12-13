// ==========================================================================
//  IdContentData.cs
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
    public sealed class IdContentData : ContentData<long>, IEquatable<IdContentData>
    {
        public IdContentData()
            : base(EqualityComparer<long>.Default)
        {
        }

        public IdContentData(IdContentData copy)
            : base(copy, EqualityComparer<long>.Default)
        {
        }

        public IdContentData MergeInto(IdContentData target)
        {
            return Merge(new IdContentData(), this, target);
        }

        public IdContentData ToCleaned()
        {
            return Clean(this, new IdContentData());
        }

        public IdContentData AddField(long id, ContentFieldData data)
        {
            Guard.GreaterThan(id, 0, nameof(id));

            this[id] = data;

            return this;
        }

        public bool Equals(IdContentData other)
        {
            return base.Equals(other);
        }
    }
}
