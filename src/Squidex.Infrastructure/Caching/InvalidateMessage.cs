// ==========================================================================
//  InvalidateMessage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.Caching
{
    public sealed class InvalidateMessage
    {
        public string CacheKey { get; set; }
    }
}
