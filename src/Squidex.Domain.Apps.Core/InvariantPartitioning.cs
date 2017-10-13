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
    public sealed class InvariantPartitioning : IFieldPartitioning, IFieldPartitionItem
    {
        public static readonly InvariantPartitioning Instance = new InvariantPartitioning();

        public int Count
        {
            get { return 1; }
        }

        public IFieldPartitionItem Master
        {
            get { return this; }
        }

        string IFieldPartitionItem.Key
        {
            get { return "iv"; }
        }

        string IFieldPartitionItem.Name
        {
            get { return "Invariant"; }
        }

        bool IFieldPartitionItem.IsOptional
        {
            get { return false; }
        }

        IEnumerable<string> IFieldPartitionItem.Fallback
        {
            get { return Enumerable.Empty<string>(); }
        }

        private InvariantPartitioning()
        {
        }

        public bool TryGetItem(string key, out IFieldPartitionItem item)
        {
            var isFound = string.Equals(key, "iv", StringComparison.OrdinalIgnoreCase);

            item = isFound ? this : null;

            return isFound;
        }

        IEnumerator<IFieldPartitionItem> IEnumerable<IFieldPartitionItem>.GetEnumerator()
        {
            yield return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }
    }
}
