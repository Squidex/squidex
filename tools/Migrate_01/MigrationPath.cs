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
        private readonly IServiceProvider serviceProvider;

        public MigrationPath(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public (int Version, IEnumerable<IMigration> Migrations) GetNext(int version)
        {
            switch (version)
            {
                case 0:
                    return (4,
                        new IMigration[]
                        {
                            serviceProvider.GetRequiredService<ConvertEventStore>(),
                            serviceProvider.GetRequiredService<RebuildSnapshots>(),
                            serviceProvider.GetRequiredService<AddPatterns>()
                        });
                case 1:
                    return (4,
                        new IMigration[]
                        {
                            serviceProvider.GetRequiredService<ConvertEventStore>(),
                            serviceProvider.GetRequiredService<AddPatterns>(),
                            serviceProvider.GetRequiredService<RebuildContentCollections>()
                        });
                case 2:
                    return (4,
                        new IMigration[]
                        {
                            serviceProvider.GetRequiredService<ConvertEventStore>(),
                            serviceProvider.GetRequiredService<AddPatterns>(),
                            serviceProvider.GetRequiredService<RebuildContentCollections>()
                        });
                case 3:
                    return (4,
                        new IMigration[]
                        {
                            serviceProvider.GetRequiredService<ConvertEventStore>()
                        });
            }

            return (0, null);
        }
    }
}
