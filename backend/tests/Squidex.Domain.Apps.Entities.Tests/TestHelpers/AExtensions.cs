// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.TestHelpers;

public static class AExtensions
{
    public static Q HasQuery(this INegatableArgumentConstraintManager<Q> that, string query)
    {
        return that.Matches(x => x.Query!.ToString() == query);
    }

    public static Q HasIdsWithoutTotal(this INegatableArgumentConstraintManager<Q> that, params DomainId[] ids)
    {
        return that.Matches(x => x.Ids != null && x.Ids.SetEquals(ids) && x.NoTotal);
    }

    public static Q HasIds(this INegatableArgumentConstraintManager<Q> that, params DomainId[] ids)
    {
        return that.Matches(x => x.Ids != null && x.Ids.SetEquals(ids));
    }

    public static Q HasIds(this INegatableArgumentConstraintManager<Q> that, IEnumerable<DomainId> ids)
    {
        return that.Matches(x => x.Ids != null && x.Ids.SetEquals(ids.ToHashSet()));
    }

    public static ClrQuery Is(this INegatableArgumentConstraintManager<ClrQuery> that, string query)
    {
        return that.Matches(x => x.ToString() == query);
    }
}
