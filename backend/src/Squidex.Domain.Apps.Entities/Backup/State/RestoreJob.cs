// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Backup.State
{
    public sealed class RestoreJob : IRestoreJob
    {
        public string AppName { get; set; }

        public DomainId Id { get; set; }

        public NamedId<DomainId> AppId { get; set; }

        public RefToken Actor { get; set; }

        public Uri Url { get; set; }

        public Instant Started { get; set; }

        public Instant? Stopped { get; set; }

        public List<string> Log { get; set; } = new List<string>();

        public JobStatus Status { get; set; }

        public string? NewAppName { get; set; }
    }
}
