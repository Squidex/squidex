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
    public abstract class BackupContextBase
    {
        public UserMapping UserMapping { get; }

        public Guid AppId { get; set; }

        public RefToken Initiator
        {
            get { return UserMapping.Initiator; }
        }

        protected BackupContextBase(Guid appId, UserMapping userMapping)
        {
            Guard.NotNull(userMapping);

            AppId = appId;

            UserMapping = userMapping;
        }
    }
}
