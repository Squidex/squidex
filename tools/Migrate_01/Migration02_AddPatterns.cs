// ==========================================================================
//  Migration02_AddPatterns.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.States;

namespace Migrate_01
{
    public sealed class Migration02_AddPatterns : IMigration
    {
        private readonly InitialPatterns initialPatterns;
        private readonly IStateFactory stateFactory;
        private readonly IAppRepository appRepository;

        public int FromVersion { get; } = 1;

        public int ToVersion { get; } = 2;

        public Migration02_AddPatterns(InitialPatterns initialPatterns, IAppRepository appRepository, IStateFactory stateFactory)
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
                var app = await stateFactory.CreateAsync<AppDomainObject>(id);

                if (app.Snapshot.Patterns.Count == 0)
                {
                    foreach (var pattern in initialPatterns.Values)
                    {
                        var command =
                            new AddPattern
                            {
                                Actor = app.Snapshot.CreatedBy,
                                AppId = new NamedId<Guid>(app.Snapshot.Id, app.Snapshot.Name),
                                Name = pattern.Name,
                                PatternId = Guid.NewGuid(),
                                Pattern = pattern.Pattern,
                                Message = pattern.Message
                            };

                        app.AddPattern(command);
                    }

                    await app.WriteAsync();
                }
            }
        }
    }
}
