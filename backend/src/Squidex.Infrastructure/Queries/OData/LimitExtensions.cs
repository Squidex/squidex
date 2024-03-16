// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.OData.UriParser;

namespace Squidex.Infrastructure.Queries.OData
{
    public static class LimitExtensions
    {
        public static void ParseTake(this ODataUriParser query, ClrQuery result)
        {
            var top = query.ParseTop();

            if (top != null)
            {
                result.Take = top.Value;
            }
        }

        public static void ParseSkip(this ODataUriParser query, ClrQuery result)
        {
            var skip = query.ParseSkip();

            if (skip != null)
            {
                result.Skip = skip.Value;
            }
        }
    }
}
