// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Commands;

public sealed class DomainObjectCacheOptions
{
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(10);
}
