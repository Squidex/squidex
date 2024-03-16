// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Infrastructure.Queries;

namespace Squidex.Infrastructure.MongoDb.Queries
{
    public static class LimitExtensions
    {
        public static IAggregateFluent<T> QueryLimit<T>(this IAggregateFluent<T> cursor, ClrQuery query)
        {
            if (query.Take < long.MaxValue)
            {
                cursor = cursor.Limit((int)query.Take);
            }

            return cursor;
        }

        public static IFindFluent<T, T> QueryLimit<T>(this IFindFluent<T, T> cursor, ClrQuery query)
        {
            if (query.Take < long.MaxValue)
            {
                cursor = cursor.Limit((int)query.Take);
            }

            return cursor;
        }

        public static IAggregateFluent<T> QuerySkip<T>(this IAggregateFluent<T> cursor, ClrQuery query)
        {
            if (query.Skip > 0)
            {
                cursor = cursor.Skip((int)query.Skip);
            }

            return cursor;
        }

        public static IFindFluent<T, T> QuerySkip<T>(this IFindFluent<T, T> cursor, ClrQuery query)
        {
            if (query.Skip > 0)
            {
                cursor = cursor.Skip((int)query.Skip);
            }

            return cursor;
        }

        public static IFindFluent<T, T> QuerySort<T>(this IFindFluent<T, T> cursor, ClrQuery query)
        {
            return cursor.Sort(query.BuildSort<T>());
        }

        public static IAggregateFluent<T> QuerySort<T>(this IAggregateFluent<T> cursor, ClrQuery query)
        {
            return cursor.Sort(query.BuildSort<T>());
        }
    }
}
