// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.Contents;

internal static class Extensions
{
    public static bool ShouldWritePublished(this WriteContent content)
    {
        return content.CurrentVersion.Status == Status.Published && !content.IsDeleted;
    }

    public static SqlQueryBuilder WhereNotDeleted(this SqlQueryBuilder builder, Query<ClrValue>? query)
    {
        return builder.WhereNotDeleted(query?.Filter);
    }

    public static SqlQueryBuilder WhereNotDeleted(this SqlQueryBuilder builder, FilterNode<ClrValue>? filter)
    {
        if (filter?.HasField("IsDeleted") != true)
        {
            builder.Where(ClrFilter.Eq(nameof(EFContentEntity.IsDeleted), false));
        }

        return builder;
    }

    public static void LimitFields(this ContentData data, IReadOnlySet<string> fields)
    {
        List<string>? toDelete = null;
        foreach (var (key, value) in data)
        {
            if (!fields.Any(x => IsMatch(key, x)))
            {
                toDelete ??= [];
                toDelete.Add(key);
            }
        }

        if (toDelete != null)
        {
            foreach (var key in toDelete)
            {
                data.Remove(key);
            }
        }

        static bool IsMatch(string actual, string filter)
        {
            const string Prefix = "data.";

            var span = filter.AsSpan();
            if (span.Equals(actual, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (span.Length <= Prefix.Length || !span.StartsWith(Prefix, StringComparison.Ordinal))
            {
                return false;
            }

            span = span[Prefix.Length..];

            return span.Equals(actual, StringComparison.Ordinal);
        }
    }
}
