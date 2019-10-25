﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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

        public static bool IsValidPartitioning(this string? value)
        {
            return value == null || AllowedPartitions.Contains(value);
        }
    }
}
