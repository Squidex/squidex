// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Squidex.Infrastructure;

public class InstantExtensions
{
    [Fact]
    public void Should_remove_ms_from_instant()
    {
        var source = Instant.FromUnixTimeMilliseconds((30 * 1000) + 100);

        Assert.Equal(Instant.FromUnixTimeSeconds(30), source.WithoutMs());
    }
}
