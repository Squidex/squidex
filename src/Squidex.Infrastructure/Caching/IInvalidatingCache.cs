// ==========================================================================
//  IInvalidatingCache.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
namespace Squidex.Infrastructure.Caching
{
    public interface IInvalidatingCache
    {
        void Invalidate(object key);
    }
}
