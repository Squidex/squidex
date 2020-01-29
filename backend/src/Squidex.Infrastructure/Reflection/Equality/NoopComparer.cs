// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Reflection.Equality
{
    internal sealed class NoopComparer : IDeepComparer
    {
        public bool IsEquals(object? x, object? y)
        {
            return false;
        }
    }
}
