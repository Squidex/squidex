// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.Contents;

public sealed record ScheduleJob(DomainId Id, Status Status, RefToken ScheduledBy, Instant DueTime)
{
    public static ScheduleJob Build(Status status, RefToken scheduledBy, Instant dueTime)
    {
        return new ScheduleJob(DomainId.NewGuid(), status, scheduledBy, dueTime);
    }
}
