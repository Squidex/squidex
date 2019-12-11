﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class BackupContext : BackupContextBase
    {
        public IBackupWriter Writer { get; }

        public BackupContext(Guid appId, IUserMapping userMapping, IBackupWriter writer)
            : base(appId, userMapping)
        {
            Guard.NotNull(writer);

            Writer = writer;
        }
    }
}
