// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Orleans;
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class BackupApps : BackupHandlerWithStore
    {
        private const string UsersFile = "Users.json";
        private readonly IGrainFactory grainFactory;
        private readonly IUserResolver userResolver;
        private readonly HashSet<string> activeUsers = new HashSet<string>();
        private Dictionary<string, string> usersWithEmail = new Dictionary<string, string>();
        private Dictionary<string, RefToken> userMapping = new Dictionary<string, RefToken>();
        private bool isReserved;
        private bool isActorAssigned;
        private AppCreated appCreated;

        public override string Name { get; } = "Apps";

        public BackupApps(IStore<Guid> store, IGrainFactory grainFactory, IUserResolver userResolver)
            : base(store)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));
            Guard.NotNull(userResolver, nameof(userResolver));

            this.grainFactory = grainFactory;

            this.userResolver = userResolver;
        }

        public override async Task BackupEventAsync(Envelope<IEvent> @event, Guid appId, BackupWriter writer)
        {
            if (@event.Payload is AppContributorAssigned appContributorAssigned)
            {
                var userId = appContributorAssigned.ContributorId;

                if (!usersWithEmail.ContainsKey(userId))
                {
                    var user = await userResolver.FindByIdOrEmailAsync(userId);

                    if (user != null)
                    {
                        usersWithEmail.Add(userId, user.Email);
                    }
                }
            }
        }

        public override Task BackupAsync(Guid appId, BackupWriter writer)
        {
            return WriterUsersAsync(writer);
        }

        public async override Task RestoreEventAsync(Envelope<IEvent> @event, Guid appId, BackupReader reader, RefToken actor)
        {
            switch (@event.Payload)
            {
                case AppCreated appCreated:
                    {
                        this.appCreated = appCreated;

                        await ResolveUsersAsync(reader, actor);
                        await ReserveAppAsync();

                        break;
                    }

                case AppContributorAssigned contributorAssigned:
                    {
                        if (isActorAssigned)
                        {
                            contributorAssigned.ContributorId = MapUser(contributorAssigned.ContributorId, actor).Identifier;
                        }
                        else
                        {
                            isActorAssigned = true;

                            contributorAssigned.ContributorId = actor.Identifier;
                        }

                        activeUsers.Add(contributorAssigned.ContributorId);
                        break;
                    }

                case AppContributorRemoved contributorRemoved:
                    {
                        contributorRemoved.ContributorId = MapUser(contributorRemoved.ContributorId, actor).Identifier;

                        activeUsers.Remove(contributorRemoved.ContributorId);
                        break;
                    }
            }

            if (@event.Payload is SquidexEvent squidexEvent)
            {
                squidexEvent.Actor = MapUser(squidexEvent.Actor.Identifier, actor);
            }
        }

        private async Task ReserveAppAsync()
        {
            var index = grainFactory.GetGrain<IAppsByNameIndex>(SingleGrain.Id);

            if (!(isReserved = await index.ReserveAppAsync(appCreated.AppId.Id, appCreated.AppId.Name)))
            {
                throw new BackupRestoreException("The app id or name is not available.");
            }
        }

        private RefToken MapUser(string userId, RefToken fallback)
        {
            return userMapping.GetOrAdd(userId, fallback);
        }

        private async Task ResolveUsersAsync(BackupReader reader, RefToken actor)
        {
            await ReadUsersAsync(reader);

            foreach (var kvp in usersWithEmail)
            {
                var user = await userResolver.FindByIdOrEmailAsync(kvp.Value);

                if (user != null)
                {
                    userMapping[kvp.Key] = new RefToken(RefTokenType.Subject, user.Id);
                }
                else
                {
                    userMapping[kvp.Key] = actor;
                }
            }
        }

        private async Task ReadUsersAsync(BackupReader reader)
        {
            var json = await reader.ReadJsonAttachmentAsync(UsersFile);

            usersWithEmail = json.ToObject<Dictionary<string, string>>();
        }

        private Task WriterUsersAsync(BackupWriter writer)
        {
            var json = JObject.FromObject(usersWithEmail);

            return writer.WriteJsonAsync(UsersFile, json);
        }

        public override async Task CompleteRestoreAsync(Guid appId, BackupReader reader)
        {
            await grainFactory.GetGrain<IAppsByNameIndex>(SingleGrain.Id).AddAppAsync(appCreated.AppId.Id, appCreated.AppId.Name);

            foreach (var user in activeUsers)
            {
                await grainFactory.GetGrain<IAppsByUserIndex>(user).AddAppAsync(appCreated.AppId.Id);
            }
        }

        public override async Task CleanupRestoreAsync(Guid appId)
        {
            if (isReserved)
            {
                var index = grainFactory.GetGrain<IAppsByNameIndex>(SingleGrain.Id);

                await index.ReserveAppAsync(appCreated.AppId.Id, appCreated.AppId.Name);
            }
        }
    }
}
