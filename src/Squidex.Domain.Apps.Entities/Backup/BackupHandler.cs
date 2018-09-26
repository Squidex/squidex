// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public abstract class BackupHandler
    {
        public abstract string Name { get; }

        public virtual Task<bool> RestoreEventAsync(Envelope<IEvent> @event, Guid appId, BackupReader reader, RefToken actor)
        {
            return TaskHelper.True;
        }

        public virtual Task BackupEventAsync(Envelope<IEvent> @event, Guid appId, BackupWriter writer)
        {
            return TaskHelper.Done;
        }

        public virtual Task RestoreAsync(Guid appId, BackupReader reader)
        {
            return TaskHelper.Done;
        }

        public virtual Task BackupAsync(Guid appId, BackupWriter writer)
        {
            return TaskHelper.Done;
        }

        public virtual Task CleanupRestoreAsync(Guid appId)
        {
            return TaskHelper.Done;
        }

        public virtual Task CompleteRestoreAsync(Guid appId, BackupReader reader)
        {
            return TaskHelper.Done;
        }

        public virtual Task CompleteBackupAsync(Guid appId, BackupWriter writer)
        {
            return TaskHelper.Done;
        }
    }
}
