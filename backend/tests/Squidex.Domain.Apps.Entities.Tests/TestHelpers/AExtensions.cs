// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.TestHelpers
{
    public static class AExtensions
    {
        public static Q HasQuery(this INegatableArgumentConstraintManager<Q> that, string query)
        {
            return that.Matches(x => x.Query!.ToString() == query);
        }

        public static Q HasOData(this INegatableArgumentConstraintManager<Q> that, string odata)
        {
            return that.HasOData(odata, null);
        }

        public static Q HasOData(this INegatableArgumentConstraintManager<Q> that, string odata, DomainId? reference = null)
        {
            return that.Matches(x => x.ODataQuery == odata && x.Reference == reference);
        }

        public static ClrQuery Is(this INegatableArgumentConstraintManager<ClrQuery> that, string query)
        {
            return that.Matches(x => x.ToString() == query);
        }

        public static T[] Is<T>(this INegatableArgumentConstraintManager<T[]> that, params T[]? values)
        {
            return values == null ? that.IsNull() : that.IsSameSequenceAs(values);
        }

        public static HashSet<T> Is<T>(this INegatableArgumentConstraintManager<HashSet<T>> that, IEnumerable<T>? values)
        {
            return values == null ? that.IsNull() : that.Matches(x => x.Intersect(values).Count() == values.Count());
        }

        public static HashSet<T> Is<T>(this INegatableArgumentConstraintManager<HashSet<T>> that, params T[]? values)
        {
            return values == null ? that.IsNull() : that.Matches(x => x.Intersect(values).Count() == values.Length);
        }
    }
}
