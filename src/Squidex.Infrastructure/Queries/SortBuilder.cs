// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;

namespace Squidex.Infrastructure.Queries
{
    public static class SortBuilder
    {
        public static SortNode Ascending(string path)
        {
            return new SortNode(path.Split('.', '/').ToList(), SortOrder.Ascending);
        }

        public static SortNode Descending(string path)
        {
            return new SortNode(path.Split('.', '/').ToList(), SortOrder.Descending);
        }
    }
}
