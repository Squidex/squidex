// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using LibGit2Sharp;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.Contents;

internal static class Extensions
{
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
}
