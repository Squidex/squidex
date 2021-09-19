// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;

namespace Squidex.Infrastructure.Queries
{
    public class Query<TValue>
    {
        public FilterNode<TValue>? Filter { get; set; }

        public string? FullText { get; set; }

        public long Skip { get; set; }

        public long Take { get; set; } = long.MaxValue;

        public long Top
        {
            set => Take = value;
        }

        public List<SortNode> Sort { get; set; } = new List<SortNode>();

        public HashSet<string> GetAllFields()
        {
            var result = new HashSet<string>();

            if (Sort != null)
            {
                foreach (var sorting in Sort)
                {
                    result.Add(sorting.Path.ToString());
                }
            }

            Filter?.AddFields(result);

            return result;
        }

        public override string ToString()
        {
            var parts = new List<string>();

            if (Filter != null)
            {
                parts.Add($"Filter: {Filter}");
            }

            if (FullText != null)
            {
                parts.Add($"FullText: '{FullText.Replace("'", "\'", StringComparison.Ordinal)}'");
            }

            if (Skip > 0)
            {
                parts.Add($"Skip: {Skip}");
            }

            if (Take < long.MaxValue)
            {
                parts.Add($"Take: {Take}");
            }

            if (Sort.Count > 0)
            {
                parts.Add($"Sort: {string.Join(", ", Sort)}");
            }

            return string.Join("; ", parts);
        }
    }
}
