// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Migrate_01.Migrations;
using Squidex.Infrastructure.Migrations;

namespace Migrate_01
{
    public sealed class MigrationPath : IMigrationPath
    {
        private const int CurrentVersion = 6;
        private readonly IServiceProvider serviceProvider;

        public MigrationPath(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public (int Version, IEnumerable<IMigration> Migrations) GetNext(int version)
        {
            if (version == CurrentVersion)
            {
                return (CurrentVersion, null);
            }

            var migrations = new List<IMigration>();

            // Version 6: Convert Event store. Must always be executed first.
            if (version < 6)
            {
                migrations.Add(serviceProvider.GetRequiredService<ConvertEventStore>());
            }

            // Version 5: Fixes the broken command architecture and requires a rebuild of all snapshots.
            if (version < 5)
            {
                migrations.Add(serviceProvider.GetRequiredService<RebuildSnapshots>());
            }

            // Version 1: Introduce App patterns.
            if (version <= 1)
            {
                migrations.Add(serviceProvider.GetRequiredService<AddPatterns>());
            }

            return (CurrentVersion, migrations);
        }
    }
}
