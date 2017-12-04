// ==========================================================================
//  MyCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using NodaTime;

namespace Squidex.Infrastructure.Commands.TestHelpers
{
    internal sealed class MyCommand : IAggregateCommand, ITimestampCommand
    {
        public Guid AggregateId { get; set; }

        public long? ExpectedVersion { get; set; }

        public Instant Timestamp { get; set; }
    }
}
