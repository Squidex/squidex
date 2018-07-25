// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Backup.State
{
    public sealed class RestoreStateJob : IRestoreJob
    {
        [JsonProperty]
        public Guid Id { get; set; } = Guid.NewGuid();

        [JsonProperty]
        public Guid AppId { get; set; }

        [JsonProperty]
        public RefToken User { get; set; }

        [JsonProperty]
        public Uri Uri { get; set; }

        [JsonProperty]
        public Instant Started { get; set; }

        [JsonProperty]
        public string Status { get; set; }

        [JsonProperty]
        public bool IsFailed { get; set; }
    }
}
