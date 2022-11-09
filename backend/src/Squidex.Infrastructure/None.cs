// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure;

public sealed record None
{
    public static readonly Type Type = typeof(None);

    public static readonly None Value = new None();

    private None()
    {
    }
}
