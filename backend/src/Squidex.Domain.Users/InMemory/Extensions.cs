// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Users.InMemory;

public static class Extensions
{
    public static ValueTask<T> AsValueTask<T>(this T value)
    {
        return new ValueTask<T>(value);
    }
}
