// ==========================================================================
//  RedisInfrastructureErrors.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.Logging;

namespace Squidex.Infrastructure.Redis
{
    public static class RedisInfrastructureErrors
    {
        public static readonly EventId InvalidatingReceivedFailed = new EventId(50001, "InvalidingReceivedFailed");

        public static readonly EventId InvalidatingPublishedFailed = new EventId(50002, "InvalidatingPublishedFailed");
    }
}
