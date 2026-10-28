// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure;

public readonly struct Paging(int skip, int take)
{
    public const int DefaultTake = 20;
    public const int MaxTake = 200;

    public int Skip { get; } = Math.Max(0, skip);

    public int Take { get; } = take > 0 ? Math.Min(take, MaxTake) : DefaultTake;
}
