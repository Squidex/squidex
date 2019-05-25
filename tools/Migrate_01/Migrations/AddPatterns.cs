// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Reflection;

namespace Migrate_01.Migrations
{
    public sealed class AddPatterns : IMigration
    {
        private readonly InitialPatterns initialPatterns;
        private readonly IGrainFactory grainFactory;

        public AddPatterns(InitialPatterns initialPatterns, IGrainFactory grainFactory)
        {
            this.initialPatterns = initialPatterns;

            this.grainFactory = grainFactory;
        }

        public async Task UpdateAsync()
        {
            var ids = await grainFactory.GetGrain<IAppsByNameIndex>(SingleGrain.Id).GetAppIdsAsync();

            var command = new ConfigurePatterns
            {
                Patterns = initialPatterns.Select(p => SimpleMapper.Map(p, new UpsertAppPattern())).ToArray()
            };

            foreach (var id in ids)
            {
                var app = grainFactory.GetGrain<IAppGrain>(id);

                var state = await app.GetStateAsync();

                if (state.Value.Patterns.Count == 0)
                {
                    command.AppId = state.Value.Id;
                    command.Actor = state.Value.CreatedBy;

                    await app.ExecuteAsync(command);
                }
            }
        }
    }
}