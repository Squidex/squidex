// ==========================================================================
//  ContentCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.CQRS.Commands;

namespace Squidex.Write.Contents.Commands
{
    public abstract class ContentCommand : SchemaCommand, IAggregateCommand
    {
        public Guid ContentId { get; set; }

        Guid IAggregateCommand.AggregateId
        {
            get { return ContentId; }
        }
    }
}
