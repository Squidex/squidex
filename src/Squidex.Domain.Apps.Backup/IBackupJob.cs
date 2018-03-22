// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;

namespace Squidex.Domain.Apps.Backup
{
    public interface IBackupJob
    {
        Guid Id { get; }

        Instant Started { get; }

        Instant? Stopped { get; }

        bool Failed { get; }

        string DownloadPath { get; }
    }
}
