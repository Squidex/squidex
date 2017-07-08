// ==========================================================================
//  AppAggregateCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.CQRS.Commands;

namespace Squidex.Write
{
    public class AppAggregateCommand : AppCommand, IAggregateCommand
    {
        Guid IAggregateCommand.AggregateId
        {
            get { return AppId.Id; }
        }
    }
}
