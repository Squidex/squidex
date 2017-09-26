// ==========================================================================
//  InvariantPartitioning.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Squidex.Domain.Apps.Core
{
    public sealed class InvariantPartitioning : IFieldPartitioning
    {
        public static readonly InvariantPartitioning Instance = new InvariantPartitioning();

        private readonly InvariantItem invariantItem = new InvariantItem();

        private sealed class InvariantItem : IFieldPartitionItem
        {
            public string Key { get; } = "iv";

            public string Name { get; } = "Invariant";

            public bool IsOptional { get; } = false;

            public IEnumerable<string> Fallback { get; } = Enumerable.Empty<string>();
        }

        public int Count
        {
            get { return 1; }
        }

        public IFieldPartitionItem Master
        {
            get { return invariantItem; }
        }

        private InvariantPartitioning()
        {
        }

        public bool TryGetItem(string key, out IFieldPartitionItem item)
        {
            var isFound = string.Equals(key, "iv", StringComparison.OrdinalIgnoreCase);

            item = isFound ? invariantItem : null;

            return isFound;
        }

        IEnumerator<IFieldPartitionItem> IEnumerable<IFieldPartitionItem>.GetEnumerator()
        {
            yield return invariantItem;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return invariantItem;
        }
    }
}
