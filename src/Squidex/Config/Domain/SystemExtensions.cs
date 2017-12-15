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
using Squidex.Infrastructure.Migrations;

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

        public static void Migrate(this IServiceProvider services)
        {
            var migrator = services.GetRequiredService<Migrator>();

            migrator.MigrateAsync().Wait();
        }
    }
}
