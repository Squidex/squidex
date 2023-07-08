// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Backup;

public interface IBackupHandler
{
    string Name { get; }

    int Order => 0;

    public Task<bool> RestoreEventAsync(Envelope<IEvent> @event, RestoreContext context,
        CancellationToken ct)
    {
        return Task.FromResult(true);
    }

    public Task BackupEventAsync(Envelope<IEvent> @event, BackupContext context,
        CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    public Task RestoreAsync(RestoreContext context,
        CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    public Task BackupAsync(BackupContext context,
        CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    public Task CleanupRestoreErrorAsync(DomainId appId)
    {
        return Task.CompletedTask;
    }

    public Task CompleteRestoreAsync(RestoreContext context, string appName)
    {
        return Task.CompletedTask;
    }

    public Task CompleteBackupAsync(BackupContext context)
    {
        return Task.CompletedTask;
    }
}
