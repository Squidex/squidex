// ==========================================================================
//  AppAggregateCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Write
{
    public class AppAggregateCommand : SquidexCommand, IAppCommand
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
