// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NodaTime;

namespace Squidex.Domain.Apps.Entities.Backup.State
{
    public sealed class RestoreStateJob : IRestoreJob
    {
        [JsonProperty]
        public string AppName { get; set; }

        [JsonProperty]
        public Guid Id { get; set; }

        [JsonProperty]
        public Guid AppId { get; set; }

        [JsonProperty]
        public Uri Url { get; set; }

        [JsonProperty]
        public string NewAppName { get; set; }

        [JsonProperty]
        public Instant Started { get; set; }

        [JsonProperty]
        public Instant? Stopped { get; set; }

        [JsonProperty]
        public List<string> Log { get; set; } = new List<string>();

        [JsonProperty]
        public JobStatus Status { get; set; }
    }
}
