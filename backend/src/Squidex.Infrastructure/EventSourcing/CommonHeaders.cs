// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing;

public static class CommonHeaders
{
    public static readonly string AggregateId = nameof(AggregateId);

    public static readonly string CommitId = nameof(CommitId);

    public static readonly string EventId = nameof(EventId);

    public static readonly string EventNumber = nameof(EventNumber);

    public static readonly string EventStreamNumber = nameof(EventStreamNumber);

    public static readonly string Restored = nameof(Restored);

    public static readonly string Timestamp = nameof(Timestamp);
}
