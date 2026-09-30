// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Caching;

public sealed class CacheGenerationsOptions
{
    public TimeSpan ResetInterval { get; set; } = TimeSpan.FromSeconds(1);
}
