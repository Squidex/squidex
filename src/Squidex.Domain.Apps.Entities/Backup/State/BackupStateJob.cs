// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.Serialization;
using NodaTime;

namespace Squidex.Domain.Apps.Entities.Backup.State
{
    public sealed class BackupStateJob : IBackupJob
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Instant Started { get; set; }

        [DataMember]
        public Instant? Stopped { get; set; }

        [DataMember]
        public int HandledEvents { get; set; }

        [DataMember]
        public int HandledAssets { get; set; }

        [DataMember]
        public JobStatus Status { get; set; }
    }
}
