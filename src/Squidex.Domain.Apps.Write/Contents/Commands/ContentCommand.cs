﻿// ==========================================================================
//  ContentCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Security.Claims;
using Squidex.Infrastructure.CQRS.Commands;

namespace Squidex.Domain.Apps.Write.Contents.Commands
{
    public abstract class ContentCommand : SchemaCommand, IAggregateCommand
    {
        public ClaimsPrincipal User { get; set; }

        public Guid ContentId { get; set; }

        Guid IAggregateCommand.AggregateId
        {
            get { return ContentId; }
        }
    }
}
