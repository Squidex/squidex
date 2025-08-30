// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Reflection;

public sealed class DelegateTypeProvider(Action<TypeRegistry> action) : ITypeProvider
{
    public void Map(TypeRegistry typeRegistry)
    {
        action(typeRegistry);
    }
}
