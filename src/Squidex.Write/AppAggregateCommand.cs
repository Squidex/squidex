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
    public class AppAggregateCommand : AggregateCommand, IAppCommand
    {
        Guid IAppCommand.AppId
        {
            get
            {
                return AggregateId;
            }
            set
            {
                AggregateId = value;
            }
        }
    }
}
