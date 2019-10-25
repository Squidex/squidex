﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public interface IBackupJob
    {
        Guid Id { get; }

        Instant Started { get; }

        Instant? Stopped { get; }

        int HandledEvents { get; }

        int HandledAssets { get; }

        JobStatus Status { get; }
    }
}
