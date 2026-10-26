// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Squidex.Infrastructure;

public sealed class PrefixModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        // The table names depend on the prefix, therefore each prefix needs its own model.
        var prefix = context.GetService<IDbContextOptions>().FindExtension<PrefixExtension>()?.Prefix;

        return (context.GetType(), prefix, designTime);
    }
}
