// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Squidex.Domain.Apps.Entities.Backup.State
{
    public sealed class BackupState
    {
        [DataMember]
        public List<BackupStateJob> Jobs { get; } = new List<BackupStateJob>();
    }
}
