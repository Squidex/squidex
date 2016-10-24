// ==========================================================================
//  IAggregateCommand.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Infrastructure.CQRS.Commands
{
    public interface IAggregateCommand : ICommand
    {
        Guid AggregateId { get; set; }
    }
}