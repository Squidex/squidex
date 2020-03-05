// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Entities.Apps.Plans
{
    public sealed class UsageNotification
    {
        public Guid AppId { get; set; }

        public string AppName { get; set; }

        public long Usage { get; set; }

        public long UsageLimit { get; set; }

        public string[] Users { get; set; }
    }
}
