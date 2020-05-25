// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class RestoreContext : BackupContextBase
    {
        public IBackupReader Reader { get; }

        public DomainId PreviousAppId { get; set; }

        public RestoreContext(DomainId appId, IUserMapping userMapping, IBackupReader reader, DomainId previousAppId)
            : base(appId, userMapping)
        {
            Guard.NotNull(reader, nameof(reader));

            Reader = reader;

            PreviousAppId = previousAppId;
        }
    }
}
