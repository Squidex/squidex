// ==========================================================================
//  AppCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.CQRS.Commands;

namespace Squidex.Write
{
    public abstract class AppCommand : AggregateCommand, IAppCommand
    {
        public Guid AppId { get; set; }
    }
}
