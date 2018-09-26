// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Infrastructure.Queries
{
    public sealed class SortNode
    {
        public IReadOnlyList<string> Path { get; }

        public SortOrder SortOrder { get; set; }

        public SortNode(IReadOnlyList<string> path, SortOrder sortOrder)
        {
            Guard.NotNull(path, nameof(path));
            Guard.NotEmpty(path, nameof(path));
            Guard.Enum(sortOrder, nameof(sortOrder));

            Path = path;

            SortOrder = sortOrder;
        }

        public override string ToString()
        {
            var path = string.Join(".", Path);

            return $"{path} {SortOrder}";
        }
    }
}