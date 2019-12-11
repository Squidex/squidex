// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class RestoreContext : BackupContextBase
    {
        public IBackupReader Reader { get; }

        public RestoreContext(Guid appId, IUserMapping userMapping, IBackupReader reader)
            : base(appId, userMapping)
        {
            Guard.NotNull(reader);

            Reader = reader;
        }
    }
}
