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
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.States;

namespace Migrate_01.Migrations
{
    public sealed class AddPatterns : IMigration
    {
        private readonly InitialPatterns initialPatterns;
        private readonly IStateFactory stateFactory;
        private readonly IAppRepository appRepository;

        public AddPatterns(InitialPatterns initialPatterns, IAppRepository appRepository, IStateFactory stateFactory)
        {
            this.initialPatterns = initialPatterns;
            this.appRepository = appRepository;
            this.stateFactory = stateFactory;
        }

        public async Task UpdateAsync()
        {
            var ids = await appRepository.QueryAppIdsAsync();

            foreach (var id in ids)
            {
                var app = await stateFactory.GetSingleAsync<AppGrain>(id);

                if (app.Snapshot.Patterns.Count == 0)
                {
                    foreach (var pattern in initialPatterns.Values)
                    {
                        var command =
                            new AddPattern
                            {
                                Actor = app.Snapshot.CreatedBy,
                                AppId = app.Snapshot.Id,
                                Name = pattern.Name,
                                PatternId = Guid.NewGuid(),
                                Pattern = pattern.Pattern,
                                Message = pattern.Message
                            };

                        await app.ExecuteAsync(command);
                    }
                }
            }
        }
    }
}
