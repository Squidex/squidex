// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using NodaTime;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public interface IRestoreJob
    {
        Uri Url { get; }

        Instant Started { get; }

        Instant? Stopped { get; }

        List<string> Log { get; }

        JobStatus Status { get; }
    }
}
