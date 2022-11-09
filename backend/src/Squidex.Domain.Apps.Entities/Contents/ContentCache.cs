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

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed class ContentCache : QueryCache<DomainId, IEnrichedContentEntity>, IContentCache
{
    public ContentCache(IMemoryCache? memoryCache, IOptions<ContentOptions> options)
        : base(options.Value.CanCache ? memoryCache : null)
    {
    }
}
