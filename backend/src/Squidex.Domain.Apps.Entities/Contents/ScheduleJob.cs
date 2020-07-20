// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ScheduleJob
    {
        public DomainId Id { get; }

        public Instant DueTime { get; }

        public Status Status { get; }

        public RefToken ScheduledBy { get; }

        public ScheduleJob(DomainId id, Status status, RefToken scheduledBy, Instant dueTime)
        {
            Id = id;
            ScheduledBy = scheduledBy;
            Status = status;
            DueTime = dueTime;
        }

        public static ScheduleJob Build(Status status, RefToken scheduledBy, Instant dueTime)
        {
            return new ScheduleJob(DomainId.NewGuid(), status, scheduledBy, dueTime);
        }
    }
}
