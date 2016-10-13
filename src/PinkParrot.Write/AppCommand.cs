// ==========================================================================
//  AppCommand.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using PinkParrot.Infrastructure.CQRS.Commands;

namespace PinkParrot.Write
{
    public abstract class AppCommand : AggregateCommand, IAppCommand
    {
        public Guid AppId { get; set; }
    }
}
