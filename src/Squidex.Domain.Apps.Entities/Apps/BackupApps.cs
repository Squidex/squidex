﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class BackupApps : BackupHandler
    {
        private const string UsersFile = "Users.json";
        private const string SettingsFile = "Settings.json";
        private readonly IAppUISettings appUISettings;
        private readonly IAppsIndex appsIndex;
        private readonly IUserResolver userResolver;
        private readonly HashSet<string> contributors = new HashSet<string>();
        private readonly Dictionary<string, RefToken> userMapping = new Dictionary<string, RefToken>();
        private Dictionary<string, string> usersWithEmail = new Dictionary<string, string>();
        private string appReservation;
        private string appName;

        public override string Name { get; } = "Apps";

        public BackupApps(IAppUISettings appUISettings, IAppsIndex appsIndex, IUserResolver userResolver)
        {
            Guard.NotNull(appsIndex, nameof(appsIndex));
            Guard.NotNull(appUISettings, nameof(appUISettings));
            Guard.NotNull(userResolver, nameof(userResolver));

            this.appsIndex = appsIndex;
            this.appUISettings = appUISettings;
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

        public override async Task BackupAsync(Guid appId, BackupWriter writer)
        {
            await WriteUsersAsync(writer);
            await WriteSettingsAsync(writer, appId);
        }

        public override async Task<bool> RestoreEventAsync(Envelope<IEvent> @event, Guid appId, BackupReader reader, RefToken actor)
        {
            switch (@event.Payload)
            {
                case AppCreated appCreated:
                    {
                        appName = appCreated.Name;

                        await ResolveUsersAsync(reader);
                        await ReserveAppAsync(appId);

                        break;
                    }

                case AppContributorAssigned contributorAssigned:
                    {
                        if (!userMapping.TryGetValue(contributorAssigned.ContributorId, out var user) || user.Equals(actor))
                        {
                            return false;
                        }

                        contributorAssigned.ContributorId = user.Identifier;
                        contributors.Add(contributorAssigned.ContributorId);
                        break;
                    }

                case AppContributorRemoved contributorRemoved:
                    {
                        if (!userMapping.TryGetValue(contributorRemoved.ContributorId, out var user) || user.Equals(actor))
                        {
                            return false;
                        }

                        contributorRemoved.ContributorId = user.Identifier;
                        contributors.Remove(contributorRemoved.ContributorId);
                        break;
                    }
            }

            if (@event.Payload is SquidexEvent squidexEvent)
            {
                squidexEvent.Actor = MapUser(squidexEvent.Actor.Identifier, actor);
            }

            return true;
        }

        public override Task RestoreAsync(Guid appId, BackupReader reader)
        {
            return ReadSettingsAsync(reader, appId);
        }

        private async Task ReserveAppAsync(Guid appId)
        {
            appReservation = await appsIndex.ReserveAsync(appId, appName);

            if (appReservation == null)
            {
                throw new BackupRestoreException("The app id or name is not available.");
            }
        }

        public override async Task CleanupRestoreErrorAsync(Guid appId)
        {
            await appsIndex.RemoveReservationAsync(appReservation);
        }

        private RefToken MapUser(string userId, RefToken fallback)
        {
            return userMapping.GetOrAdd(userId, fallback);
        }

        private async Task ResolveUsersAsync(BackupReader reader)
        {
            await ReadUsersAsync(reader);

            foreach (var kvp in usersWithEmail)
            {
                var email = kvp.Value;

                var user = await userResolver.FindByIdOrEmailAsync(email);

                if (user == null && await userResolver.CreateUserIfNotExists(kvp.Value))
                {
                    user = await userResolver.FindByIdOrEmailAsync(email);
                }

                if (user != null)
                {
                    userMapping[kvp.Key] = new RefToken(RefTokenType.Subject, user.Id);
                }
            }
        }

        private async Task ReadUsersAsync(BackupReader reader)
        {
            var json = await reader.ReadJsonAttachmentAsync<Dictionary<string, string>>(UsersFile);

            usersWithEmail = json;
        }

        private async Task WriteUsersAsync(BackupWriter writer)
        {
            var json = usersWithEmail;

            await writer.WriteJsonAsync(UsersFile, json);
        }

        private async Task WriteSettingsAsync(BackupWriter writer, Guid appId)
        {
            var json = await appUISettings.GetAsync(appId, null);

            await writer.WriteJsonAsync(SettingsFile, json);
        }

        private async Task ReadSettingsAsync(BackupReader reader, Guid appId)
        {
            var json = await reader.ReadJsonAttachmentAsync<JsonObject>(SettingsFile);

            await appUISettings.SetAsync(appId, null, json);
        }

        public override async Task CompleteRestoreAsync(Guid appId, BackupReader reader)
        {
            await appsIndex.AddAsync(appReservation);

            await appsIndex.RebuildByContributorsAsync(appId, contributors);
        }
    }
}
