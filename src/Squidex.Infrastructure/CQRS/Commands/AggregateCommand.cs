// ==========================================================================
//  AggregateCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public class AggregateCommand : IAggregateCommand
    {
        public Guid AggregateId { get; set; }
    }
}
