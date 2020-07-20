// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class BackupContext : BackupContextBase
    {
        public IBackupWriter Writer { get; }

        public BackupContext(DomainId appId, IUserMapping userMapping, IBackupWriter writer)
            : base(appId, userMapping)
        {
            Guard.NotNull(writer, nameof(writer));

            Writer = writer;
        }
    }
}
