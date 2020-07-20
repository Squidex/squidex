// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Backup.State
{
    public sealed class BackupJob : IBackupJob
    {
        public DomainId Id { get; set; }

        public Instant Started { get; set; }

        public Instant? Stopped { get; set; }

        public int HandledEvents { get; set; }

        public int HandledAssets { get; set; }

        public JobStatus Status { get; set; }
    }
}
