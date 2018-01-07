// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;
using Squidex.Infrastructure.Commands;

namespace Squidex.Infrastructure.TestHelpers
{
    internal sealed class MyCommand : IAggregateCommand, ITimestampCommand
    {
        public Guid AggregateId { get; set; }

        public long ExpectedVersion { get; set; }

        public Instant Timestamp { get; set; }
    }
}
