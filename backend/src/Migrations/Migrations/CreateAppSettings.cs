// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Migrations.OldEvents;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Migrations.Migrations
{
    public sealed class CreateAppSettings : IMigration
    {
        private readonly ICommandBus commandBus;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly IEventStore eventStore;

        public CreateAppSettings(ICommandBus commandBus,
            IEventDataFormatter eventDataFormatter,
            IEventStore eventStore)
        {
            this.commandBus = commandBus;
            this.eventDataFormatter = eventDataFormatter;
            this.eventStore = eventStore;
        }

        public async Task UpdateAsync()
        {
            var apps = new Dictionary<NamedId<DomainId>, Dictionary<DomainId, (string Name, string Pattern, string? Message)>>();

            await foreach (var storedEvent in eventStore.QueryAllAsync("^app\\-"))
            {
                var @event = eventDataFormatter.ParseIfKnown(storedEvent);

                if (@event != null)
                {
                    switch (@event.Payload)
                    {
                        case AppPatternAdded patternAdded:
                            {
                                var patterns = apps.GetOrAddNew(patternAdded.AppId);

                                patterns[patternAdded.PatternId] = (patternAdded.Name, patternAdded.Pattern, patternAdded.Message);
                                break;
                            }

                        case AppPatternUpdated patternUpdated:
                            {
                                var patterns = apps.GetOrAddNew(patternUpdated.AppId);

                                patterns[patternUpdated.PatternId] = (patternUpdated.Name, patternUpdated.Pattern, patternUpdated.Message);
                                break;
                            }

                        case AppPatternDeleted patternDeleted:
                            {
                                var patterns = apps.GetOrAddNew(patternDeleted.AppId);

                                patterns.Remove(patternDeleted.PatternId);
                                break;
                            }

                        case AppArchived appArchived:
                            {
                                apps.Remove(appArchived.AppId);
                                break;
                            }
                    }
                }
            }

            var actor = RefToken.Client("Migrator");

            foreach (var (appId, patterns) in apps)
            {
                if (patterns.Count > 0)
                {
                    var settings = new AppSettings
                    {
                        Patterns = patterns.Values.Select(x => new Pattern(x.Name, x.Pattern)
                        {
                            Message = x.Message
                        }).ToReadOnlyCollection()
                    };

                    await commandBus.PublishAsync(new UpdateAppSettings
                    {
                        AppId = appId,
                        Settings = settings,
                        FromRule = true,
                        Actor = actor
                    });
                }
            }
        }
    }
}
