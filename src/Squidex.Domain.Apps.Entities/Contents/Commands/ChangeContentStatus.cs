// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// =========================================================================

using System;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Entities.Contents.Commands
{
    public sealed class ChangeContentStatus : ContentCommand
    {
        public Status Status { get; set; }

        public Instant? DueTime { get; set; }

        public Guid? JobId { get; set; }
    }
}
