// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure.Commands;

namespace Squidex.Infrastructure.TestHelpers
{
    public class MyCommand : IAggregateCommand, ITimestampCommand
    {
        public DomainId AggregateId { get; set; }

        public long ExpectedVersion { get; set; } = EtagVersion.Any;

        public Instant Timestamp { get; set; }
    }
}
