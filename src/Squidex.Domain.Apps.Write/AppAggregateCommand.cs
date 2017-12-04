﻿// ==========================================================================
//  AppAggregateCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Write
{
    public class AppAggregateCommand : AppCommand, IAggregateCommand
    {
        Guid IAggregateCommand.AggregateId
        {
            get { return AppId.Id; }
        }
    }
}
