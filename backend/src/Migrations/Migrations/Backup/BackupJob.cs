// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure;

namespace Migrations.Migrations.Backup;

public sealed class BackupJob
{
    public DomainId Id { get; set; }

    public Instant Started { get; set; }

    public Instant? Stopped { get; set; }

    public int HandledEvents { get; set; }

    public int HandledAssets { get; set; }

    public BackupStatus Status { get; set; }
}
