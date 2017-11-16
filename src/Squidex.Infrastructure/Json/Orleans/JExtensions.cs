// ==========================================================================
//  JExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.Json.Orleans
{
    public static class JExtensions
    {
        public static J<T> AsJ<T>(this T value, bool immutable = true)
        {
            return new J<T>(value, immutable);
        }
    }
}
