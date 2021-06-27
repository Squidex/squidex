// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Squidex.Assets;
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class BackupApps : IBackupHandler
    {
        private const string SettingsFile = "Settings.json";
        private const string AvatarFile = "Avatar.image";
        private readonly IAppImageStore appImageStore;
        private readonly IAppsIndex appsIndex;
        private readonly IAppUISettings appUISettings;
        private readonly HashSet<string> contributors = new HashSet<string>();
        private string? appReservation;

        public string Name { get; } = "Apps";

        public BackupApps(IAppImageStore appImageStore, IAppsIndex appsIndex, IAppUISettings appUISettings)
        {
            this.appsIndex = appsIndex;
            this.appImageStore = appImageStore;
            this.appUISettings = appUISettings;
        }

        public async Task BackupEventAsync(Envelope<IEvent> @event, BackupContext context)
        {
            switch (@event.Payload)
            {
                case AppContributorAssigned appContributorAssigned:
                    context.UserMapping.Backup(appContributorAssigned.ContributorId);
                    break;
                case AppImageUploaded:
                    await WriteAssetAsync(context.AppId, context.Writer);
                    break;
            }
        }

        public async Task BackupAsync(BackupContext context)
        {
            var json = await appUISettings.GetAsync(context.AppId, null);

            await context.Writer.WriteJsonAsync(SettingsFile, json);
        }

        public async Task<bool> RestoreEventAsync(Envelope<IEvent> @event, RestoreContext context)
        {
            switch (@event.Payload)
            {
                case AppCreated appCreated:
                    {
                        await ReserveAppAsync(context.AppId, appCreated.Name);

                        break;
                    }

                case AppImageUploaded:
                    {
                        await ReadAssetAsync(context.AppId, context.Reader);

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

        public async Task RestoreAsync(RestoreContext context)
        {
            var json = await context.Reader.ReadJsonAsync<JsonObject>(SettingsFile);

            await appUISettings.SetAsync(context.AppId, null, json);
        }

        private async Task ReserveAppAsync(DomainId appId, string appName)
        {
            appReservation = await appsIndex.ReserveAsync(appId, appName);

            if (appReservation == null)
            {
                throw new BackupRestoreException("The app id or name is not available.");
            }
        }

        public async Task CleanupRestoreErrorAsync(DomainId appId)
        {
            if (appReservation != null)
            {
                await appsIndex.RemoveReservationAsync(appReservation);
            }
        }

        public async Task CompleteRestoreAsync(RestoreContext context)
        {
            await appsIndex.AddAsync(appReservation);
            await appsIndex.RebuildByContributorsAsync(context.AppId, contributors);
        }

        private Task WriteAssetAsync(DomainId appId, IBackupWriter writer)
        {
            return writer.WriteBlobAsync(AvatarFile, async stream =>
            {
                try
                {
                    await appImageStore.DownloadAsync(appId, stream);
                }
                catch (AssetNotFoundException)
                {
                }
            });
        }

        private async Task ReadAssetAsync(DomainId appId, IBackupReader reader)
        {
            try
            {
                await reader.ReadBlobAsync(AvatarFile, async stream =>
                {
                    try
                    {
                        await appImageStore.UploadAsync(appId, stream);
                    }
                    catch (AssetAlreadyExistsException)
                    {
                    }
                });
            }
            catch (FileNotFoundException)
            {
            }
        }
    }
}
