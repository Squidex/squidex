// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ScheduleJob
    {
        public Guid Id { get; }

        public Status Status { get; }

        public RefToken ScheduledBy { get; }

        public Instant DueTime { get; }

        public ScheduleJob(Guid id, Status status, RefToken scheduledBy, Instant dueTime)
        {
            Id = id;
            ScheduledBy = scheduledBy;
            Status = status;
            DueTime = dueTime;
        }
    }
}
