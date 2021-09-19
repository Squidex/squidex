// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using FakeItEasy;

namespace Squidex.Domain.Apps.Core.TestHelpers
{
    public static class AExtensions
    {
        public static T[] Is<T>(this INegatableArgumentConstraintManager<T[]> that, params T[]? values)
        {
            return values == null ? that.IsNull() : that.IsSameSequenceAs(values);
        }

        public static IEnumerable<T> Is<T>(this INegatableArgumentConstraintManager<IEnumerable<T>> that, params T[]? values)
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
