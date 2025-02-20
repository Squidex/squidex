// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

internal static class Extensions
{
    public static SqlQueryBuilder WhereScope(this SqlQueryBuilder queryBuilder, SearchScope scope)
    {
        if (scope == SearchScope.All)
        {
            return queryBuilder.Where(ClrFilter.Eq("ServeAll", true));
        }
        else
        {
            return queryBuilder.Where(ClrFilter.Eq("ServePublished", true));
        }
    }

    public static IQueryable<EFTextIndexTextEntity> WhereScope(this IQueryable<EFTextIndexTextEntity> query, SearchScope scope)
    {
        if (scope == SearchScope.All)
        {
            return query.Where(x => x.ServeAll);
        }
        else
        {
            return query.Where(x => x.ServePublished);
        }
    }

    public static IQueryable<EFTextIndexGeoEntity> WhereScope(this IQueryable<EFTextIndexGeoEntity> query, SearchScope scope)
    {
        if (scope == SearchScope.All)
        {
            return query.Where(x => x.ServeAll);
        }
        else
        {
            return query.Where(x => x.ServePublished);
        }
    }
}
