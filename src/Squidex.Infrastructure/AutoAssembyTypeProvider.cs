// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure
{
    public sealed class AutoAssembyTypeProvider<T> : ITypeProvider
    {
        public void Map(TypeNameRegistry typeNameRegistry)
        {
            typeNameRegistry.MapUnmapped(typeof(T).Assembly);
        }
    }
}
