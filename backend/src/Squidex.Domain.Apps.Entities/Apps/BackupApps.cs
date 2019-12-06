// ==========================================================================
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
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class BackupApps : IBackupHandler
    {
        private const string SettingsFile = "Settings.json";
        private readonly IAppUISettings appUISettings;
        private readonly IAppsIndex appsIndex;
        private readonly HashSet<string> contributors = new HashSet<string>();
        private string? appReservation;
        private string appName;

        public string Name { get; } = "Apps";

        public BackupApps(IAppUISettings appUISettings, IAppsIndex appsIndex)
        {
            Guard.NotNull(appsIndex);
            Guard.NotNull(appUISettings);

            this.appsIndex = appsIndex;
            this.appUISettings = appUISettings;
        }

        public Task BackupEventAsync(Envelope<IEvent> @event, BackupContext context)
        {
            if (@event.Payload is AppContributorAssigned appContributorAssigned)
            {
                context.UserMapping.Backup(appContributorAssigned.ContributorId);
            }

            return TaskHelper.Done;
        }

        public async Task BackupAsync(Guid appId, BackupContext writer)
        {
            await WriteSettingsAsync(writer, appId);
        }

        public async Task<bool> RestoreEventAsync(Envelope<IEvent> @event, RestoreContext context)
        {
            switch (@event.Payload)
            {
                case AppCreated appCreated:
                    {
                        appName = appCreated.Name;

                        await ReserveAppAsync(context.AppId);

                        break;
                    }

                case AppContributorAssigned contributorAssigned:
                    {
                        if (!context.UserMapping.TryMap(contributorAssigned.ContributorId, out var user) || user.Equals(context.Initiator))
                        {
                            return false;
                        }

                        contributorAssigned.ContributorId = user.Identifier;
                        contributors.Add(contributorAssigned.ContributorId);
                        break;
                    }

                case AppContributorRemoved contributorRemoved:
                    {
                        if (!context.UserMapping.TryMap(contributorRemoved.ContributorId, out var user) || user.Equals(context.Initiator))
                        {
                            return false;
                        }

                        contributorRemoved.ContributorId = user.Identifier;
                        contributors.Remove(contributorRemoved.ContributorId);
                        break;
                    }
            }

            return true;
        }

        public Task RestoreAsync(Guid appId, RestoreContext context)
        {
            return ReadSettingsAsync(context, appId);
        }

        private async Task ReserveAppAsync(Guid appId)
        {
            appReservation = await appsIndex.ReserveAsync(appId, appName);

            if (appReservation == null)
            {
                throw new BackupRestoreException("The app id or name is not available.");
            }
        }

        public async Task CleanupRestoreErrorAsync(Guid appId)
        {
            if (appReservation != null)
            {
                await appsIndex.RemoveReservationAsync(appReservation);
            }
        }

        private async Task WriteSettingsAsync(BackupContext context, Guid appId)
        {
            var json = await appUISettings.GetAsync(appId, null);

            await context.Writer.WriteJsonAsync(SettingsFile, json);
        }

        private async Task ReadSettingsAsync(RestoreContext context, Guid appId)
        {
            var json = await context.Reader.ReadJsonAttachmentAsync<JsonObject>(SettingsFile);

            await appUISettings.SetAsync(appId, null, json);
        }

        public async Task CompleteRestoreAsync(RestoreContext context)
        {
            await appsIndex.AddAsync(appReservation);

            await appsIndex.RebuildByContributorsAsync(context.AppId, contributors);
        }
    }
}
