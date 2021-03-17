// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public abstract class BackupContextBase
    {
        public IUserMapping UserMapping { get; }

        public DomainId AppId { get; set; }

        public RefToken Initiator
        {
            get => UserMapping.Initiator;
        }

        protected BackupContextBase(DomainId appId, IUserMapping userMapping)
        {
            Guard.NotNull(userMapping, nameof(userMapping));

            AppId = appId;

            UserMapping = userMapping;
        }
    }
}
