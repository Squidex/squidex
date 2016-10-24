// ==========================================================================
//  CommonHeaders.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
namespace Squidex.Infrastructure.CQRS
{
    public sealed class CommonHeaders
    {
        public const string AggregateId = "AggregateId";
        public const string CommitId = "CommitId";
        public const string Timestamp = "Timestamp";
        public const string AppId = "AppId";
        public const string EventId = "EventId";
        public const string EventNumber = "EventNumber";
    }
}
