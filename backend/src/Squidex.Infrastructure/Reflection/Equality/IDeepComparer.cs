// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Reflection.Equality
{
    public interface IDeepComparer
    {
        bool IsEquals(object? x, object? y);
    }
}
