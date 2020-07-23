// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Migrations;

namespace Migrations.Migrations
{
    public sealed class AddPatterns : IMigration
    {
        private readonly InitialPatterns initialPatterns;
        private readonly ICommandBus commandBus;
        private readonly IAppsIndex indexForApps;

        public AddPatterns(InitialPatterns initialPatterns, ICommandBus commandBus, IAppsIndex indexForApps)
        {
            this.indexForApps = indexForApps;
            this.initialPatterns = initialPatterns;
            this.commandBus = commandBus;
        }

        public async Task UpdateAsync()
        {
            var ids = await indexForApps.GetIdsAsync();

            foreach (var id in ids)
            {
                var app = await indexForApps.GetAppAsync(id, false);

                if (app != null && app.Patterns.Count == 0)
                {
                    foreach (var pattern in initialPatterns.Values)
                    {
                        var command =
                            new AddPattern
                            {
                                Actor = app.CreatedBy,
                                AppId = id,
                                Name = pattern.Name,
                                PatternId = Guid.NewGuid(),
                                Pattern = pattern.Pattern,
                                Message = pattern.Message
                            };

                        await commandBus.PublishAsync(command);
                    }
                }
            }
        }
    }
}