// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Core.Queries
{
    public sealed class Query
    {
        public FilterNode Filter { get; set; }

        public long? Skip { get; set; }

        public long? Take { get; set; }

        public List<SortNode> Sort { get; } = new List<SortNode>();

        public string FullText { get; set; }
    }
}
