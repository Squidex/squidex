// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using NodaTime;

namespace Squidex.Domain.Apps.Backup.State
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
        public string DownloadPath { get; set; }

        [JsonProperty]
        public bool Failed { get; set; }
    }
}
