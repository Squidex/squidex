// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Squidex.Infrastructure;

public static class InstantExtensions
{
    public static Instant WithoutMs(this Instant value)
    {
        return Instant.FromUnixTimeSeconds(value.ToUnixTimeSeconds());
    }

    public static Instant WithoutNs(this Instant value)
    {
        return Instant.FromUnixTimeMilliseconds(value.ToUnixTimeMilliseconds());
    }
}
