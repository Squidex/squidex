// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using NodaTime;

namespace Squidex.Domain.Apps.Entities.Backup.State
{
    public sealed class BackupStateJob : IBackupJob
    {
        [JsonProperty]
        public Guid Id { get; set; }

        [JsonProperty]
        public Instant Started { get; set; }

        [JsonProperty]
        public Instant? Stopped { get; set; }

        [JsonProperty]
        public int HandledEvents { get; set; }

        [JsonProperty]
        public int HandledAssets { get; set; }

        [JsonProperty]
        public JobStatus Status { get; set; }
    }
}
