// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Squidex.Domain.Apps.Backup.State
{
    public sealed class BackupState
    {
        [JsonProperty]
        public List<BackupStateJob> Jobs { get; set; } = new List<BackupStateJob>();
    }
}
