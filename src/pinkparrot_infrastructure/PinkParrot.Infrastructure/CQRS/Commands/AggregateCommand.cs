// ==========================================================================
//  AggregateCommand.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Infrastructure.CQRS.Commands
{
    public class AggregateCommand : ICommand
    {
        public Guid AggregateId { get; set; }
    }
}
