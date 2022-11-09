// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Queries;

public sealed class SortNode
{
    public PropertyPath Path { get; }

    public SortOrder Order { get; set; }

    public SortNode(PropertyPath path, SortOrder order)
    {
        Guard.NotNull(path);
        Guard.Enum(order);

        Path = path;

        Order = order;
    }

    public override string ToString()
    {
        var path = string.Join(".", Path);

        return $"{path} {Order}";
    }
}