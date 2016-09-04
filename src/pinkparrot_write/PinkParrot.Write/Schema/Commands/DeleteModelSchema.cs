// ==========================================================================
//  DeleteModelSchema.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using PinkParrot.Infrastructure.CQRS.Commands;

namespace PinkParrot.Write.Schema.Commands
{
    public class DeleteModelSchema : IAggregateCommand
    {
        public Guid AggregateId { get; set; }
    }
}