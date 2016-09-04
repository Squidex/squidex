// ==========================================================================
//  TenantCommand.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using PinkParrot.Infrastructure.CQRS.Commands;

namespace PinkParrot.Write
{
    public abstract class TenantCommand : AggregateCommand
    {
        public Guid TenantId { get; set; }
    }
}
