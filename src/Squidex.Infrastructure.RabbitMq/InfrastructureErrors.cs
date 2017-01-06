// ==========================================================================
//  InfrastructureErrors.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.Logging;

namespace Squidex.Infrastructure.RabbitMq
{
    public class InfrastructureErrors
    {
        public static readonly EventId EventHandlingFailed = new EventId(10001, "EventHandlingFailed");

        public static readonly EventId EventDeserializationFailed = new EventId(10002, "EventDeserializationFailed");
    }
}
