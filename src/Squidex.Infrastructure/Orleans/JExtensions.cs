// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Orleans
{
    public static class JExtensions
    {
        public static J<T> AsJ<T>(this T value)
        {
            return new J<T>(value);
        }
    }
}
