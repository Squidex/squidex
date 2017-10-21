// ==========================================================================
//  PartitioningExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;

namespace Squidex.Domain.Apps.Core
{
    public static class PartitioningExtensions
    {
        private static readonly HashSet<string> AllowedPartitions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Partitioning.Language.Key,
            Partitioning.Invariant.Key
        };

        public static bool IsValidPartitioning(this string value)
        {
            return value == null || AllowedPartitions.Contains(value);
        }
    }
}
