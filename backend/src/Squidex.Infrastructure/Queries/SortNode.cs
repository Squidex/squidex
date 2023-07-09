// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Queries;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

public sealed record SortNode(PropertyPath Path, SortOrder Order)
{
    public override string ToString()
    {
        var path = string.Join('.', Path);

        return $"{path} {Order}";
    }
}
