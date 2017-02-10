// ==========================================================================
//  InfrastructureErrors.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.Logging;

namespace Squidex.Infrastructure.Redis
{
    public class InfrastructureErrors
    {
        public static readonly EventId InvalidatingReceivedFailed = new EventId(10001, "InvalidingReceivedFailed");

        public static readonly EventId InvalidatingPublishedFailed = new EventId(10002, "InvalidatingPublishedFailed");
    }
}
