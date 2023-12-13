﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets;

public interface IAssetLoader
{
    Task<Asset?> GetAsync(DomainId appId, DomainId id, long version = EtagVersion.Any,
        CancellationToken ct = default);
}
