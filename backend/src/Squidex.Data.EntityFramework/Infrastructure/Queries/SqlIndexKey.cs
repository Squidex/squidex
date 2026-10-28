// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable MA0048 // File name must match type name
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Infrastructure.Queries;

public sealed record SqlIndexKey(string Expression, string Order, bool IsExpression);

public sealed class SqlIndexInfo
{
    public string Name { get; set; }

    public string? Description { get; set; }
}
