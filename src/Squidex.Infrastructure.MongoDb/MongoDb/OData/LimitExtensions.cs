// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Infrastructure.Queries;

namespace Squidex.Infrastructure.MongoDb.OData
{
    public static class LimitExtensions
    {
        public static IFindFluent<T, T> Take<T>(this IFindFluent<T, T> cursor, Query query)
        {
            if (query.Take < long.MaxValue)
            {
                cursor = cursor.Limit((int)query.Take);
            }

            return cursor;
        }

        public static IFindFluent<T, T> Skip<T>(this IFindFluent<T, T> cursor, Query query)
        {
            if (query.Skip > 0)
            {
                cursor = cursor.Skip((int)query.Skip);
            }

            return cursor;
        }
    }
}
