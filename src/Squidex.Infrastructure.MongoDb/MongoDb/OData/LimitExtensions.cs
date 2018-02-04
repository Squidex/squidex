// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.OData.UriParser;
using MongoDB.Driver;

namespace Squidex.Infrastructure.MongoDb.OData
{
    public static class LimitExtensions
    {
        public static IFindFluent<T, T> Take<T>(this IFindFluent<T, T> cursor, ODataUriParser query, int maxValue = 200, int defaultValue = 20)
        {
            var top = query.ParseTop();

            if (top.HasValue)
            {
                cursor = cursor.Limit(Math.Min((int)top.Value, maxValue));
            }
            else
            {
                cursor = cursor.Limit(defaultValue);
            }

            return cursor;
        }

        public static IFindFluent<T, T> Skip<T>(this IFindFluent<T, T> cursor, ODataUriParser query)
        {
            var skip = query.ParseSkip();

            if (skip.HasValue)
            {
                cursor = cursor.Skip((int)skip.Value);
            }
            else
            {
                cursor = cursor.Skip(null);
            }

            return cursor;
        }
    }
}
