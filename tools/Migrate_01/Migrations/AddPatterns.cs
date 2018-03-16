// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Infrastructure.Migrations;

namespace Migrate_01.Migrations
{
    public sealed class AddPatterns : IMigration
    {
        private readonly InitialPatterns initialPatterns;
        private readonly IGrainFactory grainFactory;
        private readonly IAppRepository appRepository;

        public AddPatterns(InitialPatterns initialPatterns, IAppRepository appRepository, IGrainFactory grainFactory)
        {
            this.initialPatterns = initialPatterns;
            this.appRepository = appRepository;
            this.grainFactory = grainFactory;
        }

        public async Task UpdateAsync()
        {
            var ids = await appRepository.QueryAppIdsAsync();

            foreach (var id in ids)
            {
                var app = grainFactory.GetGrain<IAppGrain>(id);

                var state = await app.GetStateAsync();

                if (state.Value.Patterns.Count == 0)
                {
                    foreach (var pattern in initialPatterns.Values)
                    {
                        var command =
                            new AddPattern
                            {
                                Actor = state.Value.CreatedBy,
                                AppId = state.Value.Id,
                                Name = pattern.Name,
                                PatternId = Guid.NewGuid(),
                                Pattern = pattern.Pattern,
                                Message = pattern.Message
                            };

                        await app.ExecuteAsync(command);
                    }

                    await app.WriteSnapshotAsync();
                }
            }
        }
    }
}