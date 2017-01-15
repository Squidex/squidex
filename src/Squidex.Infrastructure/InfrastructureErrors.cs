// ==========================================================================
//  InfrastructureErrors.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.Logging;

namespace Squidex.Infrastructure
{
    public class InfrastructureErrors
    {
        public static readonly EventId CommandUnknown = new EventId(20000, "CommandUnknown");

        public static readonly EventId CommandFailed = new EventId(20001, "CommandFailed");

        public static readonly EventId EventHandlingFailed = new EventId(10001, "EventHandlingFailed");

        public static readonly EventId EventDeserializationFailed = new EventId(10002, "EventDeserializationFailed");

        public static readonly EventId ReplayClearingFailed = new EventId(30001, "ReplayClearingFailed");

        public static readonly EventId ReplayPublishingFailed = new EventId(30003, "ReplayPublishingFailed");
    }
}
