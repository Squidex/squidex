// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.DomainObject;
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Apps;

public sealed class BackupApps : IBackupHandler
{
    private const string SettingsFile = "Settings.json";
    private const string AvatarFile = "Avatar.image";
    private readonly Rebuilder rebuilder;
    private readonly IAppImageStore appImageStore;
    private readonly IAppProvider appProvider;
    private readonly IAppsIndex appsIndex;
    private readonly IAppUISettings appUISettings;
    private AppDomainObject? app;
    private string? appReservation;

    public string Name { get; } = "Apps";

    public BackupApps(
        Rebuilder rebuilder,
        IAppImageStore appImageStore,
        IAppProvider appProvider,
        IAppsIndex appsIndex,
        IAppUISettings appUISettings)
    {
        this.appsIndex = appsIndex;
        this.appImageStore = appImageStore;
        this.appProvider = appProvider;
        this.appUISettings = appUISettings;
        this.rebuilder = rebuilder;
    }

    public async Task BackupEventAsync(Envelope<IEvent> @event, BackupContext context,
        CancellationToken ct)
    {
        switch (@event.Payload)
        {
            case AppContributorAssigned appContributorAssigned:
                context.UserMapping.Backup(appContributorAssigned.ContributorId);
                break;
            case AppImageUploaded:
                await WriteAssetAsync(context.AppId, context.Writer, ct);
                break;
        }
    }

    public async Task BackupAsync(BackupContext context,
        CancellationToken ct)
    {
        var json = await appUISettings.GetAsync(context.AppId, null, ct);

        await context.Writer.WriteJsonAsync(SettingsFile, json, ct);
    }

    public async Task<bool> RestoreEventAsync(Envelope<IEvent> @event, RestoreContext context,
        CancellationToken ct)
    {
        switch (@event.Payload)
        {
            case AppCreated appCreated:
                await ReserveAppAsync(context.AppId, appCreated.Name, ct);
                break;

            case AppImageUploaded:
                await ReadAssetAsync(context.AppId, context.Reader, ct);
                break;

            case AppContributorAssigned contributorAssigned:
                {
                    if (!context.UserMapping.TryMap(contributorAssigned.ContributorId, out var user) || user.Equals(context.Initiator))
                    {
                        return false;
                    }

                    contributorAssigned.ContributorId = user.Identifier;
                    break;
                }

            case AppContributorRemoved contributorRemoved:
                {
                    if (!context.UserMapping.TryMap(contributorRemoved.ContributorId, out var user) || user.Equals(context.Initiator))
                    {
                        return false;
                    }

                    contributorRemoved.ContributorId = user.Identifier;
                    break;
                }
        }

        return true;
    }

    public async Task RestoreAsync(RestoreContext context,
        CancellationToken ct)
    {
        await RestoreAppTemporarilyAsync(context, ct);
        await RestoreSettingsAsync(context, ct);
    }

    private async Task RestoreAppTemporarilyAsync(RestoreContext context,
        CancellationToken ct)
    {
        app = await rebuilder.RebuildStateAsync<AppDomainObject, App>(context.AppId, ct);

        // The app is only available in the context of the restore, so there is nothing to clear.
        appProvider.RegisterAppForLocalContext(context.AppId, app!.Snapshot);
    }

    private async Task RestoreSettingsAsync(RestoreContext context, CancellationToken ct)
    {
        var json = await context.Reader.ReadJsonAsync<JsonObject>(SettingsFile, ct);

        await appUISettings.SetAsync(context.AppId, null, json, ct);
    }

    private async Task ReserveAppAsync(DomainId appId, string appName,
        CancellationToken ct)
    {
        appReservation = await appsIndex.ReserveAsync(appId, appName, ct);

        if (appReservation == null)
        {
            throw new BackupRestoreException("The app id or name is not available.");
        }
    }

    public async Task CleanupRestoreErrorAsync(DomainId appId)
    {
        await appsIndex.RemoveReservationAsync(appReservation);
    }

    public async Task CompleteRestoreAsync(RestoreContext context, string appName)
    {
        if (app != null)
        {
            await app.RebuildStateAsync();
        }

        await appsIndex.RemoveReservationAsync(appReservation);
    }

    private async Task WriteAssetAsync(DomainId appId, IBackupWriter writer,
        CancellationToken ct)
    {
        try
        {
            // Ignore errors when writing assets, because they are not that relevant.
            await using (var stream = await writer.OpenBlobAsync(AvatarFile, ct))
            {
                await appImageStore.DownloadAsync(appId, stream, ct);
            }
        }
        catch (AssetNotFoundException)
        {
        }
    }

    private async Task ReadAssetAsync(DomainId appId, IBackupReader reader,
        CancellationToken ct)
    {
        try
        {
            // Ignore errors when writing assets, because they are not that relevant.
            await using (var stream = await reader.OpenBlobAsync(AvatarFile, ct))
            {
                await appImageStore.UploadAsync(appId, stream, ct);
            }
        }
        catch (AssetAlreadyExistsException)
        {
        }
        catch (FileNotFoundException)
        {
        }
    }
}
