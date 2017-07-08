// ==========================================================================
//  IFieldPartitioning.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Core
{
    public interface IFieldPartitioning : IReadOnlyCollection<IFieldPartitionItem>
    {
        IFieldPartitionItem Master { get; }

        bool TryGetItem(string key, out IFieldPartitionItem item);
    }
}
