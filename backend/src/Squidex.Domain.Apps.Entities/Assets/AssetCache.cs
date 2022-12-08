// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;

namespace Squidex.Domain.Apps.Entities.Assets;

public sealed class AssetCache : QueryCache<DomainId, IEnrichedAssetEntity>, IAssetCache
{
    public AssetCache(IMemoryCache? memoryCache, IOptions<AssetOptions> options)
        : base(options.Value.CanCache ? memoryCache : null)
    {
    }
}
