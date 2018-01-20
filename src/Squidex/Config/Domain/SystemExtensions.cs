// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
        public static void InitializeAll(this IServiceProvider services)
        {
            var systems = services.GetRequiredService<IEnumerable<IInitializable>>();

            foreach (var system in systems)
            {
                system.Initialize();
            }
        }

        public static void RunAll(this IServiceProvider services)
        {
            var systems = services.GetRequiredService<IEnumerable<IRunnable>>();

            foreach (var system in systems)
            {
                system.Run();
            }
        }

        public static void Migrate(this IServiceProvider services)
        {
            var migrator = services.GetRequiredService<Migrator>();

            migrator.MigrateAsync().Wait();
        }
    }
}
