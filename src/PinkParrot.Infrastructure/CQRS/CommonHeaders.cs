// ==========================================================================
//  CommonHeaders.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================
namespace PinkParrot.Infrastructure.CQRS
{
    public sealed class CommonHeaders
    {
        public const string AggregateId = "AggregateId";
        public const string CommitId = "CommitId";
        public const string Timestamp = "Timestamp";
        public const string TenantId = "TenantId";
        public const string EventId = "EventId";
        public const string EventNumber = "EventNumber";
    }
}
