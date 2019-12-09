// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public interface IBackupHandler
    {
        string Name { get; }

        public Task<bool> RestoreEventAsync(Envelope<IEvent> @event, RestoreContext context)
        {
            return TaskHelper.True;
        }

        public Task BackupEventAsync(Envelope<IEvent> @event, BackupContext context)
        {
            return TaskHelper.Done;
        }

        public Task RestoreAsync(RestoreContext context)
        {
            return TaskHelper.Done;
        }

        public Task BackupAsync(BackupContext context)
        {
            return TaskHelper.Done;
        }

        public Task CleanupRestoreErrorAsync(Guid appId)
        {
            return TaskHelper.Done;
        }

        public Task CompleteRestoreAsync(RestoreContext context)
        {
            return TaskHelper.Done;
        }

        public Task CompleteBackupAsync(BackupContext context)
        {
            return TaskHelper.Done;
        }
    }
}
