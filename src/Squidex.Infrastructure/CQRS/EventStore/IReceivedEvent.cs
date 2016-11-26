// ==========================================================================
//  IReceivedEvent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.CQRS.EventStore
{
    public interface IReceivedEvent
    {
        int EventNumber { get; }

        string EventType { get; }

        byte[] Metadata { get; }

        byte[] Payload { get; }

        DateTime Created { get; }
    }
}
