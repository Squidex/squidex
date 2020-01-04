﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.TestHelpers
{
    public static class AExtensions
    {
        public static ClrQuery Is(this INegatableArgumentConstraintManager<ClrQuery> that, string query)
        {
            return that.Matches(x => x.ToString() == query);
        }

        public static T[] Is<T>(this INegatableArgumentConstraintManager<T[]> that, params T[]? values)
        {
            return values == null ? that.IsNull() : that.IsSameSequenceAs(values);
        }

        public static IEnumerable<T> Has<T>(this INegatableArgumentConstraintManager<IEnumerable<T>> that, params T[]? values)
        {
            return values == null ? that.IsNull() : that.Matches(x => x.Intersect(values).Count() == values.Length);
        }

        public static HashSet<T> Has<T>(this INegatableArgumentConstraintManager<HashSet<T>> that, params T[]? values)
        {
            return values == null ? that.IsNull() : that.Matches(x => x.Intersect(values).Count() == values.Length);
        }
    }
}
