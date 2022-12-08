// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;

namespace Squidex.Infrastructure.Queries;

public class Query<TValue>
{
    public FilterNode<TValue>? Filter { get; set; }

    public string? FullText { get; set; }

    public long Skip { get; set; }

    public long Take { get; set; } = long.MaxValue;

    public long Random { get; set; }

    public long Top
    {
        set => Take = value;
    }

    public List<SortNode>? Sort { get; set; } = new List<SortNode>();

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
        var sb = new StringBuilder();

        if (Filter != null)
        {
            sb.AppendIfNotEmpty("; ");
            sb.Append($"Filter: {Filter}");
        }

        if (FullText != null)
        {
            sb.AppendIfNotEmpty("; ");
            sb.Append($"FullText: '{FullText.Replace("'", "\'", StringComparison.Ordinal)}'");
        }

        if (Skip > 0)
        {
            sb.AppendIfNotEmpty("; ");
            sb.Append($"Skip: {Skip}");
        }

        if (Take < long.MaxValue)
        {
            sb.AppendIfNotEmpty("; ");
            sb.Append($"Take: {Take}");
        }

        if (Random > 0)
        {
            sb.AppendIfNotEmpty("; ");
            sb.Append($"Random: {Random}");
        }

        if (Sort != null && Sort.Count > 0)
        {
            sb.AppendIfNotEmpty("; ");
            sb.Append($"Sort: {string.Join(", ", Sort)}");
        }

        return sb.ToString();
    }
}
