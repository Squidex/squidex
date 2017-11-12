// ==========================================================================
//  SystemExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure;

namespace Squidex.Config.Domain
{
    public static class SystemExtensions
    {
        public static void TestExternalSystems(this IServiceProvider services)
        {
            var systems = services.GetRequiredService<IEnumerable<IExternalSystem>>();

            foreach (var system in systems)
            {
                system.Connect();
            }
        }
    }
}
