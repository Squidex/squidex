// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Builder;

namespace Squidex.Web;

public static class BuilderExtensions
{
    public static void UseWhenPath(this IApplicationBuilder builder, string path, Action<IApplicationBuilder> configurator)
    {
        builder.UseWhen(c => c.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase), configurator);
    }
}
