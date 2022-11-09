// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.Backup.State;

public sealed class BackupState
{
    public List<BackupJob> Jobs { get; set;  } = new List<BackupJob>();

    public void EnsureCanStart()
    {
        if (Jobs.Any(x => x.Status == JobStatus.Started))
        {
            throw new DomainException(T.Get("backups.alreadyRunning"));
        }

        if (Jobs.Count >= 10)
        {
            throw new DomainException(T.Get("backups.maxReached", new { max = 10 }));
        }
    }
}
