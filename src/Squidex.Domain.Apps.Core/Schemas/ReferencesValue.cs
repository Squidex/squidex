// ==========================================================================
//  ReferencesValue.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class ReferencesValue
    {
        private static readonly List<Guid> EmptyReferencedIds = new List<Guid>();

        public IReadOnlyList<Guid> ContentIds { get; }

        public ReferencesValue(IReadOnlyList<Guid> assetIds)
        {
            ContentIds = assetIds ?? EmptyReferencedIds;
        }
    }
}
