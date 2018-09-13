// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.OData.UriParser;

namespace Squidex.Infrastructure.Queries.OData
{
    public static class LimitExtensions
    {
        public static void ParseTake(this ODataUriParser query, Query result)
        {
            var top = query.ParseTop();

            if (top.HasValue)
            {
                result.Take = top.Value;
            }
        }

        public static void ParseSkip(this ODataUriParser query, Query result)
        {
            var skip = query.ParseSkip();

            if (skip.HasValue)
            {
                result.Skip = skip.Value;
            }
        }
    }
}
