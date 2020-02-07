// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Entities.Backup.State
{
    public sealed class BackupState
    {
        public List<BackupJob> Jobs { get; } = new List<BackupJob>();
    }
}
