// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.TestHelpers
{
    public static class AExtensions
    {
        public static Query Is(this INegatableArgumentConstraintManager<Query> that, string query)
        {
            return that.Matches(x => x.ToString() == query);
        }

        public static T[] Is<T>(this INegatableArgumentConstraintManager<T[]> that, params T[] values)
        {
            if (values == null)
            {
                return that.IsNull();
            }

            return that.IsSameSequenceAs(values);
        }
    }
}
