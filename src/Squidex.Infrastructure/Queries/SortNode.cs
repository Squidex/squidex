// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Queries
{
    public sealed class SortNode
    {
        public PropertyPath Path { get; }

        public SortOrder SortOrder { get; set; }

        public SortNode(PropertyPath path, SortOrder sortOrder)
        {
            Guard.NotNull(path, nameof(path));
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