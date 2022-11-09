// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Backup.State;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;
using Squidex.Messaging;

namespace Squidex.Domain.Apps.Entities.Backup;

public sealed class BackupService : IBackupService, IDeleter
{
    private readonly SimpleState<BackupRestoreState> restoreState;
    private readonly IPersistenceFactory<BackupState> persistenceFactoryBackup;
    private readonly IMessageBus messaging;

    public BackupService(
        IPersistenceFactory<BackupRestoreState> persistenceFactoryRestore,
        IPersistenceFactory<BackupState> persistenceFactoryBackup,
        IMessageBus messaging)
    {
        this.persistenceFactoryBackup = persistenceFactoryBackup;
        this.messaging = messaging;

        restoreState = new SimpleState<BackupRestoreState>(persistenceFactoryRestore, GetType(), "Default");
    }

    Task IDeleter.DeleteAppAsync(IAppEntity app,
        CancellationToken ct)
    {
        return messaging.PublishAsync(new BackupClear(app.Id), ct: ct);
    }

    public async Task StartBackupAsync(DomainId appId, RefToken actor,
        CancellationToken ct = default)
    {
        var state = await GetStateAsync(appId, ct);

        state.Value.EnsureCanStart();

        await messaging.PublishAsync(new BackupStart(appId, actor), ct: ct);
    }

    public async Task StartRestoreAsync(RefToken actor, Uri url, string? newAppName,
        CancellationToken ct = default)
    {
        await restoreState.LoadAsync(ct);

        restoreState.Value.Job?.EnsureCanStart();

        await messaging.PublishAsync(new BackupRestore(actor, url, newAppName), ct: ct);
    }

    public Task DeleteBackupAsync(DomainId appId, DomainId backupId,
        CancellationToken ct = default)
    {
        return messaging.PublishAsync(new BackupDelete(appId, backupId), ct: ct);
    }

    public async Task<IRestoreJob> GetRestoreAsync(
        CancellationToken ct = default)
    {
        await restoreState.LoadAsync(ct);

        return restoreState.Value.Job ?? new RestoreJob();
    }

    public async Task<List<IBackupJob>> GetBackupsAsync(DomainId appId,
        CancellationToken ct = default)
    {
        var state = await GetStateAsync(appId, ct);

        return state.Value.Jobs.OfType<IBackupJob>().ToList();
    }

    public async Task<IBackupJob?> GetBackupAsync(DomainId appId, DomainId backupId,
        CancellationToken ct = default)
    {
        var state = await GetStateAsync(appId, ct);

        return state.Value.Jobs.Find(x => x.Id == backupId);
    }

    private async Task<SimpleState<BackupState>> GetStateAsync(DomainId appId,
        CancellationToken ct)
    {
        var state = new SimpleState<BackupState>(persistenceFactoryBackup, GetType(), appId);

        await state.LoadAsync(ct);

        return state;
    }
}
