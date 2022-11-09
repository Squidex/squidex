// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure;

public static class NamedId
{
    public static NamedId<T> Of<T>(T id, string name) where T : notnull
    {
        return new NamedId<T>(id, name);
    }
}
