// ==========================================================================
//  InfrastructureErrors.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.Logging;

namespace Squidex.Infrastructure.GoogleCloud
{
    public class InfrastructureErrors
    {
        public static readonly EventId InvalidatingReceivedFailed = new EventId(40001, "InvalidingReceivedFailed");

        public static readonly EventId InvalidatingPublishedFailed = new EventId(40002, "InvalidatingPublishedFailed");
    }
}
